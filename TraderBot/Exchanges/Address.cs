using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderBot.Exchanges
{
    public class Address
    {
        public AddressDetail BTC { get; set; }

        public AddressDetail XRP { get; set; }

        public AddressDetail XLM { get; set; }

        public AddressDetail STEEM { get; set; }

        public AddressDetail XEM { get; set; }

        public AddressDetail NEO { get; set; }
    }

    public class AddressDetail
    {
        public string address { get; set; }

        public string destination { get; set; }
    }

    public class AddressBuilder
    {
        public static string Build(IExchange destinyExchange, string Address, string Tag)
        {
            if(destinyExchange.GetName()== "Poloniex")
                return Address;

            return string.Format("{0}&{1}",Address,Tag);
        }
    }
}
