using System;
using System.Collections.Generic;
using System.Text;
using BitfinexDataWriter.Aggregator;

namespace BitfinexDataWriter.DataWriter
{
    public class ConsoleDataWriter : IDataWriter
    {
        public void Write(ResultData data) => 
            Console.WriteLine($"{data.InstrumentName} {data.DateTime:O} Best bid: {data.BestBid} Best ask: {data.BestAsk}");
    }
}
