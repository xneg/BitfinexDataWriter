using System;
using System.Collections.Generic;
using System.Text;

namespace BitfinexDataWriter.Aggregator
{
    public interface IAggregatorsStorage
    {
        void CreateAggregator(int channelId, string instrumentName);

        IAggregator GetAggregator(int channelId);
    }
}
