using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges.Orders
{
    public class OrderDetail
    {
        public string txId { get; set; }

        public long timestamp { get; set; }

        public double amount { get; set; }

        public CoinName currencyName { get; set; }
    }
}
