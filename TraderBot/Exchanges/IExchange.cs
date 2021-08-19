using System;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges
{
    public interface IExchange
    {
        void comparePairs(IExchange exchange);

        ICoin GetXRP();

        ICoin GetXLM();

        ICoin GetSTEEM();

        ICoin GetXEM();

        ICoin GetNEO();

        bool IsActive();

        void Deactivate();

        void Activate();

        void SetXRP(ICoinDetail coin, DateTime tick_time = default(DateTime));

        void SetXLM(ICoinDetail coin, DateTime tick_time = default(DateTime));

        void SetSTEEM(ICoinDetail coin, DateTime tick_time = default(DateTime));

        void SetXEM(ICoinDetail coin, DateTime tick_time = default(DateTime));

        void SetNEO(ICoinDetail coin, DateTime tick_time = default(DateTime));

        string GetName();

        Address GetAddress();

        Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName);

        Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName, double price, double quantity);

        Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity);

        Task<OperationResult<bool>> IsOrderComplete(IOrder order);

        Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinyExchange);

        Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order);
    }
}
