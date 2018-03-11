using BitfinexDataWriter.DataWriter;
using System;

namespace BitfinexDataWriter.Aggregator
{
    public abstract class BaseAggregator : IAggregator
    {
        private readonly IDataWriter _dataWriter;
        private readonly string _instrument;

        private double _bestAsk;
        private double _bestBid;

        protected BaseAggregator(IDataWriter dataWriter, string instrumentName)
        {
            _dataWriter = dataWriter;
            _instrument = instrumentName;
        }

        protected abstract (double Bid, double Ask) GetBestPrices();

        protected void UpdateBestPrices()
        {
            var (bid, ask) = GetBestPrices();
            if (bid != _bestBid || ask != _bestAsk)
            {
                _bestBid = bid;
                _bestAsk = ask;
                _dataWriter.Write(GetResultData());
            }
        }

        private ResultData GetResultData()
        {
            return new ResultData(_instrument, DateTime.UtcNow, _bestBid, _bestAsk);
        }
    }
}
