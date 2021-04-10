using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Timers;
using Box_stock.Data_models;
using Box_stock.Data_structures;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace Box_stock
{
    public class Manager : IDisposable
    {
        System.Threading.Timer _timer;
        ConfigurationParameters _configurationParameters;

        BTS<X_Data> _baseTree = new BTS<X_Data>();
        DoubleLinkList<TimeData> _listByTime = new DoubleLinkList<TimeData>();
        ICommunicate _comunicate;
        List<BoxCategory> _potentialItems;
        bool _lock = false;
        int _sumOfPotentialQuantities;
        bool _exactMatchSizes;
        bool _splitsPreference;

        /// <summary>
        /// initialize the Configuration Parameters, timer and other data
        /// </summary>
        /// <param name="constConfigurationParameters"></param>
        /// <param name="myInterface">Interface</param>
        public Manager(ConfigurationParameters constConfigurationParameters, ICommunicate Comunicate)
        {
            _configurationParameters = constConfigurationParameters;
            InitData();
            _comunicate = Comunicate;

            DateTime tomorrow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).AddDays(1);
            TimeSpan due = tomorrow.Subtract(DateTime.Now);
            TimeSpan period = new TimeSpan(_configurationParameters.CheckFrequency, 0, 0, 0);

            _timer = new System.Threading.Timer(DeletionEventHandler, null, due, period);
        }

        /// <summary>
        /// avoiding the event to appear till the user choose an answer when he want's to buy
        /// </summary>
        private async void HoldForLock()
        {
            while (_lock) Thread.Sleep(100);
        }

        /// <summary>
        /// delete event when the time to check arrives. Delete all the elements that their expiration date isn't valid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeletionEventHandler(object parameter)
        {
            Task.WaitAll(Task.Run(HoldForLock));

            while (_listByTime.Start != null && DateTime.Now > _listByTime.Start._data._lastPurchaseDate.
                AddDays(_configurationParameters.LifeTime))
            {
                _listByTime.RemoveFirst(out TimeData dataToDel);  //delete from the link list (of times)               
                BoxCategory currentBox = new BoxCategory(new Y_Data(dataToDel.Height), dataToDel.Base);
                _comunicate.ExpirationDateMessage(dataToDel.Base, dataToDel.Height);                                                                                        //  exact position of the real yData
                Delete(currentBox, false); //delete element                  
            }
        }

        /// <summary>
        /// Insert new items or add more if category exist 
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">height</param>
        /// <param name="count">quantity</param>
        /// <returns>A massage to the user about the order details</returns>
        public string Insert(double x, double y, int count)
        {
            if (count <= 0 || x <= 0 || y <= 0) throw new Exception("Action failed!\nQuantity & sizes must be a positive number.\n");
            X_Data xRoot;
            Y_Data yRoot;
            Y_Data yData = new Y_Data(y, count);
            X_Data xData = new X_Data(x, yData);

            string addDetails = $"boxes have beed added to the category {x} X {y}.\n"; // add to exist
            string insertDetails = $"items of a new category ({x} X {y}) were inserted to the system.\n"; // insert a new category
            string extraMeassage = "extra boxes have been returned to the factory.\n"; // insert more than the capacity accepted
            int extraQuantity;

            if (_baseTree.InsertIfNotFinding(xData, out xRoot)) //xRoot isn't null
            {
                if (xRoot.HTree.InsertIfNotFinding(yData, out yRoot)) // yRoot isn't null- this box is exist. add the count
                {
                    if (yRoot.Quantity == _configurationParameters.MaxQuantity) return $"\nCategory is full.\n{count} {extraMeassage}";

                    else if (yRoot.Quantity + count > _configurationParameters.MaxQuantity)
                    {
                        int leftToMax = _configurationParameters.MaxQuantity - yRoot.Quantity;
                        extraQuantity = count - leftToMax;
                        yRoot.Quantity = _configurationParameters.MaxQuantity;
                        return $"\n{leftToMax} {addDetails}{extraQuantity} {extraMeassage}";
                    }

                    yRoot.Quantity += count;
                    return $"\n{count} {addDetails}";
                }
            }

            _listByTime.AddLast(new TimeData(x, y));
            yData.timeRef = _listByTime.End;
            if (count > _configurationParameters.MaxQuantity)
            {
                extraQuantity = count - _configurationParameters.MaxQuantity;
                yData.Quantity = _configurationParameters.MaxQuantity;
                return $"\n{_configurationParameters.MaxQuantity} {insertDetails}{extraQuantity} {extraMeassage}";
            }
            return $"\n{yData.Quantity} {insertDetails}";
        }

        /// <summary>
        /// get details of a given category (if it's in the system) 
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">height</param>
        /// <returns>data of a category if found</returns>
        public string GetData(double x, double y)
        {
            if (x <= 0 || y <= 0) throw new Exception("Action failed!\nSizes must be a positive number.\n");

            X_Data xRoot;
            X_Data xData = new X_Data(x);
            if (_baseTree.Search(xData, out xRoot))
            {
                Y_Data yData = new Y_Data(y);
                Y_Data yRoot;
                if (xRoot.HTree.Search(yData, out yRoot))
                {
                    return $"\nBox Details:\n\nBase area: {x} m^2\nHeight: {y} m\nVolume: {x * y} m^3\n" +
                        $"Quantity in stock: {yRoot.Quantity} items\nLast date purchase: {yRoot.timeRef._data._lastPurchaseDate}\n";
                }
            }
            return "\nThe current box wasn't found\n";
        }

        public bool IsStoreEmpty() => !_baseTree.IsrootExist();

        /// <summary>
        /// when match is confirmed, display massages to the user acording to the quantity that left (alert if below the const alert
        /// parameter or delete if the quantity reaches to 0)
        /// </summary>
        /// <param name="currentQuantity">quantity left</param>
        /// <param name="x">base</param>
        /// <param name="y">height</param>
        /// <returns>systems operations acording to the quantity left</returns>
        private void SuccessfulReduceOperation(BoxCategory boxCategory)
        {
            if (boxCategory.YData.Quantity == 0)
            {
                _comunicate.RunOutMassage(boxCategory.Base, boxCategory.YData.Height);
                Delete(boxCategory, true);
            }
            else
            {
                if (boxCategory.YData.Quantity < _configurationParameters.AlertQuantity) //it's sure that it's not 0
                {
                    _comunicate.Alert(boxCategory.Base, boxCategory.YData.Height, boxCategory.YData.Quantity);
                }
                UpdateDate(boxCategory.YData);
            }
        }

        /// <summary>
        /// Delete an item from the system
        /// </summary>
        /// <param name="boxCategory">the category to delete</param>
        /// <param name="delByQuantity">An indicator that determine if the deletion was by expiration date or by 0 quantity</param>
        private void Delete(BoxCategory boxCategory, bool delByQuantity)
        {
            X_Data xToDel = new X_Data(boxCategory.Base);
            X_Data requestedBase;

            _baseTree.Search(xToDel, out requestedBase); // must be true & initialize requestedBase
            if (delByQuantity) _listByTime.DeleteByNode(boxCategory.YData.timeRef); //remove the reference first

            requestedBase.HTree.Remove(boxCategory.YData); // delete y (the remove is by value)
            if (!requestedBase.HTree.IsrootExist())
            {
                _baseTree.Remove(requestedBase); //delete x element as well (empty yTree) 
                if (IsStoreEmpty()) _comunicate.EmptyStoreMessage(); // in a case of an empty xTree
            }
        }

        /// <summary>
        /// find the best category (or combination of categories) for the purchase
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">height</param>
        /// <param name="count">quantity</param>
        /// <returns>answer to the user if found or not</returns>
        public bool FindBestMatch(double x, double y, int count)   //still in progres 
        {
            if (count <= 0 || x <= 0 || y <= 0) throw new Exception("Action failed!\nQuantity & sizes must be a positive number\n");
            double xTemp = x, yTemp = y;
            X_Data requestedBase = new X_Data(xTemp); //criterion for searching a base
            X_Data closestBiggerBase;
            Y_Data requestedHeight; //criterion for searching an height
            Y_Data closestBiggerHeight;
            double upperBoundX = x * _configurationParameters.AcceptableExceeding;
            double upperBoundY = y * _configurationParameters.AcceptableExceeding;
            int splitsLeft = _configurationParameters.MaxSplits; //indicate how many splits were left
            bool userAproved = false;

            while (xTemp <= upperBoundX) //must be <= in oreder to enter. assume the case that AcceptableExceeding=1                                                       
            {
                _baseTree.FindClosestUpper(requestedBase, out closestBiggerBase);   //closestBiggerBase is always >= requestedBase
                if (closestBiggerBase == null || closestBiggerBase.Base > upperBoundX) break;

                requestedHeight = new Y_Data(yTemp);

                while (yTemp <= upperBoundY) //yTemp can exceeds its bounderies in a specific x but maybe in other x not                                                           
                {                         
                    closestBiggerBase.HTree.FindClosestUpper(requestedHeight, out closestBiggerHeight);
                    if (closestBiggerHeight == null || closestBiggerHeight.Height > upperBoundY) // find a bigger base 
                    {
                        if (_exactMatchSizes) NegateExactMatch();
                        break;
                    }
                    if (_exactMatchSizes && closestBiggerBase.Base == x && closestBiggerHeight.Height == y)  //best case: don't ask 
                    {                                                                                 // the user (it's what he wants)
                        if (closestBiggerHeight.Quantity >= count) return Succeed(closestBiggerBase.Base, closestBiggerHeight, count);
                    }
                    if (_exactMatchSizes) NegateExactMatch();

                    if (IsPositive(closestBiggerBase, closestBiggerHeight, count, ref yTemp, ref splitsLeft, ref userAproved))
                        return userAproved; // return the user answer

                    if (splitsLeft < 0) //no splits- don't continue the searching 
                    {
                        InitData();
                        return false;
                    }
                    requestedHeight = new Y_Data(yTemp); //the request must be changed according to height (yTemp was updated)
                                                         //continue with the bigger height (new yTemp is old yTemp+delta)   
                }
                NewBaseInit(closestBiggerBase.Base, ref requestedBase, ref xTemp, ref yTemp, y);
            } //all the heights were over in the current base. Base must been changed
            InitData();
            return false; //x wasn't found      
        }

        /// <summary>
        /// Indicate that the exact match wasn't found
        /// </summary>
        private void NegateExactMatch()
        {
            _exactMatchSizes = false;
            _lock = true; //it's sure that the system will ask the user and the event should be blocked (user answer in first priority)
        }

        /// <summary>
        /// initialize a new base criterion to find
        /// </summary>
        /// <param name="requestedBase">criterion of base</param>
        /// <param name="requestedHeight">criterion of height</param>
        /// <param name="xTemp">temporary base</param>
        /// <param name="yTemp">temporary height</param>
        /// <param name="y">origing height</param>
        private void NewBaseInit(double closestBiggerBase, ref X_Data requestedBase, ref double xTemp, ref double yTemp, double y)
        {
            requestedBase = new X_Data(xTemp = closestBiggerBase + _configurationParameters.Delta);  //no y could match with the given x
            yTemp = y;
        }

        /// <summary>
        /// tell the user if a specific case is ok
        /// </summary>
        /// <param name="x_Data">base element in the tree</param>
        /// <param name="y_Data">height element in the sub tree</param>
        /// <param name="count">quantity</param>
        /// <param name="yTemp">temporary height</param>
        /// <param name="splitsLeft">number of splits that are alowd</param>
        /// <param name="userAproved">indication if user confirm or not</param>
        /// <returns>if case succeed (and update user choice if he aproves it or not)</returns>
        private bool IsPositive(X_Data x_Data, Y_Data y_Data, int count, ref double yTemp, ref int splitsLeft, ref bool userAproved)
        {
            if (CheckCases(x_Data, y_Data, count, ref yTemp, ref splitsLeft)) //if enter- return true anyway
            {
                if (_configurationParameters.MaxSplits == 0 || splitsLeft == _configurationParameters.MaxSplits)
                {                                   //match with no splits (first item). if user confirm- buy. Else-cancel
                    userAproved = UserConfirmation(x_Data.Base, y_Data.Height) ? Succeed(x_Data.Base, y_Data, count) : false;
                }
                else if (_sumOfPotentialQuantities > 0) //splits have been maid
                {
                    if (UserConfirmation(_potentialItems, x_Data.Base, y_Data, count))
                    {
                        if (!_splitsPreference) userAproved = Succeed(x_Data.Base, y_Data, count); //the user prefered to avoid spilts
                                                          //and buy only from one category. found Match without splits (the last item)         
                        else //user prefers splits
                        {
                            count -= _sumOfPotentialQuantities;
                            for (int i = 0; i < _potentialItems.Count; i++)
                            {
                                _potentialItems[i].YData.Quantity = 0;
                                SuccessfulReduceOperation(_potentialItems[i]);
                            }
                            userAproved = Succeed(x_Data.Base, y_Data, count); //found Match with splits       
                        }
                    }
                }
                InitData();
                return true; // case lead to a match (true is returned regardless the user accept it or not (it will be checked later)
            }
            return false; // the case didn't lead to a match- keep searching
        }

        /// <summary>
        /// check if a specific case is ok
        /// </summary>
        /// <param name="x_Data">base element in the tree</param>
        /// <param name="y_Data">height element in the sub tree</param>
        /// <param name="count">quantity</param>
        /// <param name="yTemp">temporary height</param>
        /// <param name="splitsLeft">number of splits that are alowd</param>
        /// <returns>if case succeed</returns>
        private bool CheckCases(X_Data x_Data, Y_Data y_Data, int count, ref double yTemp, ref int splitsLeft)
        {
            if (_configurationParameters.MaxSplits == 0)
            {
                if (y_Data.Quantity >= count) return true;  //comrpomise (not exact sizes and no splits)   
            }
            else if (splitsLeft >= 0)
            {
                if (_sumOfPotentialQuantities == 0 && y_Data.Quantity >= count) return true; //still the first item 
                if (_sumOfPotentialQuantities > 0 && _sumOfPotentialQuantities + y_Data.Quantity >= count) return true; //not the first
                if (_potentialItems.Count < _configurationParameters.MaxSplits)
                {
                    PartialMatch(y_Data, x_Data.Base);   //splits are aloud
                    splitsLeft--; //lower the splits
                }
            }

            yTemp = y_Data.Height + _configurationParameters.Delta; //keep searching with a bigger height
            return false;
        }

        /// <summary>
        /// ask the user if he is intrested with the match (with splits)
        /// </summary>
        /// <param name="potentialItems">sum of all items before</param>
        /// <param name="bases">list of previous bases</param>
        /// <param name="currentBase">list of previous yData</param>
        /// <param name="currentYData">the cureent element</param>
        /// <param name="count">quantity</param>
        /// <returns>user answer</returns>
        private bool UserConfirmation(List<BoxCategory> potentialItems, double currentBase, Y_Data currentYData, int count)
        {
            int categories = potentialItems.Count;
            StringBuilder sb = new StringBuilder($"\nThe system has found a match but not in the exact sizes.\nThe match has" +
                $" {categories} splits ({categories + 1} categories):\n\n");

            int originCount = count;

            for (int i = 0; i < categories; i++)
            {
                sb.AppendLine($"{i + 1}) category {potentialItems[i].Base} X {potentialItems[i].YData.Height}:   " +
                    $"{potentialItems[i].YData.Quantity} items");

                count -= potentialItems[i].YData.Quantity;
            }
            sb.AppendLine($"{categories + 1}) category {currentBase} X {currentYData.Height}:   {count} items");

            if (currentYData.Quantity >= originCount)
            {
                sb.Append($"\nThe system suggest another option (without splits):\nBase area {currentBase} m^2\nHeiht: " +
                    $"{currentYData.Height} m\n\nChoose your option:\n\nPress 1 to cancel.\nPress 2 for the first option" +
                    $" (with splits).\nPress 3 for the second option.\n\nUser choice: ");

                Results userAnswer = (Results)_comunicate.MultiChoiceRespond(out Results multyChoiceAnswer, sb.ToString());
                if (userAnswer == Results.cancel) return false;
                else if (userAnswer == Results.noSplits) _splitsPreference = false;
                return true;
            }
            return _comunicate.GetRespondWithSplits(out string answer, sb.ToString());
        }

        /// <summary>
        /// update data when match is conmfirmed
        /// </summary>
        /// <param name="closestBiggerBase">current base</param>
        /// <param name="closestBiggerHeight">current height</param>
        /// <param name="count">quantity</param>
        /// <returns>always true. match was already aproved</returns>
        private bool Succeed(double Base, Y_Data closestBiggerHeight, int count)
        {
            closestBiggerHeight.Quantity -= count;
            SuccessfulReduceOperation(new BoxCategory(closestBiggerHeight, Base));
            return true;
        }

        /// <summary>
        /// update dates of boxes that were purchased
        /// </summary>
        /// <param name="y_Data">height element on the sub tree</param>
        private void UpdateDate(Y_Data y_Data)
        {
            y_Data.timeRef._data._lastPurchaseDate = DateTime.Now;
            _listByTime.ReplaceByNode(y_Data.timeRef);
        }

        /// <summary>
        /// tell the user that a match was found and ask confirmation
        /// </summary>
        /// <param name="Base"></param>
        /// <param name="height"></param>
        /// <returns>user answer</returns>
        private bool UserConfirmation(double Base, double height) => _comunicate.MatchWithoutSplits(Base, height);

        /// <summary>
        /// initalize data after the purchase (wherever if done or not)
        /// </summary>
        private void InitData()
        {
            _potentialItems = new List<BoxCategory>();
            _sumOfPotentialQuantities = 0;
            _exactMatchSizes = true;
            _splitsPreference = true;
            _lock = false;
        }

        /// <summary>
        /// when there are splits, save the previous data of an element
        /// </summary>
        /// <param name="y_Data"></param>
        /// <param name="x"></param>
        private void PartialMatch(Y_Data y_Data, double x)
        {
            _sumOfPotentialQuantities += y_Data.Quantity;
            _potentialItems.Add(new BoxCategory(y_Data, x));
        }
        public void Exit() => Dispose();

        public void Dispose() => _timer.Dispose();
    }
}

