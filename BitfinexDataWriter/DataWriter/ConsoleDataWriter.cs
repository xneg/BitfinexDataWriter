using System;
using BitfinexDataWriter.Aggregator;

namespace BitfinexDataWriter.DataWriter
{
    public class ConsoleDataWriter : IDataWriter
    {
        private readonly AggregatorType aggregatorType;

        public ConsoleDataWriter(AggregatorType aggregatorType)
        {
            this.aggregatorType = aggregatorType;
        }

        public void Write(ResultData data) => 
            Console.WriteLine($"{aggregatorType} {data.InstrumentName} {data.DateTime:O} Best bid: {data.BestBid} Best ask: {data.BestAsk}");
    }
}
