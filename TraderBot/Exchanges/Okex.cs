using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;
using TraderBot.Exchanges.Orders;

namespace TraderBot.Exchanges
{
    public class Okex : Exchange
    {
        public HttpClient okexClient;

        public Okex()
        {
            BTC_XLM = new Coin(CoinName.BTC_XLM, this);
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
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
                throw new Exception("Error in Okex", ex);
            }
        }

        public override Address GetAddress()
        {
            throw new NotImplementedException();
        }

        public override string GetName()
        {
            return "Okex";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream1 = await okexClient.GetStringAsync("https://www.okex.com/api/v1/depth.do?symbol=xrp_btc&size=5");
                        var res1 = JsonConvert.DeserializeObject<Okex_OrderBook>(stream1);
                        return new OperationResult<IOrderBook> { Success = true, Result = res1 };
                    case CoinName.BTC_XLM:
                        var stream2 = await okexClient.GetStringAsync("https://www.okex.com/api/v1/depth.do?symbol=xlm_btc&size=5");
                        var res2 = JsonConvert.DeserializeObject<Okex_OrderBook>(stream2);
                        return new OperationResult<IOrderBook> { Success = true, Result = res2 };

                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Okex only accepts XRP and XLM coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Okex GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
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

        public void setClient(HttpClient okexClient)
        {
            this.okexClient = okexClient;
        }

        public override Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinationExchange)
        {
            throw new NotImplementedException();
        }
    }

    public class OkexCoinRes
    {
        public string date { get; set; }

        public OkexResult ticker { get; set; }
    }

    public class OkexResult :ICoinDetail
    {
        public string high { get; set; }

        public string vol { get; set; }

        public string last { get; set; }

        public string low { get; set; }

        public string buy { get; set; }

        public string sell { get; set; }

        public double getAsk()
        {
            return Convert.ToDouble(buy, CultureInfo.InvariantCulture);
        }

        public double getBid()
        {
            return Convert.ToDouble(sell, CultureInfo.InvariantCulture);
        }

        public double getLastPrice()
        {
            return Convert.ToDouble(last, CultureInfo.InvariantCulture);
        }
    }

    public class Okex_OrderBook : IOrderBook
    {
        public Okex_OrderBook_Pair[] asks { get; set; }

        public Okex_OrderBook_Pair[] bids { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return asks;
        }

        public IOrderBookPair[] GetBid()
        {
            return bids;
        }
    }

    [JsonConverter(typeof(OKEX_ORDERBOOK_CONVERTOR))]
    public class Okex_OrderBook_Pair : IOrderBookPair
    {
        public double value { get; set; }

        public double quantity { get; set; }

        public double GetQuantity()
        {
            return quantity;
        }

        public double GetValue()
        {
            return value;
        }
    }

    public class OKEX_ORDERBOOK_CONVERTOR : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Okex_OrderBook_Pair));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray ja = JArray.Load(reader);
            Okex_OrderBook_Pair pop = new Okex_OrderBook_Pair();
            pop.value = (double)ja[0];
            pop.quantity = (double)ja[1];
            return pop;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray ja = new JArray();
            Okex_OrderBook_Pair pop = (Okex_OrderBook_Pair)value;
            ja.Add(pop.value);
            ja.Add(pop.quantity);
            ja.WriteTo(writer);
        }
    }
}
