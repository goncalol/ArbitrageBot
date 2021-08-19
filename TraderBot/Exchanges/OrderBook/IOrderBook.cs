using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBot.Exchanges.OrderBook
{
    public interface IOrderBook
    {
        IOrderBookPair[] GetAsk();

        IOrderBookPair[] GetBid();
    }

    public interface IOrderBookPair
    {
        double GetValue();

        double GetQuantity();
    }
}
