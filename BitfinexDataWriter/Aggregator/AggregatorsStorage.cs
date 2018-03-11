using System;
using System.Collections.Generic;
using BitfinexDataWriter.DataWriter;
using BitfinexDataWriter.Responses;
using Newtonsoft.Json.Linq;

namespace BitfinexDataWriter.Aggregator
{
    /// <summary>
    /// Реализация хранилища агрегаторов.
    /// </summary>
    public class AggregatorsStorage : IAggregatorsStorage
    {
        private readonly Dictionary<int, IAggregator> _aggregators = new Dictionary<int, IAggregator>();
        private readonly DataWriterType _dataWriterType;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="dataWriterType">Тип провайдера записи данных.</param>
        public AggregatorsStorage(DataWriterType dataWriterType)
        {
            _dataWriterType = dataWriterType;
        }

        public void OnChannelCreated(SubscribedResponse subscribedResponse)
        {
            if (subscribedResponse.Event == "subscribed")
            {
                var aggregatorType = (subscribedResponse.Precision == "R0") ? AggregatorType.RawBooks : AggregatorType.Books;
                var channeldId = subscribedResponse.ChannelId;
                _aggregators.Add(channeldId, CreateAggregator(aggregatorType, channeldId, subscribedResponse.Pair));
            }
        }

        public void OnDataReceive(JToken data)
        {
            OnBook(data);
        }

        private IAggregator GetAggregator(int channelId)
        {
            return _aggregators[channelId];
        }

        private void OnBook(JToken token)
        {
            var channelId = (int)token[0];

            var data = token[1];

            if (data.Type != JTokenType.Array)
            {
                return; // heartbeat, ignore
            }

            var aggregator = GetAggregator(channelId);

            switch (aggregator)
            {
                case BookAggregator bookAggregator:
                    AddDataToAggregator<Book, BookAggregator>(data, channelId, bookAggregator);
                    break;
                case RawBookAggregator rawBookAggregator:
                    AddDataToAggregator<RawBook, RawBookAggregator>(data, channelId, rawBookAggregator);
                    break;
            }
        }
        
        private void AddDataToAggregator<TData, TAggregator> (JToken token, int channelId, TAggregator aggregator) 
            where TAggregator : IAggregator<TData>
        {
            if (token.First.Type == JTokenType.Array)
            {
                var books = token.ToObject<TData[]>();
                aggregator.GetSnapshot(books);
            }
            else
            {
                var book = token.ToObject<TData>();
                aggregator.GetBook(book);
            }
        }

        private IAggregator CreateAggregator(AggregatorType aggregatorType, int channelId, string instrumentName)
        {
            IDataWriter dataWriter;
            switch (_dataWriterType)
            {
                case DataWriterType.BinaryFile:
                    dataWriter = new FileDataWriter(instrumentName);
                    break;
                case DataWriterType.Console:
                    dataWriter = new ConsoleDataWriter();
                    break;
                default:
                    throw new ArgumentException(nameof(_dataWriterType));
            }

            switch (aggregatorType)
            {
                case AggregatorType.Books:
                    return new BookAggregator(dataWriter, channelId, instrumentName);
                case AggregatorType.RawBooks:
                    return new RawBookAggregator(dataWriter, channelId, instrumentName);
                default:
                    throw new ArgumentException(nameof(aggregatorType));
            }
        }

    }
}
