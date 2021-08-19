using Newtonsoft.Json;
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
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;
using TraderBot.Exchanges.Orders;

namespace TraderBot.Exchanges
{
    public class Bithumb : Exchange
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public HttpClient bithumbClient;
        private Address address;
        private readonly string API_Key = "Redacted";
        private readonly string API_Secret = "Redacted";


        public Bithumb()
        {
            BTC_XRP = new Coin(CoinName.BTC_XRP, this);
            address = new Address
            {
                XRP = new AddressDetail { address = "Redacted", destination = "Redacted" }
            };
        }

        public override void comparePairs(IExchange exchange)
        {
            try
            {
                compareXRP(exchange);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Bithumb ({0})", exchange.GetName()), ex);
            }
        }

        public override Address GetAddress()
        {
            return address;
        }

        public override string GetName()
        {
            return "Bithumb";
        }

        public override async Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var stream = await bithumbClient.GetStringAsync("https://api.bithumb.com/public/orderbook/XRP?count=5");
                        logger.Trace(stream);
                        var res = JsonConvert.DeserializeObject<Bithumb_OrderBook>(stream);
                        if (res.status != "0000") return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bithumb Error. Error Code:{0}.",res.status) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res.data };
                    case CoinName.BTC:
                        var stream2 = await bithumbClient.GetStringAsync("https://api.bithumb.com/public/orderbook/BTC?count=5");
                        logger.Trace(stream2);
                        var res2 = JsonConvert.DeserializeObject<Bithumb_OrderBook>(stream2);
                        if (res2.status != "0000") return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Get OrderBook from Bithumb Error. Error Code:{0}.", res2.status) };
                        return new OperationResult<IOrderBook> { Success = true, Result = res2.data };
                    default:
                        return new OperationResult<IOrderBook> { Success = false, Message = "Bithumb only accepts XRP and BTC coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrderBook> { Success = false, Message = string.Format("Exception in Bithumb GetOrderBookFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order)
        {
            try
            {
                var endpoint = "/info/user_transactions";
                var data = string.Format("searchGb=4&currency=XRP&endpoint={0}", Uri.EscapeDataString(endpoint));
                var signature = SignData(data, endpoint);
                bithumbClient.DefaultRequestHeaders.Clear();
                bithumbClient.DefaultRequestHeaders.Add("Api-Key", API_Key);
                bithumbClient.DefaultRequestHeaders.Add("Api-Sign", signature.signature);
                bithumbClient.DefaultRequestHeaders.Add("Api-Nonce", signature.nonce);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bithumb.com"+ endpoint);
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content.Headers.Add(@"Content-Length", Encoding.ASCII.GetBytes(data).Length.ToString());

                var result = await bithumbClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BithumbRes<Bithumb_User_Transactions[]>>(resultStrContent);
                if (resultObjContent.status != "0000")
                {
                    return new OperationResult<IDeposit> { Success = false, Message = string.Format("Error in HasOrderArrived() status code:{0}", resultObjContent.status) };
                }

                var newTransaction = resultObjContent.data.FirstOrDefault(e=>e.transfer_date>=order.timestamp);
                if(newTransaction == null)
                {
                    return new OperationResult<IDeposit> { Success = true, Result = null };
                }
                return new OperationResult<IDeposit> { Success = true, Result = newTransaction };
            }
            catch (Exception ex)
            {
                return new OperationResult<IDeposit> { Success = false, Message = string.Format("Exception in Bithumb HasOrderArrived() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<bool>> IsOrderComplete(IOrder order)
        {
            try
            {
                var endpoint = "/info/orders";
                //!!!!!!!!!!!!!!!!!!!!!!!!!!! o type "ask" e currency "xrp" em baixo nao estao genericos !!!!!!!!!!!!!!!!!!
                var data = string.Format("order_id={0}&type=ask&after=1525963515000&currency=XRP&endpoint={1}", order.GetOrderId_String(), Uri.EscapeDataString(endpoint));
                var signature = SignData(data, endpoint);
                bithumbClient.DefaultRequestHeaders.Clear();
                bithumbClient.DefaultRequestHeaders.Add("Api-Key", API_Key);
                bithumbClient.DefaultRequestHeaders.Add("Api-Sign", signature.signature);
                bithumbClient.DefaultRequestHeaders.Add("Api-Nonce", signature.nonce);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bithumb.com" + endpoint);
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content.Headers.Add(@"Content-Length", Encoding.ASCII.GetBytes(data).Length.ToString());

                var result = await bithumbClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<BithumbRes<Bithumb_Orders[]>>(resultStrContent);

                if(resultObjContent.status == "5600") //é aquela msg em koreano a dizer que nao ha transaçoes em andamento
                {
                    return new OperationResult<bool> { Success = true, Result = true };
                }

                if (resultObjContent.status != "0000")
                {
                    return new OperationResult<bool> { Success = false, Message = string.Format("Error in Bithumb IsOrderComplete() status code:{0}", resultObjContent.status) };
                }

                var newTransaction = resultObjContent.data.FirstOrDefault(e => e.order_id == order.GetOrderId_String());
                if (newTransaction == default(Bithumb_Orders))
                {
                    return new OperationResult<bool> { Success = false, Result = false, Message=string.Format("order id({0}) does not exist!", order.GetOrderId_String()) };
                }
                else
                {
                    if (newTransaction.date_completed != null)
                    {
                        return new OperationResult<bool> { Success = true, Result = true };
                    }
                    return new OperationResult<bool> { Success = true, Result = false };
                }
                
            }
            catch (Exception ex)
            {
                return new OperationResult<bool> { Success = false, Message = string.Format("Exception in bithumb IsOrderComplete() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {                
                var endpoint = "/trade/place";
                //price = price;
                var data = string.Format("order_currency=BTC&Payment_currency=KRW&price={0}&type=bid&units={1}&endpoint={2}", price.ToString(), quantity.ToString(CultureInfo.InvariantCulture), Uri.EscapeDataString(endpoint));
                var signature = SignData(data, endpoint);
                bithumbClient.DefaultRequestHeaders.Clear();
                bithumbClient.DefaultRequestHeaders.Add("Api-Key", API_Key);
                bithumbClient.DefaultRequestHeaders.Add("Api-Sign", signature.signature);
                bithumbClient.DefaultRequestHeaders.Add("Api-Nonce", signature.nonce);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bithumb.com" + endpoint);
                request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content.Headers.Add(@"Content-Length", Encoding.ASCII.GetBytes(data).Length.ToString());

                var result = await bithumbClient.SendAsync(request);
                string resultStrContent = await result.Content.ReadAsStringAsync();
                logger.Trace(resultStrContent);

                var resultObjContent = JsonConvert.DeserializeObject<Bithumb_Sell_Order>(resultStrContent);
                if (resultObjContent.status != "0000")
                {
                    return new OperationResult<IOrder> { Success = false, Message = string.Format("Error in PostBuyOrderFor() status code:{0}", resultObjContent.status) };
                }
                return new OperationResult<IOrder> { Success = true, Result = resultObjContent };
            }
            catch (Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Bithumb PostBuyOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public override async Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        var endpoint = "/trade/place";
                        price = price / Validations.KRW_to_BTC;
                        var data = string.Format("order_currency=XRP&Payment_currency=KRW&price={0}&type=ask&units={1}&endpoint={2}", price, quantity.ToString(CultureInfo.InvariantCulture), Uri.EscapeDataString(endpoint));
                        var signature = SignData(data, endpoint);
                        bithumbClient.DefaultRequestHeaders.Clear();
                        bithumbClient.DefaultRequestHeaders.Add("Api-Key", API_Key);
                        bithumbClient.DefaultRequestHeaders.Add("Api-Sign", signature.signature);
                        bithumbClient.DefaultRequestHeaders.Add("Api-Nonce", signature.nonce);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bithumb.com" + endpoint);
                        request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                        request.Content.Headers.Add(@"Content-Length", Encoding.ASCII.GetBytes(data).Length.ToString());

                        var result = await bithumbClient.SendAsync(request);
                        string resultStrContent = await result.Content.ReadAsStringAsync();
                        logger.Trace(resultStrContent);

                        var resultObjContent = JsonConvert.DeserializeObject<Bithumb_Sell_Order>(resultStrContent);
                        if (resultObjContent.status != "0000")
                        {
                            return new OperationResult<IOrder> { Success = false, Message = string.Format("Error in PostSellOrderFor() status code:{0}", resultObjContent.status) };
                        }
                        return new OperationResult<IOrder> { Success = true, Result = resultObjContent };
                    default:
                        return new OperationResult<IOrder> { Success = false, Message = "Bithumb only accepts XRP coin" };
                }

            }catch(Exception ex)
            {
                return new OperationResult<IOrder> { Success = false, Message = string.Format("Exception in Bithumb PostSellOrderFor() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }

        public void setClient(HttpClient bithumbClient)
        {
            this.bithumbClient = bithumbClient;
        }

        public override async Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinationExchange)
        {
            try
            {
                switch (coinName)
                {
                    case CoinName.BTC_XRP:
                        if(amount<0.001) return new OperationResult<string> { Success = false, Message = string.Format("Nao ha fundos suficientes para enviar btc: {0}", amount) };
                        logger.Trace(string.Format("address:{0} || destination:{1}",address.XRP.address,address.XRP.destination));
                        var endpoint = "/trade/btc_withdrawal";
                        var data = string.Format("units={0}&address={1}&currency=BTC&endpoint={2}", amount.ToString(CultureInfo.InvariantCulture), address.XRP.address, Uri.EscapeDataString(endpoint));
                        var signature = SignData(data, endpoint);
                        bithumbClient.DefaultRequestHeaders.Clear();
                        bithumbClient.DefaultRequestHeaders.Add("Api-Key", API_Key);
                        bithumbClient.DefaultRequestHeaders.Add("Api-Sign", signature.signature);
                        bithumbClient.DefaultRequestHeaders.Add("Api-Nonce", signature.nonce);
                        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bithumb.com" + endpoint);
                        request.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                        request.Content.Headers.Add(@"Content-Length", Encoding.ASCII.GetBytes(data).Length.ToString());

                        var result = await bithumbClient.SendAsync(request);
                        string resultStrContent = await result.Content.ReadAsStringAsync();
                        logger.Trace(resultStrContent);

                        var resultObjContent = JsonConvert.DeserializeObject<Bithumb_Sell_Order>(resultStrContent);
                        if (resultObjContent.status != "0000")
                        {
                            return new OperationResult<string> { Success = false, Message = string.Format("Error in WithDraw() status code:{0}", resultObjContent.status) };
                        }

                        return new OperationResult<string> { Success = true, Result = "Withdraw Success!" };
                    default:
                        return new OperationResult<string> { Success = false, Message = "Bithumb only accepts XRP coin" };
                }
            }
            catch (Exception ex)
            {
                return new OperationResult<string> { Success = false, Message = string.Format("Exception in Bithumb WithDraw() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : "") };
            }
        }
        
        private SignaturePair SignData(string data, string endpoint)
        {
            var nonce = GetNonce().ToString();
            var dataWithNonce = endpoint + (char)0 + data + (char)0 + nonce;

            var sResult = Convert.ToBase64String(StringToByte(Hash_HMAC_512(API_Secret, dataWithNonce)));

            return new SignaturePair { data = data, signature = sResult, nonce = nonce };
        }
    }

    public class BithumbRes<T>
    {
        public string status { get; set; }

        public T data { get; set; }
    }

    public class BithumbCoin : ICoinDetail
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public string opening_price { get; set; }

        public string closing_price { get; set; }

        public string min_price { get; set; }

        public string max_price { get; set; }

        public string average_price { get; set; }

        public string units_traded { get; set; }

        public string volume_1day { get; set; }

        public string volume_7day { get; set; }

        public string buy_price { get; set; }

        public string sell_price { get; set; }

        public string date { get; set; }

        public double getAsk()
        {
            return Convert.ToDouble(sell_price, CultureInfo.InvariantCulture) * Validations.KRW_to_BTC;
        }

        public double getBid()
        {
            return Convert.ToDouble(buy_price, CultureInfo.InvariantCulture) * Validations.KRW_to_BTC;
        }

        public double getLastPrice()
        {
            //logger.Trace("convertor:"+ Utils.KRW_to_BTC);
            return Convert.ToDouble(closing_price, CultureInfo.InvariantCulture) * Validations.KRW_to_BTC;
        }
    }

    public class Bithumb_OrderBook {
        
        public string status { get; set; }

        public Bithumb_OrderBook_Data data { get; set; }
    }

    public class Bithumb_OrderBook_Data : IOrderBook
    {
        public string timestamp { get; set; }

        public string payment_currency { get; set; }

        public string order_currency { get; set; }

        public Bithumb_OrderBook_Data_Pairs[] bids { get; set; }

        public Bithumb_OrderBook_Data_Pairs[] asks { get; set; }

        public IOrderBookPair[] GetAsk()
        {
            return bids;
        }

        public IOrderBookPair[] GetBid()
        {
            return asks;
        }
    }

    public class Bithumb_OrderBook_Data_Pairs :IOrderBookPair
    {
        public string quantity { get; set; }

        public string price { get; set; }

        public double GetQuantity()
        {
            return Convert.ToDouble(quantity, CultureInfo.InvariantCulture);
        }

        public double GetValue()
        {
            return Convert.ToDouble(price, CultureInfo.InvariantCulture) * Validations.KRW_to_BTC;
        }
    }

    public class Bithumb_User_Transactions: IDeposit
    {
        public string search { get; set; }

        public long transfer_date { get; set; }

        public string units { get; set; }

        public string price { get; set; }

        public string btc1krw { get; set; }

        public string fee { get; set; }

        public string btc_remain { get; set; }

        public string krw_remain { get; set; }

        public double GetAmount()
        {
            return Convert.ToDouble(units.Remove(0, 2), CultureInfo.InvariantCulture);
        }

        public string GetTxId()
        {
            throw new NotImplementedException();
        }
    }

    public class Bithumb_Sell_Order : IOrder
    {
        public string status { get; set; }

        public string order_id { get; set; }

        public Bithumb_Sell_Order_Detail[] data { get; set; }

        public string GetCurrencySymbol()
        {
            throw new NotImplementedException();
        }

        public long GetOrderId()
        {
            throw new NotImplementedException();
        }

        public string GetOrderId_String()
        {
            return order_id;
        }

        public double GetTotalAmount()
        {
            return Convert.ToDouble(data.Sum(e => e.total), CultureInfo.InvariantCulture);
        }
    }

    public class Bithumb_Sell_Order_Detail
    {
        public string cont_id { get; set; }

        public string units { get; set; }

        public string price { get; set; }

        public int total { get; set; }

        public int fee { get; set; }
    }

    public class Bithumb_Orders
    {
        public string order_id { get; set; }

        public string order_currency { get; set; }

        public long order_date { get; set; }

        public string payment_currency { get; set; }

        public string type { get; set; }

        public string status { get; set; }

        public string units { get; set; }

        public string units_remaining { get; set; }

        public string price { get; set; }

        public string fee { get; set; }

        public string total { get; set; }

        public string date_completed { get; set; }
    }
}
