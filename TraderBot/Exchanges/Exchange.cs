using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TraderBot.Exchanges.Coins;
using TraderBot.Exchanges.OrderBook;
using TraderBot.Exchanges.Orders;
using TraderBot.Utils;
using static TraderBot.Exchanges.Coins.Coin;

namespace TraderBot.Exchanges
{
    public abstract class Exchange : IExchange
    {
        protected ICoin BTC_XRP;

        protected ICoin BTC_XLM;

        protected ICoin BTC_STEEM;

        protected ICoin BTC_XEM;

        protected ICoin BTC_NEO;

        protected bool isActive;

        public abstract void comparePairs(IExchange exchange);

        protected void compareXLM(IExchange exchange)
        {
            var exchangeXLM = exchange.GetXLM();
            //se a outra exchange tiver ltc e se ambas as moedas já obtiveram valor(já obteve resposta da API)
            //entao compara os pares de moedas...
            if (exchangeXLM != null && exchangeXLM.hasValue() && BTC_XLM.hasValue())
            {
                Validations.CompareCoinValues(BTC_XLM, exchangeXLM);
            }
        }

        protected void compareXRP(IExchange exchange)
        {
            var exchangeXRP = exchange.GetXRP();

            if (exchangeXRP != null && exchangeXRP.hasValue() && BTC_XRP.hasValue())
            {
                Validations.CompareCoinValues(BTC_XRP, exchangeXRP);
            }
        }

        protected void compareSTEEM(IExchange exchange)
        {
            var exchangeSTEEM = exchange.GetSTEEM();

            if (exchangeSTEEM != null && exchangeSTEEM.hasValue() && BTC_STEEM.hasValue())
            {
                Validations.CompareCoinValues(BTC_STEEM, exchangeSTEEM);
            }
        }

        protected void compareXEM(IExchange exchange)
        {
            var exchangeXEM = exchange.GetXEM();

            if (exchangeXEM != null && exchangeXEM.hasValue() && BTC_XEM.hasValue())
            {
                Validations.CompareCoinValues(BTC_XEM, exchangeXEM);
            }
        }

        protected void compareNEO(IExchange exchange)
        {
            var exchangeNeo = exchange.GetNEO();

            if (exchangeNeo != null && exchangeNeo.hasValue() && BTC_NEO.hasValue())
            {
                Validations.CompareCoinValues(BTC_NEO, exchangeNeo);
            }
        }

        public ICoin GetXLM()
        {
            return BTC_XLM;
        }

        public ICoin GetXRP()
        {
            return BTC_XRP;
        }

        public ICoin GetSTEEM()
        {
            return BTC_STEEM;
        }

        public ICoin GetXEM()
        {
            return BTC_XEM;
        }

        public ICoin GetNEO()
        {
            return BTC_NEO;
        }

        public void SetXLM(ICoinDetail coin, DateTime tick_time = default(DateTime))
        {
            if(BTC_XLM!=null)
                BTC_XLM.setValue(coin, tick_time);
        }

        public void SetXRP(ICoinDetail coin, DateTime tick_time = default(DateTime))
        {
            if (BTC_XRP != null)
                BTC_XRP.setValue(coin, tick_time);
        }

        public void SetSTEEM(ICoinDetail coin, DateTime tick_time = default(DateTime))
        {
            if (BTC_STEEM != null)
                BTC_STEEM.setValue(coin, tick_time);
        }

        public void SetXEM(ICoinDetail coin, DateTime tick_time = default(DateTime))
        {
            if (BTC_XEM != null)
                BTC_XEM.setValue(coin, tick_time);
        }

        public void SetNEO(ICoinDetail coin, DateTime tick_time = default(DateTime))
        {
            if (BTC_NEO != null)
                BTC_NEO.setValue(coin, tick_time);
        }

        public abstract string GetName();

        public abstract Address GetAddress();

        public void Activate()
        {
            isActive = true ;
        }

        public void Deactivate()
        {
            isActive = false;
        }

        public bool IsActive()
        {
            return isActive;
        }

        public long GetNonce()
        {
            long nEpochTicks = 0;
            long nUnixTimeStamp = 0;
            long nNowTicks = 0;
            long nowMiliseconds = 0;
            string sNonce = "";
            DateTime DateTimeNow;


            nEpochTicks = new DateTime(1970, 1, 1).Ticks;
            DateTimeNow = DateTime.UtcNow;
            nNowTicks = DateTimeNow.Ticks;
            nowMiliseconds = DateTimeNow.Millisecond;

            nUnixTimeStamp = ((nNowTicks - nEpochTicks) / TimeSpan.TicksPerSecond);

            sNonce = nUnixTimeStamp.ToString() + nowMiliseconds.ToString("D03");

            return (Convert.ToInt64(sNonce));
        }

        public string Hash_HMAC_512(string sKey, string sData)
        {
            byte[] rgbyKey = Encoding.UTF8.GetBytes(sKey);


            using (var hmacsha512 = new HMACSHA512(rgbyKey))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(sData));

                return (ByteToString(hmacsha512.Hash));
            }
        }

        public string Hash_HMAC_256(string sKey, string sData)
        {
            byte[] rgbyKey = Encoding.UTF8.GetBytes(sKey);


            using (var hmacsha512 = new HMACSHA256(rgbyKey))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(sData));

                return (ByteToString(hmacsha512.Hash));
            }
        }

        public string ByteToString(byte[] rgbyBuff)
        {
            string sHexStr = "";


            for (int nCnt = 0; nCnt < rgbyBuff.Length; nCnt++)
            {
                sHexStr += rgbyBuff[nCnt].ToString("x2"); // Hex format
            }

            return (sHexStr);
        }

        public byte[] StringToByte(string sStr)
        {
            byte[] rgbyBuff = Encoding.UTF8.GetBytes(sStr);

            return (rgbyBuff);
        }

        public abstract Task<OperationResult<IOrderBook>> GetOrderBookFor(CoinName coinName);

        public abstract Task<OperationResult<IOrder>> PostBuyOrderFor(CoinName coinName,double price, double quantity);

        public abstract Task<OperationResult<IOrder>> PostSellOrderFor(CoinName coinName, double price, double quantity);

        public abstract Task<OperationResult<bool>> IsOrderComplete(IOrder order);

        public abstract Task<OperationResult<string>> WithDraw(CoinName coinName, double amount, IExchange destinyExchange);

        public abstract Task<OperationResult<IDeposit>> HasOrderArrived(OrderDetail order);
    }
}
