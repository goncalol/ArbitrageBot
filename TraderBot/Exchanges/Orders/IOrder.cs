using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBot.Exchanges.Orders
{
    public interface IOrder
    {
        long GetOrderId();

        string GetOrderId_String();

        double GetTotalAmount();

        string GetCurrencySymbol();
    }
}
