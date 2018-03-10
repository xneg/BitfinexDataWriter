using System;

namespace BitfinexDataWriter.Aggregator
{
    public struct ResultData
    {
        public string InstrumentName { get; }

        public DateTime DateTime { get; }

        public double BestBid { get; }

        public double BestAsk { get; }

        public ResultData(string instrumentName, DateTime dateTime, double bestBid, double bestAsk) 
            : this()
        {
            InstrumentName = instrumentName;
            DateTime = dateTime;
            BestBid = bestBid;
            BestAsk = bestAsk;
        }
    }
}
