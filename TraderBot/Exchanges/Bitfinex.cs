using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using TraderBot.Exchanges.Coins;
using static TraderBot.Exchanges.Coins.Coin;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Utils;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;
using TraderBot.Exchanges.Orders;

namespace TraderBot.Exchanges
{
    public class Bitfinex: Exchange
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private HttpClient bitfinexClient;

        private Dictionary<int, ICoin> dict = new Dictionary<int, ICoin>();

        public Bitfinex()
        {
            bitfinexClient = new HttpClient();
            BTC_XLM = new Coin(CoinName.BTC_XLM, this);
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
        }

        public void update(double[] values)
        {
            var channel = (int)values[0];
            var coin = dict[channel];
            var coinDetail = new BitfinexCoin();
            coinDetail.bid = values[1];
            coinDetail.bid_size = values[2];
            coinDetail.ask = values[3];
            coinDetail.ask_size = values[4];
            coinDetail.daily_change = values[5];
            coinDetail.daily_change_perc = values[6];
            coinDetail.last_price = values[7];
            coinDetail.volume = values[8];
            coinDetail.high = values[9];
            coinDetail.low = values[10];
            
            if (coin == BTC_XLM)
            {
                Console.WriteLine(string.Format("Bitfinex(XLM) last:{0}", coinDetail.last_price));
                //logger.Debug(string.Format("Bitfinex(LTC) last:{0}", coinDetail.last_price));
                SetXLM(coinDetail);
            }else if (coin == BTC_XRP)
            {
                Console.WriteLine(string.Format("Bitfinex(XRP) last:{0}", coinDetail.last_price));
                //logger.Debug(string.Format("Bitfinex(XRP) last:{0}", coinDetail.last_price));
                SetXRP(coinDetail);
            }
            else
            {
                logger.Debug(string.Format("Bitfinex update:: No references found"));
            }
        }

        public void subscribe(BitfinexEventResult newVal)
        {
            if (newVal.pair == "XRPBTC")
            {
                //BTC_XRP = new BitfinexCoin();
                dict.Add(newVal.chanId, BTC_XRP);
            }
            else if (newVal.pair == "XLMBTC")
            {
                //BTC_LTC = new BitfinexCoin();
                dict.Add(newVal.chanId, BTC_XLM);
            }
            else
            {
                Console.WriteLine(string.Format("Bitfinex pair not recognizable:{0}",newVal.pair));
            }
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXLM(exchange);
                compareXRP(exchange);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Bitfinex ({0})", exchange.GetName()), ex);
            }
        }

        public override string GetName()
        {
            return "Bitfinex";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream = await bitfinexClient.GetStringAsync("https://api.bitfinex.com/v1/book/XRPBTC?limit_bids=5&limit_asks=5");
                        var res = JsonConvert.DeserializeObject<Bitfinex_OrderBook>(stream);                        
                        return new OperationResult<IOrderBook> { Success = true, Result = res };
                    case CoinName.BTC_XLM:
                        var stream2 = await bitfinexClient.GetStringAsync("https://api.bitfinex.com/v1/book/XLMBTC?limit_bids=5&limit_asks=5");
                        var res2 = JsonConvert.DeserializeObject<Bitfinex_OrderBook>(stream2);
                        return new OperationResult<IOrderBook> { Success = true, Result = res2 };
                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Bitfinex only accepts XRP and XLM coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Bitfinex GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }
        

        public override Address GetAddress()
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinationExchange)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail timestamp)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            throw new NotImplementedException();
        }
    }

    public class BitfinexCoin : ICoinDetail
    {
        public double bid { get; set; }

        public double ask { get; set; }

        public double last_price { get; set; }

        public double low { get; set; }

        public double high { get; set; }

        public double volume { get; set; }

        public double bid_size { get; set; }

        public double ask_size { get; set; }

        public double daily_change { get; set; }

        public double daily_change_perc { get; set; }

        public double getAsk()
        {
            return ask;
        }

        public double getBid()
        {
            return bid;
        }

        public double getLastPrice()
        {
            return last_price;
        }
    }

    public class BitfinexEventHelper
    {
        public string @event { get; set; }
    }

    public class BitfinexEventResult
    {
        public string @event { get; set; }

        public string channel { get; set; }

        public int chanId { get; set; }

        public string pair { get; set; }
    }

    public class Bitfinex_OrderBook : IOrderBook
    {
        public Bitfinex_OrderBook_Pair[] bids { get; set; }

        public Bitfinex_OrderBook_Pair[] asks { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return asks;
        }

        public IOrderBookPair[] GetBid()
        {
            return bids;
        }
    }

    public class Bitfinex_OrderBook_Pair : IOrderBookPair
    {
        public string price { get; set; }

        public string amount { get; set; }

        public string timestamp { get; set; }

        public double GetQuantity()
        {
            return Convert.ToDouble(amount, CultureInfo.InvariantCulture);
        }

        public double GetValue()
        {
            return Convert.ToDouble(price, CultureInfo.InvariantCulture);
        }
    }
}
