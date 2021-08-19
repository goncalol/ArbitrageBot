using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
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
    public class Binance : Exchange
    {
        public HttpClient binanceClient;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string API_Key = "Redacted";
        private readonly string API_Secret = "Redacted";
        private Address address;
        public static readonly double FeePercent = 0.1;

        public Binance()
        {
            BTC_XLM = new Coin(CoinName.BTC_XLM, this);
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
            BTC_STEEM = new Coin(CoinName.BTC_STEEM, this);
            BTC_XEM = new Coin(CoinName.BTC_XEM, this);
            BTC_NEO = new Coin(CoinName.BTC_NEO, this);
            address = new Address
            {
                BTC = new AddressDetail { address = "Redacted" },
                XLM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                XRP = new AddressDetail { address = "Redacted", destination = "Redacted" },
                STEEM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                XEM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                NEO = new AddressDetail { address= "Redacted" }
            };
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXLM(exchange);
                compareXRP(exchange);
                compareSTEEM(exchange);
                compareXEM(exchange);
                compareNEO(exchange);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Binance", ex);
            }
        }

        public override Address GetAddress()
        {
            return address;
        }

        public override string GetName()
        {
            return "Binance";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream1 = await binanceClient.GetStringAsync("https://api.binance.com/api/v1/depth?symbol=XRPBTC&limit=5");
                        logger.Trace(stream1);
                        var res1 = JsonConvert.DeserializeObject<BINANCE_ORDERBOOK>(stream1);
                        //if (!res1.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res1.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res1 };
                    case CoinName.BTC_XLM:
                        var stream2 = await binanceClient.GetStringAsync("https://api.binance.com/api/v1/depth?symbol=XLMBTC&limit=5");
                        logger.Trace(stream2);
                        var res2 = JsonConvert.DeserializeObject<BINANCE_ORDERBOOK>(stream2);
                        //if (!res2.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res2.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res2 };
                    case CoinName.BTC_STEEM:
                        var stream3 = await binanceClient.GetStringAsync("https://api.binance.com/api/v1/depth?symbol=STEEMBTC&limit=5");
                        logger.Trace(stream3);
                        var res3 = JsonConvert.DeserializeObject<BINANCE_ORDERBOOK>(stream3);
                        //if (!res2.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res2.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res3 };
                    case CoinName.BTC_XEM:
                        var stream4 = await binanceClient.GetStringAsync("https://api.binance.com/api/v1/depth?symbol=XEMBTC&limit=5");
                        logger.Trace(stream4);
                        var res4 = JsonConvert.DeserializeObject<BINANCE_ORDERBOOK>(stream4);
                        //if (!res2.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res2.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res4 };
                    case CoinName.BTC_NEO:
                        var stream5 = await binanceClient.GetStringAsync("https://api.binance.com/api/v1/depth?symbol=NEOBTC&limit=5");
                        logger.Trace(stream5);
                        var res5 = JsonConvert.DeserializeObject<BINANCE_ORDERBOOK>(stream5);
                        //if (!res2.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res2.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res5 };
                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Binance only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Binance GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            try
            {
                var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var msgToSign = string.Format("timestamp={0}&recvWindow=10000000", timestamp);
                var endpoint = "https://api.binance.com/wapi/v3/depositHistory.html";
                var signature = SignData(endpoint,msgToSign);
                binanceClient.DefaultRequestHeaders.Clear();
                binanceClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", API_Key);
                var request = new HttpRequestMessage(HttpMethod.Get, signature);

                var result = await binanceClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BINANCE_DEPOSIT_HISTORY>(resultStrContent);
                if (!resultObjContent.success) return new OperationResult<IDeposit> { Success = false, Message = "" };

                var orderResult = resultObjContent.depositList.FirstOrDefault(o => o.txId == order.txId && o.status==1);//(0:pending,1:success)
                if (orderResult == null) return new OperationResult<IDeposit> { Success = true, Result = null };

                return new OperationResult<IDeposit> { Success = true, Result = orderResult };
            }
            catch (Exception ex)
            {
                return new OperationResult<IDeposit> { Success = false, Message = string.Format("Exception in Binance HasOrderArrived() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            try
            {
                var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var msgToSign = string.Format("recvWindow=10000000&orderId={0}&symbol={1}&timestamp={2}", order.GetOrderId(), order.GetCurrencySymbol(), timestamp);
                var endpoint = "https://api.binance.com/api/v3/order";
                var signature = SignData(endpoint, msgToSign);
                binanceClient.DefaultRequestHeaders.Clear();
                binanceClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", API_Key);
                var request = new HttpRequestMessage(HttpMethod.Get, signature);

                var result = await binanceClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BINANCE_ORDER>(resultStrContent);

                if(resultObjContent.msg!=null) return new OperationResult<bool> { Success = false, Message = resultObjContent.msg };
                if (resultObjContent.status!= "FILLED") return new OperationResult<bool> { Success = true, Result=false };

                return new OperationResult<bool> { Success = true, Result = true };                
            }
            catch (Exception ex)
            {
                return new OperationResult<bool> { Success = false, Message = string.Format("Exception in Binance IsOrderComplete() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            return await Market("BUY", coinName, price, quantity);
        }
        
        public override async Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            return await Market("SELL", coinName, price, quantity);
        }

        public override async Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinyExchange)
        {
            try
            {
                var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var symbol = "";
                var msgToSign = "";
                var destinyAddress = destinyExchange.GetAddress();

                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        //logger.Trace("Address to send: " + destinyAddress.XRP.address + " tag: " + destinyAddress.XRP.destination);
                        symbol = "XRP";
                        msgToSign = string.Format("name=Ripple&recvWindow=10000000&asset={0}&amount={1}&timestamp={2}&{3}", symbol, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp, AddressBuilder.Build(destinyExchange,"address="+ destinyAddress.XRP.address, "addressTag="+ destinyAddress.XRP.destination));
                        break;
                    case CoinName.BTC_XLM:
                       // logger.Trace("Address to send: " + destinyAddress.XLM.address + " tag: " + destinyAddress.XLM.destination);
                        symbol = "XLM";
                        msgToSign = string.Format("name=Stellar&recvWindow=10000000&asset={0}&amount={1}&timestamp={2}&{3}", symbol, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp, AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.XLM.address, "addressTag=" + destinyAddress.XLM.destination));
                        break;
                    case CoinName.BTC_STEEM:
                       // logger.Trace("Address to send: " + destinyAddress.STEEM.address + " tag: " + destinyAddress.STEEM.destination);
                        symbol = "STEEM";
                        msgToSign = string.Format("name=Steem&recvWindow=10000000&asset={0}&amount={1}&timestamp={2}&{3}", symbol, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp, AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.STEEM.address, "addressTag=" + destinyAddress.STEEM.destination));
                        break;
                    case CoinName.BTC_XEM:
                       // logger.Trace("Address to send: " + destinyAddress.XEM.address + " tag: " + destinyAddress.XEM.destination);
                        symbol = "XEM";
                        msgToSign = string.Format("name=Xem&recvWindow=10000000&asset={0}&amount={1}&timestamp={2}&{3}", symbol, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp, AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.XEM.address, "addressTag=" + destinyAddress.XEM.destination));
                        break;
                    case CoinName.BTC_NEO:
                        //logger.Trace("Address to send: " + destinyAddress.NEO.address);
                        symbol = "NEO";
                        msgToSign = string.Format("name=Neo&recvWindow=10000000&asset={0}&address={1}&amount={2}&timestamp={3}", symbol, destinyAddress.NEO.address, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp);
                        break;
                    case CoinName.BTC:
                       // logger.Trace("Address to send: " + destinyAddress.BTC.address);
                        symbol = "BTC";
                        msgToSign = string.Format("name=Bitcoin&recvWindow=10000000&asset={0}&address={1}&amount={2}&timestamp={3}", symbol, destinyAddress.BTC.address, amount.ToString("F8", CultureInfo.InvariantCulture), timestamp);
                        break;
                    default:
                        return new OperationResult<string> { Success = false, Message = "Binance only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }

                logger.Trace("Msg to sign: " + msgToSign);
                var endpoint = "https://api.binance.com/wapi/v3/withdraw.html";
                var signature = SignData(endpoint, msgToSign);
                binanceClient.DefaultRequestHeaders.Clear();
                binanceClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", API_Key);
                var request = new HttpRequestMessage(HttpMethod.Post, signature);

                var result = await binanceClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BINANCE_WITHDRAW>(resultStrContent);

                if (!resultObjContent.success) return new OperationResult<string> { Success = false, Message = resultObjContent.msg };
                var txId = await GetTxId(symbol, resultObjContent.id);
                if (txId == null) return new OperationResult<string> { Success = false, Message = "Could not get TxId of withdraw request" };
                return new OperationResult<string> { Success = true, Result = resultObjContent.id };
            }
            catch (Exception ex)
            {
                return new OperationResult<string> { Success = false, Message = string.Format("Exception in Binance WithDraw() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public async Task<string> GetTxId(string currency, string id)
        {
            BINANCE_WITHDRAW_HISTORY_DETAIL first = null;
            while (true)//SOLUÇAO MANHOSA: ESPERAR QUE O WITDRAW CHEGUE AO HISTORICO PARA DEVOLVER O TXID <-> CONSEQUENCIA: TA A QUEIMAR TEMPO QUANDO PODE A ORDEM TER JA CHEGADO À EXCHANGE DESTINO(IMPROVAVEL)
            {
                var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var msgToSign = string.Format("recvWindow=10000000&asset={0}&timestamp={1}",currency, timestamp);
                var endpoint = "https://api.binance.com/wapi/v3/withdrawHistory.html";
                var signature = SignData(endpoint, msgToSign);
                binanceClient.DefaultRequestHeaders.Clear();
                binanceClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", API_Key);
                var request = new HttpRequestMessage(HttpMethod.Get, signature);

                var result = await binanceClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BINANCE_WITHDRAW_HISTORY>(resultStrContent);
                first = resultObjContent.withdrawList.FirstOrDefault(e => e.id == id && e.txId!=null);

                if (first != null) break;
                await Task.Delay(1000);
            }

            return first.txId;
        }

        public void setClient(HttpClient binanceClient)
        {
            this.binanceClient = binanceClient;
        }

        private string SignData(string endpoint, string msgToSign)
        {
            var sResult = Hash_HMAC_256(API_Secret, msgToSign);

            return string.Format("{0}?{1}&signature={2}", endpoint, msgToSign, sResult);
        }

        public async Task<OperationResult<IOrder>> Market(string operation, CoinName coinName, double price, double quantity)
        {
            try
            {
                var symbol = "";
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        symbol = "XRPBTC";
                        break;
                    case CoinName.BTC_XLM:
                        symbol = "XLMBTC";
                        break;
                    case CoinName.BTC_STEEM:
                        symbol = "STEEMBTC";
                        break;
                    case CoinName.BTC_XEM:
                        symbol = "XEM";
                        break;
                    case CoinName.BTC_NEO:
                        symbol = "NEO";
                        break;
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Binance only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }

                var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                var msgToSign = string.Format("recvWindow=10000000&symbol={0}&side={1}&type=LIMIT&timeInForce=GTC&quantity={2}&price={3}&timestamp={4}", symbol, operation, quantity.ToString("F8", CultureInfo.InvariantCulture), price.ToString("F8", CultureInfo.InvariantCulture), timestamp);
                var endpoint = "https://api.binance.com/api/v3/order";
                var signature = SignData(endpoint, msgToSign);
                binanceClient.DefaultRequestHeaders.Clear();
                binanceClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", API_Key);
                var request = new HttpRequestMessage(HttpMethod.Post, signature);

                var result = await binanceClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BINANCE_ORDER>(resultStrContent);
                if (resultObjContent.clientOrderId == null) return new OperationResult<IOrder> { Success = false, Message = "" };

                return new OperationResult<IOrder> { Success = true, Result = resultObjContent };
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Binance PostSellOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

    }

    public class BINANCE_COIN_DETAIL : ICoinDetail
    {
        public string symbol { get; set; }

        public string bidPrice { get; set; }

        public string bidQty { get; set; }

        public string askPrice { get; set; }

        public string askQty { get; set; }

        public double getAsk()
        {
            return Convert.ToDouble(askPrice, CultureInfo.InvariantCulture);
        }

        public double getBid()
        {
            return Convert.ToDouble(bidPrice, CultureInfo.InvariantCulture);
        }

        public double getLastPrice()
        {
            throw new NotImplementedException();
        }
    }

    public class BINANCE_ORDERBOOK : IOrderBook
    {
        public long lastUpdateId { get; set; }

        public BINANCE_ORDERBOOK_PAIR[] asks { get; set; }

        public BINANCE_ORDERBOOK_PAIR[] bids { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return asks;
        }

        public IOrderBookPair[] GetBid()
        {
            return bids;
        }
    }

    [JsonConverter(typeof(BINANCE_ORDERBOOK_CONVERTOR))]
    public class BINANCE_ORDERBOOK_PAIR : IOrderBookPair
    {
        public string value { get; set; }

        public string quantity { get; set; }

        public double GetQuantity()
        {
            return Convert.ToDouble(quantity, CultureInfo.InvariantCulture);
        }

        public double GetValue()
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
    }

    public class BINANCE_ORDERBOOK_CONVERTOR : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BINANCE_ORDERBOOK_PAIR));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray ja = JArray.Load(reader);
            BINANCE_ORDERBOOK_PAIR pop = new BINANCE_ORDERBOOK_PAIR();
            pop.value = (string)ja[0];
            pop.quantity = (string)ja[1];
            return pop;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JArray ja = new JArray();
            BINANCE_ORDERBOOK_PAIR pop = (BINANCE_ORDERBOOK_PAIR)value;
            ja.Add(pop.value);
            ja.Add(pop.quantity);
            ja.WriteTo(writer);
        }
    }

    public class BINANCE_DEPOSIT_HISTORY
    {
        public BINANCE_DEPOSIT_DETAIL[] depositList { get; set; }

        public bool success { get; set; }
    }

    public class BINANCE_DEPOSIT_DETAIL : IDeposit
    {
        public long insertTime { get; set; }

        public double amount { get; set; }

        public string asset { get; set; }

        public string address { get; set; }

        public string txId { get; set; }

        public int status { get; set; }

        public double GetAmount()
        {
            return amount;
        }

        public string GetTxId()
        {
            return txId;
        }
    }

    public class BINANCE_ORDER : IOrder
    { 
        public int code { get; set; }

        public string msg { get; set; }

        public string symbol { get; set; }

        public long orderId { get; set; }

        public string clientOrderId { get; set; }

        public long transactTime { get; set; }

        public string price { get; set; }

        public string origQty { get; set; }

        public string executedQty { get; set; }

        public string status { get; set; }

        public string timeInForce { get; set; }

        public string type { get; set; }

        public string side { get; set; }

        public bool isWorking { get; set; }

        public string GetCurrencySymbol()
        {
            return symbol;
        }

        public long GetOrderId()
        {
            return orderId;
        }

        public string GetOrderId_String()
        {
            return orderId+"";
        }

        public double GetTotalAmount()
        {
            return ((Convert.ToDouble(executedQty, CultureInfo.InvariantCulture)) - (Binance.FeePercent / 100));
        }
    }

    public class BINANCE_WITHDRAW
    {
        public string msg { get; set; }

        public bool success { get; set; }

        public string id { get; set; }
    }

    public class BINANCE_WITHDRAW_HISTORY
    {
        public BINANCE_WITHDRAW_HISTORY_DETAIL[] withdrawList { get; set; }

        public bool success { get; set; }
    }

    public class BINANCE_WITHDRAW_HISTORY_DETAIL
    {
        public string id { get; set; }

        public double amount { get; set; }

        public string address { get; set; }

        public string asset { get; set; }

        public string txId { get; set; }

        public long applyTime { get; set; }

        public int status { get; set; }
    }
}
