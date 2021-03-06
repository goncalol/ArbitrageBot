using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges
{
    public class Poloniex : Exchange
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public HttpClient poloniexClient;
        private Address address;
        private readonly string API_Key = "Redacted";
        private readonly string API_Secret = "Redacted";

        public Poloniex()
        {
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
            BTC_STEEM = new Coin(CoinName.BTC_STEEM, this);
            address = new Address
            {
                BTC = new AddressDetail { address = "Redacted" },
                XRP = new AddressDetail { address = "Redacted" },
                STEEM = new AddressDetail { address = "" },
                //BTS = new AddressDetail { address = "poloniexwallet", destination = "Redacted" }
            };
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXRP(exchange);
                //compareSTEEM(exchange);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Poloniex ({0})", exchange.GetName()), ex);
            }
        }

        public override string GetName()
        {
            return "Poloniex";
        }

        public override Address GetAddress()
        {
            return address;
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream = await poloniexClient.GetStringAsync("https://poloniex.com/public?command=returnOrderBook&currencyPair=BTC_XRP&depth=5");
                        logger.Trace(stream);
                        var res = JsonConvert.DeserializeObject<POLONIEX_ORDERBOOK>(stream);
                        if (res.isFrozen != "0") return new OperationResult<IOrderBook> { Success = false, Message = "Get OrderBook from Poloniex Error. The market is frozen." };
                        return new OperationResult<IOrderBook> { Success = true, Result=res };
                    case CoinName.BTC_STEEM:
                        var stream2 = await poloniexClient.GetStringAsync("https://poloniex.com/public?command=returnOrderBook&currencyPair=BTC_STEEM&depth=5");
                        logger.Trace(stream2);
                        var res2 = JsonConvert.DeserializeObject<POLONIEX_ORDERBOOK>(stream2);
                        if (res2.isFrozen != "0") return new OperationResult<IOrderBook> { Success = false, Message = "Get OrderBook from Poloniex Error. The market is frozen." };
                        return new OperationResult<IOrderBook> { Success = true, Result = res2 };
                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Poloniex only accepts XRP, STEEM coin" };
                }
            }catch(Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Poloniex GetOrderBookFor() || Message:{0} || InnerMessage:{1}",ex.Message, ex.InnerException!=null?ex.InnerException.Message:"") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        return await Market(quantity, price, "BTC_XRP", "buy");
                    case CoinName.BTC_STEEM:
                        return await Market(quantity, price, "BTC_STEEM", "buy");
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Poloniex only accepts XRP, STEEM coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Poloniex PostBuyOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }
        
        public override async Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            try
            {
                //var data = string.Format("command=returnOpenOrders&currencyPair={0}", ((POLONIEX_BUY_ORDER)order).GetCurrencySymbol());
                //var signature = SignData(data);
                //poloniexClient.DefaultRequestHeaders.Clear();
                //poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
                //poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
                //var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
                //request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

                //var result = await poloniexClient.SendAsync(request);
                //string resultStrContent = await result.Content.ReadAsStringAsync();
                //logger.Trace(resultStrContent);

                //var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_OPEN_ORDERS[]>(resultStrContent);
                //if (resultObjContent.Length!=0)
                //{
                //    var o = resultObjContent.FirstOrDefault(e=>e.orderNumber==order.GetOrderId_String());
                //    if(o != null)//se existir a ordem
                //    {
                //        return new OperationResult<bool> { Success = true, Result = false };
                //    }
                //}

                var data2 = string.Format("command=returnTradeHistory&currencyPair={0}", ((POLONIEX_BUY_ORDER)order).GetCurrencySymbol());
                var signature2 = SignData(data2);
                poloniexClient.DefaultRequestHeaders.Clear();
                poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
                poloniexClient.DefaultRequestHeaders.Add("Sign", signature2.signature);
                var request2 = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
                request2.Content = new StringContent(signature2.data, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result2 = await poloniexClient.SendAsync(request2);
                string resultStrContent2 = await result2.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent2);

                var resultObjContent2 = JsonConvert.DeserializeObject<POLONIEX_OPEN_ORDERS[]>(resultStrContent2);
                if (resultObjContent2.Length != 0)
                {
                    var o2 = resultObjContent2.FirstOrDefault(e => e.orderNumber == order.GetOrderId_String());
                    if (o2 != null)//se existir a ordem
                    {
                        return new OperationResult<bool> { Success = true, Result = true };
                    }
                }
                return new OperationResult<bool> { Success = true, Result=false };
            }
            catch (Exception ex)
            {
                return new OperationResult<bool> { Success = false, Message = string.Format("Exception in Poloniex IsOrderComplete() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinyExchange)
        {
            try
            {
                var timestampNow = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                string data = "";
                var destinyAddress = destinyExchange.GetAddress();
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        data = string.Format("command=withdraw&currency={0}&amount={1}&address={2}&paymentId={3}", "XRP", amount.ToString("F8",CultureInfo.InvariantCulture), destinyAddress.XRP.address, destinyAddress.XRP.destination);
                        break;
                    case CoinName.BTC_STEEM:
                        data = string.Format("command=withdraw&currency={0}&amount={1}&address={2}&paymentId={3}", "STEEM", amount.ToString("F8",CultureInfo.InvariantCulture), destinyAddress.STEEM.address, destinyAddress.STEEM.destination);
                        break;
                    case CoinName.BTC:
                        data = string.Format("command=withdraw&currency={0}&amount={1}&address={2}", "BTC", amount.ToString("F8", CultureInfo.InvariantCulture), destinyAddress.BTC.address);
                        break;
                    default:
                        return new OperationResult<string> { Success = false, Message = "Poloniex only accepts XRP, STEEM coin" };
                }

                logger.Trace("Msg to sign: " + data);
                var signature = SignData(data);
                poloniexClient.DefaultRequestHeaders.Clear();
                poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
                poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
                request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = await poloniexClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_WITHDRAW_RESPONSE>(resultStrContent);
                if (resultObjContent.error != null) return new OperationResult<string> { Success = false, Message = resultObjContent.error };

                var TxId = await GetTxId(timestampNow);
                if(TxId==null) return new OperationResult<string> { Success = false, Message = "Error getting txid with in poloniex withdraw to "+destinyExchange.GetName() };

                return new OperationResult<string> { Success = true, Result = TxId };

            }
            catch (Exception ex)
            {
                return new OperationResult<string> { Success = false, Message = string.Format("Exception in Poloniex WithDraw() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }
        
        public override async Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            try
            {
                var endTimestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                var data = string.Format("command=returnDepositsWithdrawals&start={0}&end={1}", order.timestamp, endTimestamp);
                var signature = SignData(data);
                poloniexClient.DefaultRequestHeaders.Clear();
                poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
                poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
                request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

                var result = await poloniexClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                 var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_DEPOSITS_RESPONSE>(resultStrContent);
                if (resultObjContent.error != null) return new OperationResult<IDeposit> { Success = false, Message = resultObjContent.error };

                var newTransaction = resultObjContent.deposits.FirstOrDefault(e => e.GetAmount()==order.amount && e.status == "COMPLETE");
                if (newTransaction == null)
                {
                    return new OperationResult<IDeposit> { Success = true, Result = null };
                }

                return new OperationResult<IDeposit> { Success = true, Result = newTransaction };
            }
            catch (Exception ex)
            {
                return new OperationResult<IDeposit> { Success = false, Message = string.Format("Exception in Poloniex HasOrderArrived() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }            
        }

        public override async Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        return await Market(quantity, price, "BTC_XRP", "sell");
                    case CoinName.BTC_STEEM:
                        return await Market(quantity, price, "BTC_STEEM", "sell");
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Poloniex only accepts XRP, STEEM coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Poloniex PostSellOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        private async Task<OperationResult<IOrder>> Market(double quantity, double price, string currency, string orderType)
        {
            var data = string.Format("command={3}&currencyPair={0}&amount={1}&rate={2}", currency, quantity.ToString("F8", CultureInfo.InvariantCulture), price.ToString("F8", CultureInfo.InvariantCulture), orderType);
            var signature = SignData(data);
            poloniexClient.DefaultRequestHeaders.Clear();
            poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
            poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
            request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

            var result = await poloniexClient.SendAsync(request);
            string resultStrContent = await result.Content.ReadAsStringAsync();
            logger.Trace(resultStrContent);

            var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_BUY_ORDER>(resultStrContent);
            if (resultObjContent.error != null) return new OperationResult<IOrder> { Success = false, Message = resultObjContent.error };

            resultObjContent.SetCurrencySymbol(currency);
            return new OperationResult<IOrder> { Success = true, Result = resultObjContent };
        }

        public async Task<OperationResult<POLONIEX_BALANCES>> GetBalances()
        {
            var data = "command=returnBalances";
            var signature = SignData(data);
            poloniexClient.DefaultRequestHeaders.Clear();
            poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
            poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
            request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

            var result = await poloniexClient.SendAsync(request);
            string resultStrContent = await result.Content.ReadAsStringAsync();
            logger.Trace(resultStrContent);

            var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_BALANCES>(resultStrContent);
            if (resultObjContent.error != null) return new OperationResult<POLONIEX_BALANCES> { Success = false, Message = resultObjContent.error };
            
            return new OperationResult<POLONIEX_BALANCES> { Success = true, Result = resultObjContent };
        }

        public async Task<string> GetTxId(long timestampNow)
        {
            try
            {
                string txId = null;
                while (true)
                {
                    var timestampfinal = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                    var data = string.Format("command=returnDepositsWithdrawals&start={0}&end={1}",timestampNow,timestampfinal);
                    var signature = SignData(data);
                    poloniexClient.DefaultRequestHeaders.Clear();
                    poloniexClient.DefaultRequestHeaders.Add("Key", API_Key);
                    poloniexClient.DefaultRequestHeaders.Add("Sign", signature.signature);
                    var request = new HttpRequestMessage(HttpMethod.Post, "https://poloniex.com/tradingApi");
                    request.Content = new StringContent(signature.data, Encoding.UTF8, "application/x-www-form-urlencoded");

                    var result = await poloniexClient.SendAsync(request);
                    string resultStrContent = await result.Content.ReadAsStringAsync();
                    logger.Trace(resultStrContent);

                    var resultObjContent = JsonConvert.DeserializeObject<POLONIEX_WITHDRAWALS>(resultStrContent);
                    if (resultObjContent.error != null) return null;

                    var first = resultObjContent.withdrawals.FirstOrDefault(e => e.timestamp >= timestampNow);
                    var splot = first.status.Split(':');

                    if (splot.Length == 2 && splot[0] == "COMPLETE")
                    {
                        txId = splot[1].Replace(" ", string.Empty);
                        break;
                    }

                    await Task.Delay(1000);
                }

                return txId;
            }catch(Exception ex)
            {
                logger.Error(string.Format("Exception in Poloniex GetTxId() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                return null;
            }        
        }

        private SignaturePair SignData(string data)
        {
            var dataWithNonce = string.Format("nonce={1}&{0}", data, GetNonce());

            var sResult = Hash_HMAC_512(API_Secret, dataWithNonce);

            return new SignaturePair { data = dataWithNonce, signature = sResult };
        }

        public void setClient(HttpClient poloniexClient)
        {
            this.poloniexClient = poloniexClient;
        }
    }

    public class POLONIEX_COIN_DETAIL : ICoinDetail
    {
        public int id { get; set; }

        public string last { get; set; }

        public string lowestAsk { get; set; }

        public string highestBid { get; set; }

        public string percentChange { get; set; }

        public string baseVolume { get; set; }

        public string quoteVolume { get; set; }

        public string isFrozen { get; set; }

        public string high24hr { get; set; }

        public string low24hr { get; set; }

        public double getAsk()
        {
            return Convert.ToDouble(lowestAsk, CultureInfo.InvariantCulture);
        }

        public double getBid()
        {
            return Convert.ToDouble(highestBid, CultureInfo.InvariantCulture);
        }

        public double getLastPrice()
        {
            return Convert.ToDouble(last, CultureInfo.InvariantCulture);
        }
    }

    public class POLONIEX_API_MODEL
    {
        public POLONIEX_COIN_DETAIL BTC_XRP { get; set; }
    }

    public class POLONIEX_ORDERBOOK : IOrderBook
    {
        public POLONIEX_ORDERBOOK_PAIR[] asks { get; set; }

        public POLONIEX_ORDERBOOK_PAIR[] bids { get; set; }

        public string isFrozen { get; set; }

        public long seq { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return asks;
        }

        public IOrderBookPair[] GetBid()
        {
            return bids;
        }
    }

    [JsonConverter(typeof(POLONIEX_ORDERBOOK_CONVERTOR))]
    public class POLONIEX_ORDERBOOK_PAIR : IOrderBookPair
    {
        public string value { get; set; }

        public double quantity { get; set; }

        public double GetQuantity()
        {
            return quantity;
        }

        public double GetValue()
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
    }

    public class POLONIEX_ORDERBOOK_CONVERTOR : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(POLONIEX_ORDERBOOK_PAIR));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray ja = JArray.Load(reader);
            POLONIEX_ORDERBOOK_PAIR pop = new POLONIEX_ORDERBOOK_PAIR();
            pop.value = (string)ja[0];
            pop.quantity = (double)ja[1];
            return pop;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray ja = new JArray();
            POLONIEX_ORDERBOOK_PAIR pop = (POLONIEX_ORDERBOOK_PAIR)value;
            ja.Add(pop.value);
            ja.Add(pop.quantity);
            ja.WriteTo(writer);
        }
    }

    public class SignaturePair
    {
        public string data { get; set; }

        public string signature { get; set; }
        public string nonce { get; internal set; }
    }

    public class POLONIEX_BUY_ORDER : IOrder
    {
        public string error { get; set; }

        public long orderNumber { get; set; }

        private string Symbol { get; set; }

        public POLONIEX_BUY_ORDER_DETAILS[] resultingTrades { get; set; }

        public string GetCurrencySymbol()
        {
           return Symbol;
        }

        public long GetOrderId()
        {
            return orderNumber;
        }

        public string GetOrderId_String()
        {
            return ""+orderNumber;
        }

        public double GetTotalAmount()
        {
            return resultingTrades.Sum(e => Convert.ToDouble(e.amount, CultureInfo.InvariantCulture));
        }

        public void SetCurrencySymbol(string currency)
        {
            Symbol = currency;
        }
    }

    public class POLONIEX_BUY_ORDER_DETAILS
    {
        public string amount { get; set; }

        public string date { get; set; }

        public string rate { get; set; }

        public string total { get; set; }

        public string tradeID { get; set; }

        public string type { get; set; }
    }

    public class POLONIEX_OPEN_ORDERS
    {
        public string orderNumber { get; set; }

        public string type { get; set; }

        public string rate { get; set; }

        public string amount { get; set; }

        public string total { get; set; }
    }

    public class POLONIEX_WITHDRAW_RESPONSE
    {
        public string error { get; set; }

        public string response { get; set; }
    }

    public class POLONIEX_DEPOSITS_RESPONSE 
    {
        public string error { get; set; }

        public POLONIEX_DEPOSITS_DETAIL[] deposits { get; set; }
        
    }

    public class POLONIEX_WITHDRAWALS
    {
        public string error { get; set; }

        public POLONIEX_WITHDRAWALS_DETAIL[] withdrawals { get; set; }
    }

    public class POLONIEX_WITHDRAWALS_DETAIL
    {
        public long withdrawalNumber { get; set; }

        public string currency { get; set; }

        public string address { get; set; }

        public string amount { get; set; }

        public long timestamp { get; set; }

        public string status { get; set; }

        public string ipAddress { get; set; }
    }

    public class POLONIEX_DEPOSITS_DETAIL : IDeposit
    {
        public string currency { get; set; }

        public string address { get; set; }

        public string amount { get; set; }

        public string confirmations { get; set; }

        public string txid { get; set; }

        public string timestamp { get; set; }

        public string status { get; set; }

        public double GetAmount()
        {
            return Convert.ToDouble(amount, CultureInfo.InvariantCulture);
        }

        public string GetTxId()
        {
            throw new NotImplementedException();
        }
    }

    public class POLONIEX_BALANCES
    {
        public string error { get; set; }

        public string BTC { get; set; }

        public string XRP { get; set; }

        public string STEEM { get; set; }
    }
}
