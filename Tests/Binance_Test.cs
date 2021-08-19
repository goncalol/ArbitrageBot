using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TraderBot.Exchanges;
using TraderBot.Exchanges.Orders;
using static TraderBot.Exchanges.Coins.Coin;

namespace Tests
{
    [TestClass]
    public class Binance_Test
    {
        Bittrex bittrex;
        HttpClient bittrexClient;
        Binance binance;
        HttpClient binanceClient;
        Poloniex poloniex;
        HttpClient poloniexClient;

        [TestInitialize]
        public void Init()
        {
            poloniex = new Poloniex();
            poloniexClient = new HttpClient();
            poloniex.setClient(poloniexClient);
            bittrex = new Bittrex();
            bittrexClient = new HttpClient();
            bittrex.setClient(bittrexClient);
            binance = new Binance();
            binanceClient = new HttpClient();
            binance.setClient(binanceClient);
        }

        [TestMethod]
        public void TestTicker()
        {
            binanceClient.DefaultRequestHeaders.Accept.Clear();
            var XRPstr = binanceClient.GetStringAsync("https://api.binance.com/api/v3/ticker/bookTicker?symbol=XRPBTC").Result;
            
            var XRP = JsonConvert.DeserializeObject<BINANCE_COIN_DETAIL>(XRPstr);

            binance.SetXRP(XRP);
        }

        [TestMethod]
        public void TestOrderBook()
        {
            var ob = binance.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);
        }

        [TestMethod]
        public void TestDeposit()
        {
            var ob = binance.HasOrderArrived(new OrderDetail { txId="cenas"}).Result;

            Assert.IsTrue(ob.Success);
            Assert.IsNull(ob.Result);
        }

        [TestMethod]
        public void Test_OrderComplete_That_Not_Exists()
        {
            var ob = binance.IsOrderComplete(new BINANCE_ORDER { symbol="XRPBTC", orderId=123  }).Result;

            Assert.IsFalse(ob.Success);
        }

        [TestMethod]
        public void TestSell()
        {
            var ob = binance.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);

            var order = binance.PostSellOrderFor(CoinName.BTC_XRP, ob.Result.GetBid().First().GetValue(),14).Result;

            Assert.IsTrue(order.Success);
            Assert.IsNotNull(order.Result);

            while (true)
            {
                var OrderComplete = binance.IsOrderComplete(order.Result).Result;
                Assert.IsTrue(OrderComplete.Success);

                if (OrderComplete.Result)
                {
                    return;
                }
                Thread.Sleep(2000);
            }
        }

        [TestMethod]
        public void TestBuy()
        {
            var ob = binance.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);

            var order = binance.PostBuyOrderFor(CoinName.BTC_XRP, ob.Result.GetAsk().First().GetValue(), 13).Result;

            Assert.IsTrue(order.Success);
            Assert.IsNotNull(order.Result);

            while (true)
            {
                var OrderComplete = binance.IsOrderComplete(order.Result).Result;
                Assert.IsTrue(OrderComplete.Success);

                if (OrderComplete.Result)
                {
                    return;
                }
                Thread.Sleep(2000);
            }
        }

        [TestMethod]
        public void TestWithDraw_To_BITTREX()
        {
            var wd = binance.WithDraw(CoinName.BTC_XRP, 23.237, bittrex).Result;
            Assert.IsTrue(wd.Success);

            Stopwatch time = Stopwatch.StartNew();

            while (true)
            {
                var ob = bittrex.HasOrderArrived(new OrderDetail { txId = wd.Result, currencyName=CoinName.BTC_XRP }).Result;

                Assert.IsTrue(ob.Success);
                if (ob.Result != null)
                {
                    time.Stop();
                    var spent = time.ElapsedMilliseconds;
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void TestWithDraw_To_POLONIEX()
        {
            var wd = binance.WithDraw(CoinName.BTC_XRP, 35.952, poloniex).Result;
            Assert.IsTrue(wd.Success);

            Stopwatch time = Stopwatch.StartNew();

            while (true)
            {
                var ob = poloniex.HasOrderArrived(new OrderDetail { txId = wd.Result, currencyName = CoinName.BTC_XRP }).Result;

                Assert.IsTrue(ob.Success);
                if (ob.Result != null)
                {
                    time.Stop();
                    var spent = time.ElapsedMilliseconds;
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void Test_Deposit_With_UID()
        {
            var depositList = binance.GetTxId("XRP", "Redacted").Result;

            //Assert.IsTrue(depositList.success);
        }
    }
}
