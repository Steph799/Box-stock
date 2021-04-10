using System;
using System.Collections.Generic;
using System.Text;
using Box_stock.Data_models;

namespace Box_stock
{
    public interface ICommunicate
    {
        bool GetRespond(out string answer);
        bool GetRespondWithSplits(out string answer, string details);
        bool MatchWithoutSplits(double x, double y);
        Enum MultiChoiceRespond(out Results answer, string details);
        void Alert(double x, double y, int quantity);
        void RunOutMassage(double x, double y);
        void ExpirationDateMessage(double x, double y);
        void EmptyStoreMessage();
    }
}
