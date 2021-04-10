using System;
using System.Collections.Generic;
using System.Text;

namespace Box_stock
{
    public class ConfigurationParameters
    {
        const int MAX_POSSIBLE_SPLITS = 3; //set by the instructore
        double _numberInPrecent;

        public int MaxQuantity { get; }
        public int AlertQuantity { get; }
        public double AcceptableExceeding { get => 1+(_numberInPrecent / 100); }  // convert from exceeding % to the coeficient
        public double Delta { get => DigitSensitivity <= 0 ? 1 : Math.Pow(10, -DigitSensitivity); } 
        public int MaxSplits { get; }
        public int CheckFrequency { get; } //in days
        public int LifeTime { get; } //in days
        public int DigitSensitivity { get; }    

        public ConfigurationParameters(int maxQuantity, int alertQuantity, double acceptableExceeding, int numOfDigits,
             int maxSplits, int checkFrequency, int lifeTime) 
        {
            MaxQuantity = maxQuantity;
            AlertQuantity = alertQuantity;
            _numberInPrecent = acceptableExceeding;            
            DigitSensitivity = numOfDigits;           
            MaxSplits = maxSplits;
            CheckFrequency = checkFrequency;
            LifeTime = lifeTime;

            if (!CheckValidation()) throw new Exception("\nInitailization have failed!\nOne (or more) of your parameters is not" +
                " valid. Please try again\n");
        }
        private bool CheckValidation()
        {
            return MaxQuantity > 0 && AlertQuantity > 0 && AcceptableExceeding >= 1 && Delta > 0 && MaxSplits >= 0 &&
               MaxSplits<= MAX_POSSIBLE_SPLITS && CheckFrequency > 0 && LifeTime > 0; //MaxSplits won't be bigger than 3 on purpose,
        }                                             //in order to avoid the user to recieve to many different boxes (there is no 
    }                                                            //problem to write any different, smaller or bigger, positive number)
}
