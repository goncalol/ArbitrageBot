using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using PusherClient;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TraderBot.Exchanges;
using WebSocketSharp;

namespace TraderBot
{
    public class Program
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly HttpClient poloniexClient = new HttpClient();
        private static readonly HttpClient bittrexClient = new HttpClient();
        private static readonly HttpClient binanceClient = new HttpClient();
        private static readonly HttpClient cryptopiaClient = new HttpClient();
        private static readonly HttpClient okexClient = new HttpClient();
        private static readonly HttpClient bithumbClient = new HttpClient();
        private static readonly HttpClient utilsClient = new HttpClient();

        public static WebSocket bitfinexSocket;
        private static readonly int delayTimeApiUnavailable = 5;//5min
        public static bool convertorReady = false;

        static void Main(string[] args)
        {
            logger.Debug("start application....");

            Validations.initValues();

            var poloniex = new Poloniex();
            var bittrex = new Bittrex();
            var cryptopia = new Cryptopia();
            var bitfinex = new Bitfinex();
            var Bitstamp = new Bitstamp();
            var Okex = new Okex();
            var Bithumb = new Bithumb();
            var Binance = new Binance();
            //var exchanges = new IExchange[] { poloniex, bittrex, bitfinex, Okex, Bithumb };
            var exchanges = new IExchange[] { bittrex, Binance, poloniex };

           // Utils();

            BittrexRequests(bittrex, 1000);
            BinanceRequests(Binance, 1000);
            PoloniexRequests(poloniex, 1000);
            //PoloniexRequests(poloniex, 500);
            ////CryptopiaRequests(cryptopia, 500); //NÃO TEM XRP, NEM STEEM, NEM XLM
            //BitfinexRequests(bitfinex, false);
            //BitstampRequests(Bitstamp);
            //OkexRequests(Okex, 500);
            //BithumbRequests(Bithumb,500);

            CompareCoinPairs(exchanges);

            Console.Read();

            logger.Debug("end application....");
        }

        public static async Task PoloniexRequests(Poloniex poloniex, int waitime)
        {
            bool delay = false;
            poloniex.setClient(poloniexClient);

            while (true)
            {
                try
                {
                    if (delay)
                    {
                        logger.Debug(string.Format("Poloniex is out. Going to wait for 5 min(stamp:{0})",DateTime.Now));
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        logger.Debug(string.Format("Poloniex awake from wait delay(stamp:{0})", DateTime.Now));
                        delay = false;
                    }

                    //!!!!!!!!!PEDIDO AS VEZES DEMORA 7 SEGUNDOS A RETORNAR !!!!!!!!!!!!!!!!!!!!!!!!
                    poloniexClient.DefaultRequestHeaders.Accept.Clear();
                    var stream = await poloniexClient.GetStringAsync("https://poloniex.com/public?command=returnTicker");
                    
                    var exchangeTick = JsonConvert.DeserializeObject<POLONIEX_API_MODEL>(stream);
                    
                    poloniex.SetXRP(exchangeTick.BTC_XRP);
                    //poloniex.SetXLM(exchangeTick.BTC_XRP);
                    if (!poloniex.IsActive()) poloniex.Activate();

                    Console.Write(string.Format("Poloniex XRP last:{0}\n", exchangeTick.BTC_XRP.getLastPrice()));

                    await Task.Delay(waitime);
                }
                catch(Exception ex)
                {
                    logger.Error(string.Format("(PoloniexRequests) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));

                    poloniex.Deactivate();
                    delay = true;
                }
            }
        }

        public static async Task BittrexRequests(Bittrex bitterx, int waitime)
        {
            bool delay = false;
            bitterx.setClient(bittrexClient);

            while (true)
            {
                try
                {
                    if (delay)
                    {
                        logger.Debug(string.Format("Bittrex is out. Going to wait for 5 min(stamp:{0})", DateTime.Now));
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        logger.Debug(string.Format("Bittrex awake from wait delay(stamp:{0})", DateTime.Now));
                        delay = false;
                    }

                    bittrexClient.DefaultRequestHeaders.Accept.Clear();
                    var XLMstr = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-XLM");
                    var last_tick_XLM = DateTime.Now;

                    var XRPstr = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-XRP");
                    var last_tick_xrp = DateTime.Now;

                    var STEEMstr = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-STEEM");
                    var last_tick_steem = DateTime.Now;

                    var XEMstr = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-XEM");
                    var last_tick_XEM = DateTime.Now;

                    var NEOstr = await bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-NEO");
                    var last_tick_NEO = DateTime.Now;

                    var XLM = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(XLMstr);
                    var XRP = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(XRPstr);
                    var STEEM = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(STEEMstr);
                    var XEM = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(XEMstr);
                    var NEO = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(NEOstr);

                    bitterx.SetXLM(XLM.result,last_tick_XLM);
                    bitterx.SetXRP(XRP.result,last_tick_xrp);
                    bitterx.SetSTEEM(STEEM.result, last_tick_steem);
                    bitterx.SetXEM(XEM.result, last_tick_XEM);
                    bitterx.SetNEO(NEO.result, last_tick_NEO);

                    if (!bitterx.IsActive()) bitterx.Activate();

                    Console.WriteLine(string.Format("Bittrex(XLM) last:{0}", XLM.result.Last));
                    Console.WriteLine(string.Format("Bittrex(XRP) last:{0}", XRP.result.Last));
                    Console.WriteLine(string.Format("Bittrex(STEEM) last:{0}", STEEM.result.Last));
                    Console.WriteLine(string.Format("Bittrex(XEM) last:{0}", XEM.result.Last));
                    Console.WriteLine(string.Format("Bittrex(NEO) last:{0}", NEO.result.Last));

                    //Thread.Sleep(waitime);
                    await Task.Delay(waitime);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("(BittrexRequests) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    bitterx.Deactivate();
                    delay = true;
                }
            }
        }

        private static async Task BinanceRequests(Binance binance, int waitime)
        {
            bool delay = false;
            binance.setClient(binanceClient);

            while (true)
            {
                try
                {
                    if (delay)
                    {
                        logger.Debug(string.Format("Binance is out. Going to wait for 5 min(stamp:{0})", DateTime.Now));
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        logger.Debug(string.Format("Binance awake from wait delay(stamp:{0})", DateTime.Now));
                        delay = false;
                    }

                    binanceClient.DefaultRequestHeaders.Accept.Clear();
                    var XLMstr = await binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=XLMBTC");
                    var last_tick_XLM = DateTime.Now;

                    var XRPstr = await binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=XRPBTC");
                    var last_tick_xrp = DateTime.Now;

                    var STEEMstr = await binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=STEEMBTC");
                    var last_tick_steem = DateTime.Now;

                    var XEMstr = await binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=XEMBTC");
                    var last_tick_xem = DateTime.Now;

                    var NEOstr = await binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=NEOBTC");
                    var last_tick_neo = DateTime.Now;

                    var XLM = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(XLMstr);
                    var XRP = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(XRPstr);
                    var STEEM = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(STEEMstr);
                    var XEM = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(XEMstr);
                    var NEO = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(NEOstr);

                    binance.SetXLM(XLM, last_tick_XLM);
                    binance.SetXRP(XRP, last_tick_xrp);
                    binance.SetSTEEM(STEEM, last_tick_steem);
                    binance.SetXEM(XEM, last_tick_xem);
                    binance.SetNEO(NEO, last_tick_neo);

                    if (!binance.IsActive()) binance.Activate();

                    Console.WriteLine(string.Format("Binance(XLM) ask:{0} || bid:{1}", XLM.askPrice, XLM.bidPrice));
                    Console.WriteLine(string.Format("Binance(XRP) ask:{0} || bid:{1}", XRP.askPrice, XRP.bidPrice));
                    Console.WriteLine(string.Format("Binance(STEEM) ask:{0} || bid:{1}", STEEM.askPrice,STEEM.bidPrice));
                    Console.WriteLine(string.Format("Binance(XEM) ask:{0} || bid:{1}", XEM.askPrice, XEM.bidPrice));
                    Console.WriteLine(string.Format("Binance(NEO) ask:{0} || bid:{1}", NEO.askPrice, NEO.bidPrice));

                    //Thread.Sleep(waitime);
                    await Task.Delay(waitime);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("(BinanceRequests) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    binance.Deactivate();
                    delay = true;
                }
            }
        }

        public static async Task CryptopiaRequests(Cryptopia cryptopia, int waitime)
        {
            bool delay = false;
            while (true)
            {
                try
                {
                    if (delay)
                    {
                        logger.Debug(string.Format("Cryptopia is out. Going to wait for 5 min(stamp:{0})", DateTime.Now));
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        logger.Debug(string.Format("Cryptopia awake from wait delay(stamp:{0})", DateTime.Now));
                        delay = false;
                    }
                    cryptopiaClient.DefaultRequestHeaders.Accept.Clear();
                    var LTCstr = await cryptopiaClient.GetStringAsync("https://www.cryptopia.co.nz/api/GetMarket/LTC_BTC");
                    var last_ticket_LTC = DateTime.Now;

                    var LTC = JsonConvert.DeserializeObject<CRYPTOPIA_COIN>(LTCstr);

                    cryptopia.SetXLM(LTC.Data,last_ticket_LTC);
                    if (!cryptopia.IsActive()) cryptopia.Activate();

                    Console.WriteLine(string.Format("Cryptopia(LTC) last:{0:0.00000000}", LTC.Data.LastPrice));

                    await Task.Delay(waitime);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("(CryptopiaRequests) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    cryptopia.Deactivate();
                    delay = true;
                }
            }
        }

        public static async Task BitfinexRequests(Bitfinex bitfinex, bool waitBeforeCreateSocket)
        {
            logger.Trace(string.Format("BitfinexRequests start task"));

            if (waitBeforeCreateSocket)
            {
                logger.Debug(string.Format("Bitfinex is out. Going to wait for 5 min(stamp:{0})", DateTime.Now));
                await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                logger.Debug(string.Format("Bitfinex awake from wait delay(stamp:{0})", DateTime.Now));
            }

            bitfinexSocket = new WebSocket("wss://api.bitfinex.com/ws");

            bitfinexSocket.OnMessage += (sender, e) =>
            {
                var token = JToken.Parse(e.Data);
                if(token.Type == JTokenType.Array)
                {
                    try
                    {
                        bitfinex.update(token.ToObject<double[]>());
                        if(!bitfinex.IsActive()) bitfinex.Activate();
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(string.Format("Could not update bitfinex with msg:\n{0}",token.ToString()));
                    }
                }
                else if(token.Type == JTokenType.Object)
                {
                    var val = token.ToObject<BitfinexEventHelper>();

                    if (val.@event == "subscribed")
                    {
                        var newVal = token.ToObject<BitfinexEventResult>();
                        bitfinex.subscribe(newVal);
                    }
                }
            };

            bitfinexSocket.OnOpen += (sender, e) =>
            {
                bitfinexSocket.Send("{\"event\":\"subscribe\", \"channel\":\"ticker\",\"pair\":\"XRPBTC\"}");
                bitfinexSocket.Send("{\"event\":\"subscribe\", \"channel\":\"ticker\",\"pair\":\"XLMBTC\"}");
            };

            bitfinexSocket.OnError += (sender, e) =>
            {
                logger.Error(string.Format("BitfinexRequests ERROR|| sender:{0} || e:{1}", sender, e));
            };

            bitfinexSocket.OnClose += (sender, e) =>
            {
                logger.Error(string.Format("BitfinexRequests CLOSED|| sender:{0} || e:{1}", sender, e));
                bitfinex.Deactivate();
                BitfinexRequests(bitfinex,true);
                logger.Trace(string.Format("Bitfinex will dispose"));
                ((IDisposable)bitfinexSocket).Dispose();
                logger.Trace(string.Format("Bitfinex disposed"));
            };


            bitfinexSocket.Connect();
            logger.Trace(string.Format("BitfinexRequests task end"));
        }

        public static async Task BitstampRequests(Bitstamp bitstamp)
        {
            try
            {
                var pusher = new Pusher("de504dc5763aeef9ff52");
                var xrpChannel = pusher.Subscribe("live_trades_xrpbtc");

                pusher.ConnectionStateChanged += (sender, state) =>
                {
                    if(state== ConnectionState.Disconnected)
                    {
                        bitstamp.Deactivate();
                    }else if (state==ConnectionState.Connected)
                    {
                        bitstamp.Activate();
                    }
                };

                pusher.Error += (sender, state) =>
                {
                    logger.Error(string.Format("BitstampRequests || sender:{0} || state:{1}",sender,state));
                };
                
                xrpChannel.Bind("trade", (dynamic data) =>
                {
                    //logger.Debug(string.Format("Bitstamp(XRP) last:{0:0.00000000}", data.price));
                    var xrp = ((JObject)data).ToObject<BitstampCoin>();
                    bitstamp.SetXRP(xrp);
                });
                pusher.Connect();

            }catch(Exception ex)
            {
                logger.Error(string.Format("Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
            }
        }

        public static async Task OkexRequests(Okex okex, int waitime)
        {
            bool delay = false;
            okex.setClient(okexClient);

            while (true)
            {
                try
                {
                    if (delay)
                    {
                        logger.Debug(string.Format("Okex is out. Going to wait for 5 min(stamp:{0})", DateTime.Now));
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        logger.Debug(string.Format("Okex awake from wait delay(stamp:{0})", DateTime.Now));
                        delay = false;
                    }
                    
                    okexClient.DefaultRequestHeaders.Accept.Clear();
                    var XLMstr = await okexClient.GetStringAsync("https://www.okex.com/api/v1/ticker.do?symbol=xlm_btc");
                    var last_tick_xlm = DateTime.Now;

                    var XRPstr = await okexClient.GetStringAsync("https://www.okex.com/api/v1/ticker.do?symbol=xrp_btc");
                    var last_tick_xrp = DateTime.Now;

                    var XLM = JsonConvert.DeserializeObject<OkexCoinRes>(XLMstr);
                    var XRP = JsonConvert.DeserializeObject<OkexCoinRes>(XRPstr);

                    okex.SetXLM(XLM.ticker, last_tick_xlm);
                    okex.SetXRP(XRP.ticker, last_tick_xrp);
                    if (!okex.IsActive()) okex.Activate();

                    Console.WriteLine(string.Format("Okex(LTC) last:{0}", XLM.ticker.last));
                    Console.WriteLine(string.Format("Okex(XRP) last:{0}", XRP.ticker.last));

                    //Thread.Sleep(waitime);
                    await Task.Delay(waitime);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("(Okex) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    okex.Deactivate();
                    delay = true;
                }
            }
        }

        public static async Task BithumbRequests(Bithumb bithumb, int waitime)
        {
            bool delay = false;
            bithumb.setClient(bithumbClient);

            while (true)
            {
                try
                {
                    if (delay)
                    {
                        await Task.Delay(delayTimeApiUnavailable * 1000 * 60);
                        delay = false;
                    }

                    if (!convertorReady)
                    {
                        await Task.Delay(waitime);
                        continue;
                    }

                    bithumbClient.DefaultRequestHeaders.Accept.Clear();

                    var XRPstr = await bithumbClient.GetStringAsync("https://api.bithumb.com/public/ticker/XRP");
                    var last_tick_xrp = DateTime.Now;
                    var XRP = JsonConvert.DeserializeObject<BithumbRes<BithumbCoin>>(XRPstr);
                    
                    bithumb.SetXRP(XRP.data, last_tick_xrp);
                    if (!bithumb.IsActive()) bithumb.Activate();
                    
                    Console.WriteLine(string.Format("Bithumb(XRP) last:{0}", XRP.data.getLastPrice()));
                    
                    await Task.Delay(waitime);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("(BithumbRequests) Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    bithumb.Deactivate();
                    delay = true;
                }
            }
        }


        public static async Task Utils()
        {
            while (true)
            {
                try
                {
                    utilsClient.DefaultRequestHeaders.Accept.Clear();
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var str = await bithumbClient.GetStringAsync("https://www.xe.com/currencyconverter/convert/?Amount=1&From=KRW&To=XBT");
                    string pattern = @">1 KRW = 0.\d{12}";
                    var r = Regex.Match(str, pattern);
                    var res = r.Value.Substring(9, r.Length-9);
                    var finalres = Convert.ToDouble(res, CultureInfo.InvariantCulture);
                    Exchanges.Validations.KRW_to_BTC = finalres;
                    convertorReady = true;
                    stopWatch.Stop();
                    await Task.Delay(30 * 1000);
                }
                catch(Exception ex)
                {
                    logger.Error(string.Format("Error Obtnaining KRW to BTC value ||  Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                }
            }
        }

        public static async Task CompareCoinPairs(IExchange[] exchanges)
        {
            try
            {
                while (true)
                {
                    for (int i = 0; i < exchanges.Length; i++)
                    {
                        if (!exchanges[i].IsActive()) continue;

                        for (int ii = i + 1; ii < exchanges.Length; ii++)
                        {
                            if(exchanges[ii].IsActive())
                                exchanges[i].comparePairs(exchanges[ii]);//e.g Poloniex.compare(bittrex) | Polonoiex.compare(cryptopia) | ...
                        }
                    }
                }
            }catch(Exception ex)
            {
                logger.Error(string.Format("Message:{0} || InnerMessage:{1}",ex.Message, ex.InnerException!=null?ex.InnerException.Message:""));
            }
        }
    }
}