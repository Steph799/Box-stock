using System;
using System.Collections.Generic;
using System.Text;

namespace Box_stock.Data_models
{
    struct BoxCategory
    {
        public Y_Data YData { get; }
        public double Base { get; }
        public BoxCategory(Y_Data yData, double _base)
        {
            YData = yData;
            Base = _base;
        }
    }
}
