using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using System.Linq;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;
using TraderBot.Exchanges.Orders;
using NLog;
using System.Globalization;

namespace TraderBot.Exchanges
{
    public class Bittrex : Exchange
    {
        public HttpClient bittrexClient;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Address address;
        private readonly string API_Key = "Redacted";
        private readonly string API_Secret = "Redacted";

        public Bittrex()
        {
            BTC_XLM = new Coin(CoinName.BTC_XLM,this);
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
            BTC_STEEM = new Coin(CoinName.BTC_STEEM,this);
            BTC_XEM = new Coin(CoinName.BTC_XEM, this);
            BTC_NEO = new Coin(CoinName.BTC_NEO, this);
            address = new Address { BTC = new AddressDetail { address= "Redacted" },
                                    XRP = new AddressDetail { address= "Redacted", destination= "Redacted" },
                                    XLM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                                    STEEM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                                    XEM = new AddressDetail { address = "Redacted", destination = "Redacted" },
                                    NEO = new AddressDetail { address = "Redacted" }
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
                throw new Exception("Error in Bittrex", ex);
            }
        }

        public override Address GetAddress()
        {
            return address;
        }

        public override string GetName()
        {
            return "Bittrex";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream1 = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-XRP&type=both");
                        logger.Trace(stream1);
                        var res1 = JsonConvert.DeserializeObject<Bittrex_OrderBook>(stream1);
                        if (!res1.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res1.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res1.result };
                    case CoinName.BTC_XLM:
                        var stream2 = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-XLM&type=both");
                        logger.Trace(stream2);
                        var res2 = JsonConvert.DeserializeObject<Bittrex_OrderBook>(stream2);
                        if (!res2.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res2.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res2.result };
                    case CoinName.BTC_STEEM:
                        var stream3 = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-STEEM&type=both");
                        logger.Trace(stream3);
                        var res3 = JsonConvert.DeserializeObject<Bittrex_OrderBook>(stream3);
                        if (!res3.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res3.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res3.result };
                    case CoinName.BTC_XEM:
                        var stream4 = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-XEM&type=both");
                        logger.Trace(stream4);
                        var res4 = JsonConvert.DeserializeObject<Bittrex_OrderBook>(stream4);
                        if (!res4.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res4.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res4.result };
                    case CoinName.BTC_NEO:
                        var stream5= await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getorderbook?market=BTC-NEO&type=both");
                        logger.Trace(stream5);
                        var res5 = JsonConvert.DeserializeObject<Bittrex_OrderBook>(stream5);
                        if (!res5.success) return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bittrex Error. Message:{0}.", res5.message) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res5.result };
                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Bittrex only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Bittrex GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            try
            {
                var currencyName = "";
                switch (order.currencyName)
                {
                    case CoinName.BTC_XRP:
                        currencyName = "XRP";
                        break;
                    case CoinName.BTC_XLM:
                        currencyName = "XLM";
                        break;
                    case CoinName.BTC_STEEM:
                        currencyName = "STEEM";
                        break;
                    case CoinName.BTC:
                        currencyName = "BTC";
                        break;
                    case CoinName.BTC_XEM:
                        currencyName = "XEM";
                        break;
                    case CoinName.BTC_NEO:
                        currencyName = "NEO";
                        break;
                    default:
                        return new OperationResult<IDeposit> { Success = false, Message = "Bittrex only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }
                var depositList = await GetDepositHistory(currencyName);
                if (!depositList.success) return new OperationResult<IDeposit> { Success = false, Message = depositList.message };
                
                var orderResult = depositList.result.FirstOrDefault(o => o.TxId==order.txId);//[{"Id":70158401,"Amount":5.26900000,"Currency":"STEEM","Confirmations":142,"LastUpdated":"2018-05-23T22:14:03.7","TxId":"8f3966ff4f3eff63faf1e96d743e25e39cd29928","CryptoAddress":"a8b531795ac241428b2"}]
                if (orderResult==null) return new OperationResult<IDeposit> { Success = true, Result = null };

                return new OperationResult<IDeposit> { Success = true, Result = orderResult };
            }
            catch (Exception ex)
            {
                return new OperationResult<IDeposit> { Success = false, Message = string.Format("Exception in Bittrex HasOrderArrived() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            try
            {
                var msg = string.Format("https://bittrex.com/api/v1.1/account/getorder?apikey={0}&uuid={1}", API_Key, order.GetOrderId_String());
                var signature = SignData(msg);
                bittrexClient.DefaultRequestHeaders.Clear();
                bittrexClient.DefaultRequestHeaders.Add("apisign", signature.signature);
                var request = new HttpRequestMessage(HttpMethod.Get, signature.data);

                var result = await bittrexClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<Bittrex_Order>(resultStrContent);
                if (!resultObjContent.success) return new OperationResult<bool> { Success = false, Message = resultObjContent.message };

                if(resultObjContent.result.IsOpen && resultObjContent.result.Closed==null)
                    return new OperationResult<bool> { Success = true, Result = false };

                return new OperationResult<bool> { Success = true, Result = true };
            }
            catch (Exception ex)
            {
                return new OperationResult<bool> { Success = false, Message = string.Format("Exception in Bittrex IsOrderComplete() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        return await Market("BTC-XRP", price, quantity, "buylimit");
                    case CoinName.BTC_XLM:
                        return await Market("BTC-XLM", price, quantity, "buylimit");
                    case CoinName.BTC_STEEM:
                        return await Market("BTC-STEEM", price, quantity, "buylimit");
                    case CoinName.BTC_XEM:
                        return await Market("BTC-XEM", price, quantity, "buylimit");
                    case CoinName.BTC_NEO:
                        return await Market("BTC-NEO", price, quantity, "buylimit");
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Bittrex only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Bittrex PostBuyOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        return await Market("BTC-XRP", price, quantity, "selllimit");
                    case CoinName.BTC_XLM:
                        return await Market("BTC-XLM", price, quantity, "selllimit");
                    case CoinName.BTC_STEEM:
                        return await Market("BTC-STEEM", price, quantity, "selllimit");
                    case CoinName.BTC_XEM:
                        return await Market("BTC-XEM", price, quantity, "selllimit");
                    case CoinName.BTC_NEO:
                        return await Market("BTC-NEO", price, quantity, "selllimit");
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Bittrex only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Bittrex PostSellOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinyExchange)
        {
            try
            {
                var symbol = "";
                var msg = "";
                var destinyAddress = destinyExchange.GetAddress();

                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        //logger.Trace("Address to send: " + destinyAddress.XRP.address + " tag: " + destinyAddress.XRP.destination);
                        symbol = "XRP";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&{3}", API_Key,symbol, amount.ToString("F8",CultureInfo.InvariantCulture), AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.XRP.address, "paymentid=" + destinyAddress.XRP.destination));
                        break;
                    case CoinName.BTC_XLM:
                        //logger.Trace("Address to send: " + destinyAddress.XLM.address + " tag: " + destinyAddress.XLM.destination);
                        symbol = "XLM";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&{3}", API_Key, symbol, amount.ToString("F8", CultureInfo.InvariantCulture), AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.XLM.address, "paymentid=" + destinyAddress.XLM.destination));
                        break;
                    case CoinName.BTC_STEEM:
                        //logger.Trace("Address to send: " + destinyAddress.STEEM.address + " tag: " + destinyAddress.STEEM.destination);
                        symbol = "STEEM";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&{3}", API_Key, symbol, amount.ToString("F8", CultureInfo.InvariantCulture), AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.STEEM.address, "paymentid=" + destinyAddress.STEEM.destination));
                        break;
                    case CoinName.BTC_XEM:
                        //logger.Trace("Address to send: " + destinyAddress.XEM.address + " tag: " + destinyAddress.XEM.destination);
                        symbol = "XEM";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&{3}", API_Key, symbol, amount.ToString("F8", CultureInfo.InvariantCulture), AddressBuilder.Build(destinyExchange, "address=" + destinyAddress.XEM.address, "paymentid=" + destinyAddress.XEM.destination));
                        break;
                    case CoinName.BTC_NEO:
                        //logger.Trace("Address to send: " + destinyAddress.NEO.address);
                        symbol = "NEO";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&address={3}", API_Key, symbol, amount.ToString("F8", CultureInfo.InvariantCulture), destinyAddress.NEO.address);
                        break;
                    case CoinName.BTC:
                        //logger.Trace("Address to send: " + destinyAddress.BTC.address);
                        symbol = "BTC";
                        msg = string.Format("https://bittrex.com/api/v1.1/account/withdraw?apikey={0}&currency={1}&quantity={2}&address={3}", API_Key, symbol, amount.ToString("F8", CultureInfo.InvariantCulture), destinyAddress.BTC.address);
                        break;
                    default:
                        return new OperationResult<string> { Success = false, Message = "Bittrex only accepts XRP, XLM, STEEM, XEM and NEO coin" };
                }

                logger.Trace("Msg to sign: " + msg);
                var signature = SignData(msg);
                bittrexClient.DefaultRequestHeaders.Clear();
                bittrexClient.DefaultRequestHeaders.Add("apisign", signature.signature);
                var request = new HttpRequestMessage(HttpMethod.Get, signature.data);

                var result = await bittrexClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<Bittrex_Buy_Order>(resultStrContent);
                if (!resultObjContent.success) return new OperationResult<string> { Success = false, Message = resultObjContent.message };

                var txId = await GetTxId(symbol, resultObjContent.result.uuid);
                if (txId == null) return new OperationResult<string> { Success = false, Message = "Could not get TxId of withdraw request" };
                return new OperationResult<string> { Success = true, Result=txId };
            }
            catch (Exception ex)
            {
                return new OperationResult<string> { Success = false, Message = string.Format("Exception in Bittrex WithDraw() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public async Task<string> GetTxId(string currency, string uuid)
        {
            Bittrex_Deposit first = null;
            while (true)//SOLUÇAO MANHOSA: ESPERAR QUE O WITDRAW CHEGUE AO HISTORICO PARA DEVOLVER O TXID <-> CONSEQUENCIA: TA A QUEIMAR TEMPO QUANDO PODE A ORDEM TER JA CHEGADO À EXCHANGE DESTINO(IMPROVAVEL)
            {
                var msg = string.Format("https://bittrex.com/api/v1.1/account/getwithdrawalhistory?apikey={0}&currency={1}", API_Key, currency);
                var signature = SignData(msg);
                bittrexClient.DefaultRequestHeaders.Clear();
                bittrexClient.DefaultRequestHeaders.Add("apisign", signature.signature);
                var request = new HttpRequestMessage(HttpMethod.Get, signature.data);

                var result = await bittrexClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BittrexCoinRes<Bittrex_Deposit[]>>(resultStrContent);
                if (!resultObjContent.success) return null;
            
                first = resultObjContent.result.FirstOrDefault(e=>e.PaymentUuid == uuid);

                if (first != null) break;
                await Task.Delay(1000);
            }

            return first.TxId;
        }

        private async Task<BittrexCoinRes<Bittrex_Deposit[]>> GetDepositHistory(string currency)
        {
            var msg = string.Format("https://bittrex.com/api/v1.1/account/getdeposithistory?apikey={0}&currency={1}", API_Key, currency);
            var signature = SignData(msg);
            bittrexClient.DefaultRequestHeaders.Clear();
            bittrexClient.DefaultRequestHeaders.Add("apisign", signature.signature);
            var request = new HttpRequestMessage(HttpMethod.Get, signature.data);

            var result = await bittrexClient.SendAsync(request);
            string resultStrContent = await result.Content.ReadAsStringAsync();
            logger.Trace(resultStrContent);

            var resultObjContent = JsonConvert.DeserializeObject<BittrexCoinRes<Bittrex_Deposit[]>>(resultStrContent);
            return resultObjContent;
        }

        private async Task<OperationResult<IOrder>> Market(string currency,double price, double quantity, string orderType)
        {
            var msg = string.Format("https://bittrex.com/api/v1.1/market/{3}?apikey={0}&market={4}&quantity={1}&rate={2}", API_Key, quantity.ToString(CultureInfo.InvariantCulture), price.ToString("F8", CultureInfo.InvariantCulture).TrimEnd('0'), orderType, currency);
            var signature = SignData(msg);
            bittrexClient.DefaultRequestHeaders.Clear();
            bittrexClient.DefaultRequestHeaders.Add("apisign", signature.signature);
            var request = new HttpRequestMessage(HttpMethod.Get, signature.data);

            var result = await bittrexClient.SendAsync(request);
            string resultStrContent = await result.Content.ReadAsStringAsync();
            logger.Trace(resultStrContent);

            var resultObjContent = JsonConvert.DeserializeObject<Bittrex_Buy_Order>(resultStrContent);
            if (!resultObjContent.success) return new OperationResult<IOrder> { Success = false, Message = resultObjContent.message };

            resultObjContent.SetAmmount(quantity);
            return new OperationResult<IOrder> { Success = true, Result = resultObjContent };
        }

        public void setClient(HttpClient bittrexClient)
        {
            this.bittrexClient = bittrexClient;
        }

        private SignaturePair SignData(string data)
        {
            var dataWithNonce = string.Format("{0}&nonce={1}", data, GetNonce());

            var sResult = Hash_HMAC_512(API_Secret, dataWithNonce);

            return new SignaturePair { data = dataWithNonce, signature = sResult };
        }
    }

    public class BittrexCoinRes<T>
    {
        public bool success { get; set; }

        public string message { get; set; }

        public T result { get; set; }

    }

    public class BittrexResult : ICoinDetail
    {
        public double Bid { get; set; }

        public double Ask { get; set; }

        public double Last { get; set; }

        public double getAsk()
        {
            return Ask;
        }

        public double getBid()
        {
            return Bid;
        }

        public double getLastPrice()
        {
            return Last;
        }
    }

    public class Bittrex_OrderBook 
    {
        public bool success { get; set; }

        public string message { get; set; }

        public Bittrex_OrderBook_Result result { get; set; }
        
    }

    public class Bittrex_OrderBook_Result : IOrderBook
    {
        public Bittrex_OrderBook_Result_Pair[] buy { get; set; }

        public Bittrex_OrderBook_Result_Pair[] sell { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return sell;
        }

        public IOrderBookPair[] GetBid()
        {
            return buy;
        }
    }

    public class Bittrex_OrderBook_Result_Pair : IOrderBookPair
    {
        public double Quantity { get; set; }

        public double Rate { get; set; }

        public double GetQuantity()
        {
            return Quantity;
        }

        public double GetValue()
        {
            return Rate;
        }
    }

    public class Bittrex_Buy_Order : IOrder
    {
        public bool success { get; set; }

        public string message { get; set; }

        private double ammount { get; set; }

        public Bittrex_Buy_Order_Result result { get; set; }

        public long GetOrderId()
        {
            throw new NotImplementedException();
        }

        public string GetOrderId_String()
        {
            return result.uuid;
        }

        public double GetTotalAmount()
        {
            return ammount;
        }

        public void SetAmmount(double amount)
        {
            ammount = amount;
        }

        public string GetCurrencySymbol()
        {
            throw new NotImplementedException();
        }
    }

    public class Bittrex_Buy_Order_Result
    {
        public string uuid { get; set; }
    }

    public class Bittrex_Order
    {
        public bool success { get; set; }

        public string message { get; set; }

        public Bittrex_Order_Detail result { get; set; }
    }

    public class Bittrex_Order_Detail
    {
        public string OrderUuid { get; set; }

        public string Closed { get; set; }

        public bool IsOpen { get; set; }
    }

    public class Bittrex_Deposit : IDeposit
    {
        public string PaymentUuid { get; set; }

        public string Currency { get; set; }

        public double Amount { get; set; }

        public string Address { get; set; }

        public string Opened { get; set; }

        public bool Authorized { get; set; }

        public bool PendingPayment { get; set; }

        public double TxCost { get; set; }

        public string TxId { get; set; }

        public bool Canceled { get; set; }

        public bool InvalidAddress { get; set; }

        public double GetAmount()
        {
            return Amount;
        }

        public string GetTxId()
        {
            return TxId;
        }
    }
}
