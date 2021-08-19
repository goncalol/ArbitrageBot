using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;

namespace TraderBot.Exchanges.Coins
{
    public class Coin : ICoin
    {
        public enum CoinName { BTC_XRP, BTC_XLM, BTC_STEEM, BTC_XEM, BTC_NEO, BTC };

        private bool hasvalue = false;
        private ICoinDetail value;
        private DateTime lastTick;
        private CoinName coinName;
        private IExchange exchange;
        private List<PromisedPair> promisedPairContainer;

        public Coin(CoinName coinName, IExchange exchange)
        {
            this.coinName = coinName;
            this.exchange = exchange;
            promisedPairContainer = new List<PromisedPair>();
        }

        public string getCoinName()
        {
            return coinName.ToString();
        }

        public string getExchangeName()
        {
            return exchange.GetName();
        }
        
        public DateTime getTimestamp()
        {
            return lastTick;
        }

        public ICoinDetail getValue()
        {
            return value;
        }

        public bool hasValue()
        {
            return hasvalue;
        }

        public void setValue(ICoinDetail value, DateTime tick_time = default(DateTime))
        {
            this.value = value;
            lastTick = tick_time== default(DateTime) ? DateTime.Now:tick_time;
            hasvalue = true;
        }

        public void CreatePromissedPair(ICoin otherCoin, double firstDiference)
        {
            promisedPairContainer.Add(new PromisedPair { pairCoin=otherCoin, firstTime=DateTime.Now, firstDiference= firstDiference });
        }

        public PromisedPair GetPromissedPair(ICoin otherCoin)
        {
            var res = promisedPairContainer.FirstOrDefault(e=>e.pairCoin==otherCoin);

            return res==default(PromisedPair)?null:res;
        }

        public Task<OperationResult<IOrderBook>> GetOrderBook()
        {
            return exchange.GetOrderBookFor(coinName);
        }

        public Task<OperationResult<IOrderBook>> Get_BTC_OrderBook()
        {
            return exchange.GetOrderBookFor(CoinName.BTC);
        }

        public Task<OperationResult<IOrder>> PostBuyOrder(double price, double quantity)
        {
            return exchange.PostBuyOrderFor(coinName,price, quantity);
        }

        public Task<OperationResult<IOrder>> PostSellOrder(double price, double quantity)
        {
            return exchange.PostSellOrderFor(coinName, price, quantity);
        }

        public Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            return exchange.IsOrderComplete(order);
        }

        public Task<OperationResult<string>> WithDraw(double amount, ICoin destinyCoin)
        {
            return exchange.WithDraw(coinName, amount, destinyCoin.GetExchange());
        }

        public Address GetAddress()
        {
            return exchange.GetAddress();
        }

        public Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            return exchange.HasOrderArrived(order);
        }

        public CoinName getCoin()
        {
            return coinName;
        }

        public IExchange GetExchange()
        {
            return exchange;
        }
    }

    public class PromisedPair
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ICoin pairCoin { get; set; }

        public double firstDiference { get; set; }

        public double BuyAveragePrice { get; set; }

        public DateTime firstTime { get; set; }

        private bool processing = false;

        public bool IsInPeriod(int minSeconds, int maxSeconds, double val)
        {
            var now = DateTime.Now;
            if (DateTime.Compare(now, firstTime.AddSeconds(minSeconds)) > 0)
            {
                if (DateTime.Compare(now, firstTime.AddSeconds(maxSeconds)) < 0)
                {
                    return true;
                }
                else
                {
                    firstTime = now;
                    firstDiference = val;
                    logger.Debug(string.Format("Coin({0}) overpass maximum interval({1})", pairCoin.getCoinName(), maxSeconds));
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public void SetProcessing(bool processing)
        {
            this.processing = processing;
        }

        public void ResetTime(double val)
        {
            firstDiference = val;
            firstTime = DateTime.Now;
        }

        public bool IsProcessing()
        {
            return processing;
        }

        public double GetAverageBuyValue()
        {
            return BuyAveragePrice;
        }

        public void SetAverageBuyPrice(double price)
        {
            BuyAveragePrice = price;
        }
    }
}
