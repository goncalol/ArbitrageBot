using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges
{
    public class Bitstamp : Exchange
    {

        public Bitstamp()
        {
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXRP(exchange);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Bitstamp ({0})", exchange.GetName()), ex);
            }
        }

        public override Address GetAddress()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Bitstamp";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            //try
            //{
            //    switch (coinName)
            //    {
            //        case CoinName.BTC_XRP:
            //            var stream = await bithumbClient.GetStringAsync("https://api.bithumb.com/public/orderbook/XRP?count=5");
            //            var res = JsonConvert.DeserializeObject<Bithumb_OrderBook>(stream);
            //            if (res.status != "0000") return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bithumb Error. Error Code:{0}.", res.status) };
            //            return new OperationResult<IOrderBook> { Success = true, Result = res.data };
            //        default:
            //            return new OperationResult<IOrderBook> { Success = false, Message = "Bithumb only accepts XRP coin" };
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Bithumb GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            //}
            return null;
        }

        public override Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinationExchange)
        {
            throw new NotImplementedException();
        }
    }

    public class BitstampCoin : ICoinDetail
    {
        public double amount { get; set; }

        public double buy_order_id { get; set; }

        public double sell_order_id { get; set; }

        public string amount_str { get; set; }

        public string timestamp { get; set; }

        public double price { get; set; }

        public int type { get; set; }

        public int id { get; set; }

        public double getAsk()
        {
            return sell_order_id;
        }

        public double getBid()
        {
            return buy_order_id;
        }

        public double getLastPrice()
        {
            return price;
        }
    }
}
