using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TraderBot.Exchanges;
using System.Net.Http;
using static TraderBot.Exchanges.Coins.Coin;
using TraderBot.Exchanges.Orders;
using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class Bittrex_Test
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
            bittrex = new Bittrex();
            bittrexClient = new HttpClient();
            bittrex.setClient(bittrexClient);
            binance = new Binance();
            binanceClient = new HttpClient();
            binance.setClient(binanceClient);
            poloniex = new Poloniex();
            poloniexClient = new HttpClient();
            poloniex.setClient(poloniexClient);
        }

        [TestMethod]
        public void TestOrderBook()
        {
            //ja foi testado muitas vezes com o bithumb!!!!!!!!!!!!!!!!!
        }

        [TestMethod]
        public void TestWithDraw_To_BINANCE()
        {
            var wd = bittrex.WithDraw(CoinName.BTC_XRP, 22.987, binance).Result;
            Assert.IsTrue(wd.Success);

            Stopwatch time = Stopwatch.StartNew();

            while (true)
            {
                var ob = binance.HasOrderArrived(new OrderDetail { txId = wd.Result }).Result;

                Assert.IsTrue(ob.Success);
                if(ob.Result != null)
                {
                    time.Stop();
                    var spent = time.ElapsedTicks;
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void TestBuy()
        {
            var XRPstr = bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-XRP").Result;
            
            var XRP = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(XRPstr);

            var order = bittrex.PostBuyOrderFor(CoinName.BTC_XRP,XRP.result.Ask, 15).Result;

            Assert.IsTrue(order.Success);

            while (true)
            {
                var OrderComplete = bittrex.IsOrderComplete(order.Result).Result;
                Assert.IsTrue(OrderComplete.Success);

                if (OrderComplete.Result)
                {
                    return;
                }
                Thread.Sleep(2000);               
            }
        }

        [TestMethod]
        public void TestSell()
        {
            var XRPstr = bittrexClient.GetStringAsync("https://bittrex.com/api/v1.1/public/getticker?market=BTC-XRP").Result;

            var XRP = JsonConvert.DeserializeObject<BittrexCoinRes<BittrexResult>>(XRPstr);

            var order = bittrex.PostSellOrderFor(CoinName.BTC_XRP, XRP.result.Bid, 10).Result;

            Assert.IsTrue(order.Success);

            while (true)
            {
                var OrderComplete = bittrex.IsOrderComplete(order.Result).Result;
                Assert.IsTrue(OrderComplete.Success);

                if (OrderComplete.Result)
                {
                    return;
                }
                Thread.Sleep(2000);
            }
        }

        [TestMethod]
        public void Test_Order_Arrived_To_Exchange()
        {
            var cenas =  bittrex.HasOrderArrived(new OrderDetail { txId= "Redacted" }).Result;

            Assert.IsTrue(cenas.Success);
            Assert.AreEqual("Redacted", cenas.Result.GetTxId());
        }

        [TestMethod]
        public void Test_Order_NOT_Arrived_To_Exchange()
        {
            var cenas = bittrex.HasOrderArrived(new OrderDetail { txId = "merda" }).Result;

            Assert.IsTrue(cenas.Success);
            Assert.IsNull(cenas.Result);

        }

        [TestMethod]
        public void TestWithDraw_To_POLONIEX()
        {
            var wd = bittrex.WithDraw(CoinName.BTC_XRP, 20, poloniex).Result;
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
            var depositList = bittrex.GetTxId("XRP", "Redacted").Result;

            //Assert.IsTrue(depositList.success);
        }
    }
}
