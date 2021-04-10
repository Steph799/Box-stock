using System;
using System.Collections.Generic;
using System.Text;
using Box_stock.Data_structures;

namespace Box_stock.Data_models
{
    class X_Data:IComparable<X_Data>
    { 
        public double Base { get; set; }
        public BTS<Y_Data> HTree { get; set; }

        public X_Data(double _base, Y_Data yData) 
        {
            Base = _base;           
            HTree = new BTS<Y_Data>();
            HTree.Add(yData);
        }
        public X_Data(double _base) => Base = _base;//dummy constractor
   
        public int CompareTo(X_Data other) => Base.CompareTo(other.Base);    
    }
}
