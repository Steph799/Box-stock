using System;
using Box_stock;

namespace User
{
    class Program 
    {
        static ConfigurationParameters configurationParameters;
        static Manager manager;
        const string INITIALIZED_ERROR = "The system hasn't been initialized yet\n";
        static UserCommunicatorImplementation printData;
        static void Main(string[] args)
        {
            string userInput = null;
            printData = new UserCommunicatorImplementation();
            configurationParameters = new ConfigurationParameters(100, 10, 50, 2,
                3, 1, 21); //data base 
            manager = new Manager(configurationParameters, printData);

            manager.Insert(2, 1, 18); //basic data base
            manager.Insert(2, 0.2, 18);
            manager.Insert(2, 2, 18);
            manager.Insert(2, 3, 14);
            manager.Insert(2, 1.59, 14);
            manager.Insert(1, 1.5, 27);
            manager.Insert(1, 3, 118);
            manager.Insert(3, 1, 12);
            manager.Insert(0.5, 0.5, 15);
            manager.Insert(0.75, 1, 25);
            manager.Insert(1.1, 0.8, 32);
            manager.Insert(1, 2, 5);
            manager.Insert(1.3, 1, 37);
            manager.Insert(0.5, 0.6, 100);
            manager.Insert(1.7, 0.8, 33);
            manager.Insert(1, 1, 17);
            manager.Insert(2, 1.8, 57);
            manager.Insert(1.25, 1, 27);
            manager.Insert(1.75, 1.25, 30);
            manager.Insert(1.45, 2.5, 31);
            manager.Insert(2.5, 2.8, 97);
            manager.Insert(1.7, 2.1, 22);
            manager.Insert(1.66, 1.55, 60);
            manager.Insert(1.66, 1.3, 11);
            manager.Insert(1.6, 1.71, 18);
            manager.Insert(0.8, 1.05, 38);
            manager.Insert(2.5, 2.8, 210);


            while (userInput != "5")
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Choose your option:\n\n1) view System definitions\n" +
                    "2) Order items\n3) Get data\n4) Buy\n5) Exit\n\nUser choice: ");

                Console.ForegroundColor = ConsoleColor.Green;
                userInput = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                try
                {
                    switch (userInput)
                    {
                        case "1":
                            Console.WriteLine($"\nSystem configurations:\n");
                            Console.WriteLine(ViewDefinitions());                         
                            break;

                        case "2":
                            InsertDetails();                         
                            break;

                        case "3":
                            if (manager.IsStoreEmpty()) printData.EmptyStoreMessage();
                            else GetData();                         
                            break;

                        case "4":
                            if (manager.IsStoreEmpty()) printData.EmptyStoreMessage();
                            else Console.WriteLine(BuyBoxes());
                            break;

                        case "5":
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Bye bye");
                            manager.Exit();
                            break;

                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            InvalidInputChoice();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n{e.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        private static string ViewDefinitions() //case 1
        {
            if (configurationParameters != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                return $"Maximum quantity per box: {configurationParameters.MaxQuantity}\nShortage alert from" +
                    $" {configurationParameters.AlertQuantity} items\nAcceptable exceeding:" +
                    $" {(Math.Round(configurationParameters.AcceptableExceeding * 100) - 100)}%\nAddition delta while searching a" +
                    $" bigger element: {configurationParameters.Delta}\nMaximum number of splits: {configurationParameters.MaxSplits}\n" +
                    $"Checking frequency: every {configurationParameters.CheckFrequency} days\nLifetime of an item:" +
                    $" {configurationParameters.LifeTime} days\n\n";
            }
            throw new Exception(INITIALIZED_ERROR);
        }

        private static void InsertDetails() //case 2
        {
            double bottom, height;
            int quantity;

            Insertsizes(out bottom, out height, true, out quantity);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(manager.Insert(bottom, height, quantity));
        }

        private static void GetData() //case 3
        {
            double bottom, height;
            Insertsizes(out bottom, out height, false, out int quantity);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{manager.GetData(bottom, height)}\n");
        }

        private static string BuyBoxes() //case 4
        {
            double bottom, height;
            int quantity;
            Insertsizes(out bottom, out height, true, out quantity);
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (manager.FindBestMatch(bottom, height, quantity))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                return "\nYour stock contents has been updated\n";
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            return "\nUnable to complete the purchase\n";
        }

        private static void Insertsizes(out double bottom, out double height, bool quantityInclude, out int quantity)
        {
            if (manager == null) throw new Exception(INITIALIZED_ERROR);
            quantity = 0;

            Console.Write("\nInsert the base value: ");
            ValidSize(out bottom);
            Console.Write("Insert the height value: ");
            ValidSize(out height);
            ValidSensitivity(ref bottom, ref height);
            if (quantityInclude)
            {
                Console.Write("Insert the quantity: ");
                ValidSize(out quantity);
            }
        }

        /// <summary>
        /// avoid the user of enter an input with a bigger sensetivity than specified
        /// </summary>
        /// <param name="bottom"></param>
        /// <param name="height"></param>
        private static void ValidSensitivity(ref double bottom, ref double height)
        {
            bottom = Math.Round(bottom, configurationParameters.DigitSensitivity);
            height = Math.Round(height, configurationParameters.DigitSensitivity);
        }

        private static void ValidSize(out int size)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            while (!int.TryParse(Console.ReadLine(), out size)) InvalidInputColors();
        }

        private static void ValidSize(out double size)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            while (!double.TryParse(Console.ReadLine(), out size)) InvalidInputColors();
        
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void InvalidInputColors()
        {
            Console.ForegroundColor = ConsoleColor.White;
            InvalidInputSize();
            Console.ForegroundColor = ConsoleColor.Green;
        }
        private static void InvalidInputChoice() => Console.WriteLine("Invalid input. Please enter a number from 1-5\n");
        private static void InvalidInputSize() => Console.WriteLine("Invalid input. Please enter a number\n");      
    }
}
