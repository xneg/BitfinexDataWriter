using BitfinexDataWriter.DataWriter;
using System;
using System.Collections.Generic;

namespace BitfinexDataWriter.Aggregator
{
    public class AggregatorsStorage : IAggregatorsStorage
    {
        private readonly Dictionary<int, IAggregator> _aggregators = new Dictionary<int, IAggregator>();
        private readonly DataWriterType _dataWriterType;

        public AggregatorsStorage(DataWriterType dataWriterType)
        {
            _dataWriterType = dataWriterType;
        }

        public void CreateAggregator(int channelId, string instrumentName)
        {
            _aggregators.Add(channelId, GetAggregator(channelId, instrumentName));
        }

        public IAggregator GetAggregator(int channelId)
        {
            return _aggregators[channelId];
        }

        private IAggregator GetAggregator(int channelId, string instrumentName)
        {
            switch (_dataWriterType)
            {
                case DataWriterType.BinaryFile:
                    return new BookAggregator(new FileDataWriter(instrumentName), channelId, instrumentName);
                case DataWriterType.Console:
                    return new BookAggregator(new ConsoleDataWriter(), channelId, instrumentName);
                default:
                    throw new ArgumentException(nameof(_dataWriterType));
            }
        }
    }
}
