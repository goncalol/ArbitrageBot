using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges.Coins
{
    public interface ICoin
    {
        bool hasValue();

        ICoinDetail getValue();

        void setValue(ICoinDetail value, DateTime tick_time = default(DateTime));

        DateTime getTimestamp();

        string getCoinName();

        CoinName getCoin();

        string getExchangeName();

        IExchange GetExchange();

        void CreatePromissedPair(ICoin otherCoin, double firstDiference);

        PromisedPair GetPromissedPair(ICoin otherCoin);

        Task<OperationResult<IOrderBook>> GetOrderBook();

        Task<OperationResult<IOrder>> PostBuyOrder(double price, double quantity);

        Task<OperationResult<IOrder>> PostSellOrder(double price, double quantity);

        Task<OperationResult<bool>> IsOrderComplete(IOrder order);

        Address GetAddress();

        Task<OperationResult<string>> WithDraw(double amount, ICoin destinationCoin);

        Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order);

        Task<OperationResult<IOrderBook>> Get_BTC_OrderBook();
    }

    public interface ICoinDetail
    {
        double getLastPrice();

        double getAsk();

        double getBid();
    }
}
