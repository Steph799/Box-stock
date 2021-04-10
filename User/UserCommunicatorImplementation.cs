using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Box_stock;

namespace User
{
    class UserCommunicatorImplementation : ICommunicate
    {
        /// <summary>
        /// yes/no question
        /// </summary>
        /// <param name="answer">user answer</param>
        /// <returns>yes or no</returns>
        public bool GetRespond(out string answer)
        {
            Console.Write("\nPress \"y\" to confirm or \"n\" to cancel the purchase: ");
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                answer = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                if (InvalidAnswer(answer)) Console.Write("Invalid input. Enter \"y\" or \"n\": ");
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return answer == "y";
                }
            } while (true);
        }
        private bool InvalidAnswer(string answer) => answer != "y" && answer != "n";

        /// <summary>
        /// tell the user that the system found a match that doesn't require splits
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">heigh</param>
        /// <returns>user respond</returns>
        public bool MatchWithoutSplits(double x, double y)
        {
            Console.Write($"\nThe system has found a match but not in the exact sizes:\nBase area: {x}\nHeiht: {y}\n");           
            return GetRespond(out string answer);
        }

        private bool IsValid(Results answer) => answer == Results.cancel || answer == Results.withSplits || answer == Results.noSplits;

        /// <summary>
        /// Alert the user about lack of quantity
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">heigh</param>
        /// <param name="quantity">quantity</param>
        public void Alert(double x, double y, int quantity)
        {
            Console.WriteLine($"\nAlert! you have {quantity} boxes with sizes {x} X {y} left. Consider reorder.\n");
        }

        /// <summary>
        /// update the user about a category that was over and will be no longer part of the system
        /// </summary>
        /// <param name="x">base</param>
        /// <param name="y">heigh</param>
        public void RunOutMassage(double x, double y)
        {
            Console.WriteLine($"\nThe are no more boxes from sizes {x} x {y}.\nThis category will be removed from the system.\n");
        }

        /// <summary>
        /// tell the user that the system found a match that require splits
        /// </summary>
        /// <param name="answer">user answer</param>
        /// <param name="details">all splits detais and categories</param>
        /// <returns>user answer</returns>
        public bool GetRespondWithSplits(out string answer, string details) 
        {                                                                   
            Console.WriteLine(details);        
            return GetRespond(out answer);
        }

        /// <summary>
        /// tell the user that the system found a match that require splits and offer him another choice without splits if he wants
        /// </summary>
        /// <param name="answer">user answer</param>
        /// <param name="details">all splits detais and categories</param>
        /// <returns>user answer</returns>
        public Enum MultiChoiceRespond(out Results answer, string details)
        {
            Console.Write(details);
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (Enum.TryParse(Console.ReadLine(), out answer) && IsValid(answer))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    return answer;
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Invalid input. Please enter 1,2 or 3: ");
            } while (true);
        }

        public void ExpirationDateMessage(double x, double y)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nAttention!\ncategory box {x} X {y} is no longer valid and will be removed from the system.");
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public void EmptyStoreMessage()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nStore is empty\n");
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }
}
