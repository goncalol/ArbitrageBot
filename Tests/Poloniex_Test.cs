using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using TraderBot.Exchanges;
using TraderBot.Exchanges.Orders;
using static TraderBot.Exchanges.Coins.Coin;

namespace Tests
{
    [TestClass]
    public class Poloniex_Test
    {
        Poloniex poloniex;
        HttpClient poloniexClient;
        Binance binance;
        HttpClient binanceClient;

        [TestInitialize]
        public void Init()
        {
            poloniex = new Poloniex();
            poloniexClient = new HttpClient();
            poloniex.setClient(poloniexClient);
            binance = new Binance();
            binanceClient = new HttpClient();
            binance.setClient(binanceClient);
        }

        [TestMethod]
        public void TestOrderBook()
        {
            var ob = poloniex.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);
        }

        [TestMethod]
        public void TestDeposit()
        {
            var ob = poloniex.HasOrderArrived(new OrderDetail { txId = "cenas" }).Result;

            Assert.IsTrue(ob.Success);
            Assert.IsNull(ob.Result);
        }

        [TestMethod]
        public void Test_OrderComplete_That_Not_Exists()
        {
            var obj = new POLONIEX_BUY_ORDER { orderNumber = 123 };
            obj.SetCurrencySymbol("BTC_XRP");
            var ob = poloniex.IsOrderComplete(obj).Result;

            Assert.IsFalse(ob.Success);
        }

        [TestMethod]
        public void Test_Deposit_With_TxId()
        {
            while (true)
            {
                var ob = poloniex.HasOrderArrived(new OrderDetail { amount= 18.85000000, timestamp= 1527610602 }).Result;

                if (ob.Result != null) break;

                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void Test_Balances()
        {           
            var ob = poloniex.GetBalances().Result;

            Assert.IsTrue(ob.Success);
        }

        [TestMethod]
        public void TestSell()
        {
            var ob = poloniex.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);

            var order = poloniex.PostSellOrderFor(CoinName.BTC_XRP, ob.Result.GetBid().First().GetValue(), 14).Result;

            Assert.IsTrue(order.Success);
            Assert.IsNotNull(order.Result);

            while (true)
            {
                var OrderComplete = poloniex.IsOrderComplete(order.Result).Result;
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
            var ob = poloniex.GetOrderBookFor(CoinName.BTC_XRP).Result;

            Assert.IsTrue(ob.Success);

            var order = poloniex.PostBuyOrderFor(CoinName.BTC_XRP, ob.Result.GetAsk().First().GetValue(), 13).Result;

            Assert.IsTrue(order.Success);
            Assert.IsNotNull(order.Result);

            while (true)
            {
                var OrderComplete = poloniex.IsOrderComplete(order.Result).Result;
                Assert.IsTrue(OrderComplete.Success);

                if (OrderComplete.Result)
                {
                    return;
                }
                Thread.Sleep(2000);
            }
        }

        [TestMethod]
        public void TestWithDraw_To_BINANCE()
        {
            var wd = poloniex.WithDraw(CoinName.BTC_XRP, 33.226, binance).Result;
            Assert.IsTrue(wd.Success);

            Stopwatch time = Stopwatch.StartNew();

            while (true)
            {
                var ob = binance.HasOrderArrived(new OrderDetail { txId = wd.Result, currencyName = CoinName.BTC_XRP }).Result;

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
            var depositList = poloniex.GetTxId(1527689452).Result;

            Assert.IsNotNull(depositList);
        }
    }
}
