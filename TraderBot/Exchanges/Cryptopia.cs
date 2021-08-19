using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges
{
    public class Cryptopia : Exchange
    {
        public Cryptopia()
        {
            BTC_XLM = new Coin(CoinName.BTC_XLM, this);
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXLM(exchange);
            }
            catch(Exception ex)
            {
                throw new Exception("Error in Cryptopia", ex);
            }
        }

        public override Address GetAddress()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Cryptopia";
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

    public class CRYPTOPIA_COIN
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public CRYPTOPIA_COIN_DETAIL Data { get; set; }

        public string Error { get; set; }
    }

    public class CRYPTOPIA_COIN_DETAIL : ICoinDetail
    {
        public int TradePairId { get; set; }

        public string Label { get; set; }

        public double AskPrice { get; set; }

        public double BidPrice { get; set; }

        public double Low { get; set; }

        public double High { get; set; }

        public double Volume { get; set; }

        public double LastPrice { get; set; }

        public double BuyVolume { get; set; }

        public double SellVolume { get; set; }

        public double Change { get; set; }

        public double Open { get; set; }

        public double Close { get; set; }

        public double BaseVolume { get; set; }

        public double BuyBaseVolume { get; set; }

        public double SellBaseVolume { get; set; }

        public double getAsk()
        {
            return AskPrice;
        }

        public double getBid()
        {
            return BidPrice;
        }

        public double getLastPrice()
        {
            return LastPrice;
        }
    }
}
