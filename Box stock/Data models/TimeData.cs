using System;
using System.Collections.Generic;
using System.Text;
using Box_stock.Data_structures;

namespace Box_stock.Data_models
{
    struct TimeData
    {
        public double Base { get; } 
        public double Height { get; }  
   
        public DateTime _lastPurchaseDate { get; set; } 

        public TimeData(double _Base, double height)
        {
            Base = _Base;
            Height = height;
            _lastPurchaseDate = DateTime.Now;
        }         
    }
}
