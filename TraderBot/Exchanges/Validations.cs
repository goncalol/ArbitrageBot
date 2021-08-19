using NLog;
using System;
using System.Configuration;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using System.Collections.Generic;
using TraderBot.Exchanges.Orders;

namespace TraderBot.Exchanges
{
    public class Validations
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static DateTime start = DateTime.Now;
        public static double KRW_to_BTC;
        private static double percent;
        private static int max_time;
        private static int min_time;
        private static double BTC_QTY;
        private static bool IsProcessingPair = false;
        public static double averagePrice;
        public static double ammountToWithDraw;

        public static void initValues()
        {
            percent = Double.Parse(ConfigurationManager.AppSettings["percentValue"]);
            BTC_QTY = Double.Parse(ConfigurationManager.AppSettings["btc_qty"]);
            max_time = Int32.Parse(ConfigurationManager.AppSettings["max_time"]);
            min_time = Int32.Parse(ConfigurationManager.AppSettings["min_time"]);
        }

        public static void CompareCoinValues(ICoin currentCoin, ICoin otherCoin)// Exchange A, Exchange B
        {
            if (!currentCoin.hasValue() || !otherCoin.hasValue() || IsProcessingPair) return;

            var currentExchangeAskValue = currentCoin.getValue().getAsk();
            var otherExchangeBidValue = otherCoin.getValue().getBid();
                     
            //!!!!!!!!!!!!!PENSAR MELHOR NESTA LOGICA - PENSO QUE TAMOS A PERDER OPORTUNIDADES!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if(currentExchangeAskValue < otherExchangeBidValue)// comprar na exchange A(curret) <--> vender na exchange B(other)
            {
                var diference = Math.Abs(currentExchangeAskValue - otherExchangeBidValue);
                var val = (diference / currentExchangeAskValue) * 100;
                if (val > percent)
                {
                    IsProcessingPair = true;
                    logger.Info(string.Format("[{6}] Coin:{0} || Diff:{1:F3}% || Buying in {2}({3:F8}), selling in {4}({5:F8})",
                                       currentCoin.getCoinName(), val, currentCoin.getExchangeName(), currentExchangeAskValue,
                                       otherCoin.getExchangeName(), otherExchangeBidValue, DateTime.Now.ToLongTimeString()));
                    OrderBook(currentCoin, otherCoin, null);
                    //var promissedPair = currentCoin.GetPromissedPair(otherCoin);
                    //if (promissedPair != null)
                    //{
                    //    if (promissedPair.IsInPeriod(min_time, max_time, val) && !promissedPair.IsProcessing())
                    //    {
                    //        //logger.Info(string.Format("|| BEGINING [{8}]  {0:F3}%  ||  END [{9}]  {10:F3}%  ||({7})  |||  {1}(value:{2:F8} | stamp:{3}) > {4}(value:{5:F8} | stamp:{6})  |||", promissedPair.firstDiference,
                    //        //                currentCoin.getExchangeName(), currentExchangeAskValue, currentCoin.getTimestamp().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    //        //                otherCoin.getExchangeName(), otherExchangeBidValue, otherCoin.getTimestamp().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    //        //                otherCoin.getCoinName(), promissedPair.firstTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    //        //                DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture), val));

                    //        logger.Info(string.Format("[{6}] Coin:{0} || Diff:{1:F3}% || Buying in {2}({3:F8}), selling in {4}({5:F8})",
                    //                    currentCoin.getCoinName(), val, currentCoin.getExchangeName(), currentExchangeAskValue,
                    //                    otherCoin.getExchangeName(), otherExchangeBidValue, DateTime.Now.ToLongTimeString()));
                    //        promissedPair.ResetTime(val);
                    //        promissedPair.SetProcessing(true);
                    //        OrderBook(currentCoin, otherCoin, promissedPair);
                    //    }
                    //}
                    //else
                    //{
                    //    logger.Debug(string.Format("create promise to Coin({0}) {1}-{2}", currentCoin.getCoinName(), currentCoin.getExchangeName(), otherCoin.getExchangeName()));
                    //    currentCoin.CreatePromissedPair(otherCoin, val);
                    //}
                }
            }
            else
            {
                var currentExchangeBidValue = currentCoin.getValue().getBid();
                var otherExchangeAskValue = otherCoin.getValue().getAsk();

                if(otherExchangeAskValue < currentExchangeBidValue)// comprar na exchange B(other) <--> vender na exchange A(current)
                {
                    var diference = Math.Abs(otherExchangeAskValue - currentExchangeBidValue);
                    var val = (diference / otherExchangeAskValue) * 100;
                    if (val > percent)
                    {
                        IsProcessingPair = true;
                        logger.Info(string.Format("[{6}]  Coin:{0} || Diff:{1:F3}% || Buying in {2}({3:F8}), selling in {4}({5:F8})",
                                        currentCoin.getCoinName(), val, otherCoin.getExchangeName(), otherExchangeBidValue,
                                        currentCoin.getExchangeName(), currentExchangeAskValue, DateTime.Now.ToLongTimeString()));
                        OrderBook(otherCoin, currentCoin, null);
                        //var promissedPair = currentCoin.GetPromissedPair(otherCoin);
                        //if (promissedPair != null)
                        //{
                        //    if (promissedPair.IsInPeriod(min_time, max_time, val) && !promissedPair.IsProcessing())
                        //    {
                        //        //logger.Info(string.Format("|| BEGINING [{8}]  {0:F3}%  ||  END [{9}]  {10:F3}%  ||({7})  |||  {1}(value:{2:F8} | stamp:{3}) > {4}(value:{5:F8} | stamp:{6})  |||", promissedPair.firstDiference,
                        //        //                currentCoin.getExchangeName(), currentExchangeAskValue, currentCoin.getTimestamp().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                        //        //                otherCoin.getExchangeName(), otherExchangeBidValue, otherCoin.getTimestamp().ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                        //        //                otherCoin.getCoinName(), promissedPair.firstTime.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
                        //        //                DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture), val));

                        //        logger.Info(string.Format("[{6}]  Coin:{0} || Diff:{1:F3}% || Buying in {2}({3:F8}), selling in {4}({5:F8})",
                        //                currentCoin.getCoinName(), val, otherCoin.getExchangeName(), otherExchangeBidValue,
                        //                currentCoin.getExchangeName(), currentExchangeAskValue, DateTime.Now.ToLongTimeString()));
                        //        promissedPair.ResetTime(val);
                        //        promissedPair.SetProcessing(true);
                        //        //OrderBook(otherExchangeBidValue, currentExchangeBidValue, promissedPair);
                        //    }
                        //}
                        //else
                        //{
                        //    logger.Debug(string.Format("create promise to Coin({0}) {1}-{2}", currentCoin.getCoinName(), currentCoin.getExchangeName(), otherCoin.getExchangeName()));
                        //    currentCoin.CreatePromissedPair(otherCoin, val);
                        //}
                    }
                }
            }
        }

        private static async Task OrderBook(ICoin CointToBuy, ICoin CoinToSell, PromisedPair promissedPair)
        {
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();
            try
            {
                logger.Debug(string.Format("[{0}] Getting order book for {1}", DateTime.Now.ToLongTimeString(), CointToBuy.getExchangeName()));
                var sellPrices = await CointToBuy.GetOrderBook();
                if (!sellPrices.Success)
                {
                    //promissedPair.SetProcessing(false);
                    logger.Error(sellPrices.Message);
                    Environment.Exit(0);
                    return;
                }

                logger.Debug(string.Format("[{0}] Order book Success", DateTime.Now.ToLongTimeString()));
                var sellPricesArr = sellPrices.Result.GetAsk();
                var coinToSellQty = Math.Floor((BTC_QTY / CointToBuy.getValue().getAsk()) * 100) / 100; // quantidade final(arredondada 2 casas para baixo) para ser comprada
                logger.Debug(string.Format("[{0}] coinToSellQty: {1}", DateTime.Now.ToLongTimeString(), coinToSellQty));

                var valuesToBuyAux = new List<PairAux>();
                var QtyAcumulator = 0d;
                var askPriceToBuy = 0d;//preço final para ser comprado
                for (int i = 0; ; i++)
                {
                    var PairQty = sellPricesArr[i].GetQuantity();
                    QtyAcumulator += PairQty;
                    askPriceToBuy = sellPricesArr[i].GetValue();

                    if (QtyAcumulator >= coinToSellQty)
                    {
                        if (i == 0) valuesToBuyAux.Add(new PairAux { price = askPriceToBuy, quantity = coinToSellQty });
                        else valuesToBuyAux.Add(new PairAux { price = askPriceToBuy, quantity = Math.Abs((QtyAcumulator - PairQty) - coinToSellQty) });
                        break;
                    }
                    valuesToBuyAux.Add(new PairAux { price = askPriceToBuy, quantity = PairQty });
                }
                logger.Debug(string.Format("[{0}] final price to buy: {1}", DateTime.Now.ToLongTimeString(), askPriceToBuy));

                //calcular a media ponderada para ver se o valor ainda faz sentido comprar
                var sum = 0d;
                foreach (var p in valuesToBuyAux)
                {
                    sum += p.price * p.quantity;
                }
                var average = sum / coinToSellQty;
                if (average >= CoinToSell.getValue().getBid())
                {
                    logger.Warn(string.Format("Buying {0} from {1} has average({2}) >= {3} bid value({4})", CointToBuy.getCoinName(),
                                        CointToBuy.getExchangeName(), average, CoinToSell.getExchangeName(), CoinToSell.getValue().getBid()));
                    //promissedPair.SetProcessing(false);
                    Environment.Exit(0);
                    return;
                }


                logger.Info(string.Format("[{4}] Buying {0} from {1} at price({2:F8}) with quantity({3})", CointToBuy.getCoinName(),
                                        CointToBuy.getExchangeName(), askPriceToBuy, coinToSellQty, DateTime.Now.ToLongTimeString()));


                //Processo de COMPRA
                logger.Debug(string.Format("[{0}] Starting buying process", DateTime.Now.ToLongTimeString()));
                var buyOrder = await CointToBuy.PostBuyOrder(askPriceToBuy, coinToSellQty);
                logger.Debug(string.Format("[{0}] End post request", DateTime.Now.ToLongTimeString()));

                if (!buyOrder.Success)
                {
                    //promissedPair.SetProcessing(false);
                    logger.Error(buyOrder.Message);
                    Environment.Exit(0);
                    return;
                }

                logger.Debug(string.Format("[{0}] Set average price of the coin that was bought:{1}", DateTime.Now.ToLongTimeString(), average));
                //promissedPair.SetAverageBuyPrice(average);
                averagePrice = average;
                CheckBuyOrderCompletion(buyOrder.Result, CointToBuy, CoinToSell, null);// promissedPair);
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Exception in OrderBook() || Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                Environment.Exit(0);
            }
            //stopWatch.Stop();            
        }

        private static async Task CheckBuyOrderCompletion(IOrder order, ICoin CointToBuy, ICoin CoinToSell, PromisedPair promissedPair)
        {

            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Check if Order was completed", DateTime.Now.ToLongTimeString()));
                    var OrderComplete = await CointToBuy.IsOrderComplete(order);
                    logger.Debug(string.Format("[{0}] End of order completion request", DateTime.Now.ToLongTimeString()));
                    if (!OrderComplete.Success)
                    {
                       // promissedPair.SetProcessing(false);
                        logger.Error(OrderComplete.Message);
                        Environment.Exit(0);
                        // return;
                    }

                    if (OrderComplete.Result)
                    {
                        await SendCoinToExchange(order.GetTotalAmount(), CointToBuy, CoinToSell, null);// promissedPair);
                        return;
                    }

                    logger.Debug(string.Format("[{0}] Order is still not completed. waiting 2s to check again...", DateTime.Now.ToLongTimeString()));
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("CheckBuyOrderCompletion() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    Environment.Exit(0);
                }
            }
        }

        private static async Task SendCoinToExchange(double amount, ICoin CointToBuy, ICoin coinToSell, PromisedPair promissedPair)
        {
            logger.Debug(string.Format("[{0}] Send coin with amount({1}) to other exchange", DateTime.Now.ToLongTimeString(),amount));
            ammountToWithDraw = amount;
            var withdraw = await CointToBuy.WithDraw(amount, coinToSell);
            logger.Debug(string.Format("[{0}] End of withdraw request", DateTime.Now.ToLongTimeString()));

            if (!withdraw.Success)
            {
                //promissedPair.SetProcessing(false);
                logger.Error(withdraw.Message);
                Environment.Exit(0);
                // return;
            }

            ExchangeToSellListen(withdraw.Result, CointToBuy, coinToSell, null);// promissedPair);
        }

        private static async Task ExchangeToSellListen(string txId, ICoin cointToBuy, ICoin coinToSell, PromisedPair promissedPair)
        {
            var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            //logger.Trace("Current timestamp: "+timestamp);

            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Check if order of {1} is on {2}", DateTime.Now.ToLongTimeString(),cointToBuy.getExchangeName(),coinToSell.getExchangeName()));
                    var HasOrderArrived = await coinToSell.HasOrderArrived(new OrderDetail { txId= txId, currencyName=cointToBuy.getCoin(), timestamp=timestamp, amount=ammountToWithDraw});
                    logger.Debug(string.Format("[{0}] End of Check if order arrived request", DateTime.Now.ToLongTimeString()));

                    if (!HasOrderArrived.Success)//O QUE DEVERIA DE ACONTECER NESTE CASO !!!!!!!!!!!!!!!!
                    {
                        //promissedPair.SetProcessing(false);//NAO PODE DEIXAR CONTINUAR A COMPRAR MOEDAS ATE VENDER O RETRANSFERIR O ACTUAL
                        logger.Error(HasOrderArrived.Message);
                        Environment.Exit(0);
                        //await Task.Delay(1000 * 60);//1 min
                        //continue;
                    }

                    if (HasOrderArrived.Result != null)
                    {
                        logger.Debug(string.Format("[{0}] Order has arrived to {1}", DateTime.Now.ToLongTimeString(),coinToSell.getExchangeName()));

                        OrderBookToSell(HasOrderArrived.Result.GetAmount(), cointToBuy, coinToSell, null);// promissedPair);
                        return;
                    }

                    logger.Debug(string.Format("[{0}] Order still hasn't arrived to {1}. waiting 1min to check again...", DateTime.Now.ToLongTimeString(), coinToSell.getExchangeName()));
                    await Task.Delay(1000*60);//1 min
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("ExchangeToSellListen() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                    Environment.Exit(0);
                }
            }
        }

        private static async Task OrderBookToSell(double amountToSell, ICoin cointToBuy, ICoin coinToSell, PromisedPair promissedPair)
        {
            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Checking orderbook for {1}", DateTime.Now.ToLongTimeString(), coinToSell.getExchangeName()));
                    var buyPrices = await coinToSell.GetOrderBook();
                    
                    if (!buyPrices.Success)//!!!!!!!!!!!!!!!!!!!!!!! O QUE FAZER NESTE ESTADO!!!!!!!!!!!!!!!!!!!!!
                    {
                        //promissedPair.SetProcessing(false);
                        logger.Error(buyPrices.Message);
                        continue;
                    }

                    logger.Debug(string.Format("[{0}] Order book Success", DateTime.Now.ToLongTimeString()));

                    var buyPricesArr = buyPrices.Result.GetBid();
                    logger.Debug(string.Format("[{0}] coinToSellQty: {1}", DateTime.Now.ToLongTimeString(), amountToSell));

                    var valuesToSellAux = new List<PairAux>();
                    var QtyAcumulator = 0d;
                    var bidPriceToSell = 0d;//preço final para ser vendido
                    for (int i = 0; ; i++)
                    {
                        var PairQty = buyPricesArr[i].GetQuantity();
                        QtyAcumulator += PairQty;
                        bidPriceToSell = buyPricesArr[i].GetValue();

                        if (QtyAcumulator >= amountToSell)
                        {
                            if (i == 0) valuesToSellAux.Add(new PairAux { price = bidPriceToSell, quantity = amountToSell });
                            else valuesToSellAux.Add(new PairAux { price = bidPriceToSell, quantity = Math.Abs((QtyAcumulator - PairQty) - amountToSell) });
                            break;
                        }
                        valuesToSellAux.Add(new PairAux { price = bidPriceToSell, quantity = PairQty });
                    }
                    logger.Debug(string.Format("[{0}] final price to sell: {1}", DateTime.Now.ToLongTimeString(), bidPriceToSell));

                    //calcular a media ponderada para ver se o valor ainda faz sentido comprar
                    var sum = 0d;
                    foreach (var p in valuesToSellAux)
                    {
                        sum += p.price * p.quantity;
                    }
                    var averagePriceToSell = sum / amountToSell;
                    var averagePriceToBuy = averagePrice;//promissedPair.GetAverageBuyValue();
                    logger.Debug(string.Format("[{0}] Average price to sell({1}) - Average price to buy({2})", DateTime.Now.ToLongTimeString(), averagePriceToSell, averagePriceToBuy));

                    if (averagePriceToBuy < averagePriceToSell)
                    {
                        var diference = Math.Abs(averagePriceToSell - averagePriceToBuy);
                        var val = (diference / averagePriceToBuy) * 100;
                        logger.Debug(string.Format("[{0}] Diference value: {1}%", DateTime.Now.ToLongTimeString(), val));

                        if (val >= percent)
                        {
                            logger.Debug(string.Format("[{0}] Start posting sell order price({1}) amount({2})", DateTime.Now.ToLongTimeString(), bidPriceToSell, amountToSell));
                            var sellOrder = await coinToSell.PostSellOrder(bidPriceToSell, amountToSell);

                            if (!sellOrder.Success)
                            {
                                //promissedPair.SetProcessing(false);
                                logger.Error(sellOrder.Message);
                                //return;
                            }
                            else
                            {
                                logger.Debug(string.Format("[{0}] Sell order posted", DateTime.Now.ToLongTimeString()));
                                CheckSellOrderCompletion(sellOrder.Result, cointToBuy, coinToSell, promissedPair,(bidPriceToSell / KRW_to_BTC)* amountToSell);
                                return;
                            }
                        }
                    }                                  
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("OrderBookToSell() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                }

                await Task.Delay(2000);
            }
        }

        private static async Task CheckSellOrderCompletion(IOrder order, ICoin cointToBuy, ICoin coinToSell, PromisedPair promissedPair, double totalKRWAmountToSell)
        {
            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Listening for sell order completion...", DateTime.Now.ToLongTimeString()));
                    var OrderComplete = await coinToSell.IsOrderComplete(order);
                    if (!OrderComplete.Success)
                    {
                       // promissedPair.SetProcessing(false);
                        logger.Error(OrderComplete.Message);
                       // return;
                    }
                    else
                    {
                        
                        if (OrderComplete.Result)
                        {
                            logger.Debug(string.Format("[{0}] Sell order completed with success. amount:{1}", DateTime.Now.ToLongTimeString(), order.GetTotalAmount()));

                            //await SendCoinBackToExchange(order.GetTotalAmount(), cointToBuy, coinToSell, promissedPair);
                            //Processo de COMPRA
                            //logger.Debug(string.Format("[{0}] Starting buying BTC FROM KRW process", DateTime.Now.ToLongTimeString()));
                            //var totalKRWAmountToSell = order.GetTotalAmount();
                            //var rr = await Get_BTC_Price_To_Buy(coinToSell, promissedPair, totalKRWAmountToSell);
                            //if (rr != null) {
                            //    var buyOrder = await coinToSell.PostBuyOrder(rr.price, rr.quantity);
                            //    logger.Debug(string.Format("[{0}] End post request", DateTime.Now.ToLongTimeString()));

                            //    if (buyOrder.Success)
                            //    {
                            //        //!!!!!!!!!!!!ESPERAR E DEPOIS ENVIAR....!!!!!!!!!!
                            //        //await SendCoinBackToExchange(buyOrder.GetTotalAmount(), cointToBuy, coinToSell, promissedPair);
                            //        CheckBuyOrderCompletionV2(buyOrder.Result, coinToSell, cointToBuy, promissedPair, rr.quantity);
                            //        return;
                            //    }
                            //    else
                            //    {
                            //        logger.Error(buyOrder.Message);
                            //    }
                            //}

                            Environment.Exit(0);
                        }
                        logger.Debug(string.Format("[{0}] End of Request orderComplete(). Still not sold. waiting 2s...", DateTime.Now.ToLongTimeString()));

                    }

                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("CheckSellOrderCompletion() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                }

                await Task.Delay(2000);
            }
        }

        private static async Task SendCoinBackToExchange(double amount, ICoin cointToBuy, ICoin coinToSell, PromisedPair promissedPair)
        {
            logger.Debug(string.Format("[{0}] Start sending coin back to {1}", DateTime.Now.ToLongTimeString(), cointToBuy.getExchangeName()));
            var withdraw = await coinToSell.WithDraw(amount, cointToBuy);
            if (!withdraw.Success)
            {
                promissedPair.SetProcessing(false);
                logger.Error(withdraw.Message);
                return;
            }
            logger.Debug(string.Format("[{0}] Coin sent with Success", DateTime.Now.ToLongTimeString()));

            logger.Info(withdraw.Result);

            ExchangeToListenProfit(cointToBuy, coinToSell, promissedPair);
        }

        private static async Task ExchangeToListenProfit(ICoin cointToBuy, ICoin coinToSell, PromisedPair promissedPair)
        {
            var timestamp = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Check if coin has arrived", DateTime.Now.ToLongTimeString()));
                    var HasOrderArrived = await cointToBuy.HasOrderArrived(new OrderDetail {timestamp=timestamp });
                    logger.Debug(string.Format("[{0}] Request ended", DateTime.Now.ToLongTimeString()));

                    if (!HasOrderArrived.Success)//O QUE DEVERIA DE ACONTECER NESTE CASO !!!!!!!!!!!!!!!!
                    {
                        //promissedPair.SetProcessing(false);//NAO PODE DEIXAR CONTINUAR A COMPRAR MOEDAS ATE VENDER O RETRANSFERIR O ACTUAL
                        logger.Error(HasOrderArrived.Message);
                        await Task.Delay(1000 * 60);//1 min
                        continue;
                    }

                    if (HasOrderArrived.Result != null)
                    {
                        //OrderBookToSell(HasOrderArrived.Result.GetAmount(), cointToBuy, coinToSell, promissedPair);
                        logger.Info("Process executed with SUCCESS!!!!!!!!!!");
                        promissedPair.SetProcessing(false);
                        return;
                    }

                    logger.Debug(string.Format("[{0}] Coin still hasn't arrived. wating 1 min...", DateTime.Now.ToLongTimeString()));
                    await Task.Delay(1000 * 60);//1 min
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("ExchangeToListenProfit() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                }
            }
        }

        private static async Task<PairAux> Get_BTC_Price_To_Buy(ICoin CointToBuy, PromisedPair promissedPair, double totalKRWAmountToSell)
        {
            logger.Debug(string.Format("[{0}] Getting order book for {1}", DateTime.Now.ToLongTimeString(), CointToBuy.getExchangeName()));
            var sellPrices = await CointToBuy.Get_BTC_OrderBook();
            if (!sellPrices.Success)
            {
               // promissedPair.SetProcessing(false);
                logger.Error(sellPrices.Message);
                return null;
            }

            logger.Debug(string.Format("[{0}] Order book Success", DateTime.Now.ToLongTimeString()));
            var sellPricesArr = sellPrices.Result.GetAsk();
           // var coinToSellQty = Math.Floor((BTC_QTY / sellPricesArr.First().GetValue()) * 100) / 100; // quantidade final(arredondada 2 casas para baixo) para ser comprada
            logger.Debug(string.Format("[{0}] coinToBuyQty: {1}", DateTime.Now.ToLongTimeString(), totalKRWAmountToSell));

            var BTC_Qty = 0d;
            var LastKRWPrice = 0d;//preço final de krw para comprar
            var KRW_Acumulator = 0d;
            var BTC_Qty_Final = 0d;//final quantity of btc
            var prevAcumulator = 0d;

            for (int i = 0; ; i++)
            {
                BTC_Qty = sellPricesArr[i].GetQuantity();
                BTC_Qty_Final += BTC_Qty;
                LastKRWPrice = sellPricesArr[i].GetValue() / KRW_to_BTC;
                
                KRW_Acumulator += BTC_Qty * LastKRWPrice;

                if (KRW_Acumulator== totalKRWAmountToSell)
                {
                    break;
                }
                else if (KRW_Acumulator > totalKRWAmountToSell)
                {
                    //remover o btc acumulado
                    BTC_Qty_Final -= BTC_Qty;

                    //calcular o resto
                    var resto = Math.Round((totalKRWAmountToSell - prevAcumulator)*BTC_Qty / (KRW_Acumulator),4);
                    BTC_Qty_Final += resto;
                    break;
                }
                prevAcumulator = KRW_Acumulator;
            }

            logger.Info(string.Format("[{4}] Buying BTC from {1} at price({2:F8}) with quantity({3})", CointToBuy.getCoinName(),
                                    CointToBuy.getExchangeName(), LastKRWPrice, BTC_Qty_Final, DateTime.Now.ToLongTimeString()));

            return new PairAux { price= LastKRWPrice, quantity= BTC_Qty_Final };
        }

        private static async Task CheckBuyOrderCompletionV2(IOrder order, ICoin CointToBuy, ICoin CoinToSell, PromisedPair promissedPair,double totalKRWAmountToSell)
        {

            while (true)
            {
                try
                {
                    logger.Debug(string.Format("[{0}] Check if Order was completed", DateTime.Now.ToLongTimeString()));
                    var OrderComplete = await CointToBuy.IsOrderComplete(order);
                    logger.Debug(string.Format("[{0}] End of order completion request", DateTime.Now.ToLongTimeString()));
                    if (!OrderComplete.Success)
                    {
                        // promissedPair.SetProcessing(false);
                        logger.Error(OrderComplete.Message);
                        // return;
                    }

                    if (OrderComplete.Result)
                    {
                        await SendCoinToExchange(totalKRWAmountToSell, CointToBuy, CoinToSell, promissedPair);
                        return;
                    }

                    logger.Debug(string.Format("[{0}] Order is still not completed. waiting 2s to check again...", DateTime.Now.ToLongTimeString()));
                    await Task.Delay(2000);
                }
                catch (Exception ex)
                {
                    logger.Error(string.Format("CheckBuyOrderCompletion() error Message:{0} || InnerMessage:{1}", ex.Message, ex.InnerException != null ? ex.InnerException.Message : ""));
                }
            }
        }

    }

    public class PairAux
    {
        public double price { get; set; }

        public double quantity { get; set; }
    }
   
}
