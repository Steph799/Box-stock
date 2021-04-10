using System;
using System.Collections.Generic;
using System.Text;
using Box_stock.Data_structures;

namespace Box_stock.Data_models
{
    class Y_Data : IComparable<Y_Data>
    {
        public double Height { get; set; }
        public int Quantity { get; set; }
        public DoubleLinkList<TimeData>.Node timeRef { get; set; }

        public Y_Data(double height, int count)
        {
            Height = height;
            Quantity = count;
        }
        public Y_Data(double height) => Height = height; //dummy constractor    

        public int CompareTo(Y_Data other) => Height.CompareTo(other.Height);
    }
}
