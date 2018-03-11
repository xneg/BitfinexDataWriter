using BitfinexDataWriter.Responses;
using Newtonsoft.Json.Linq;

namespace BitfinexDataWriter.Aggregator
{
    /// <summary>
    /// Интерфейс хранилища агрегаторов.
    /// </summary>
    public interface IAggregatorsStorage
    {
        void OnChannelCreated(SubscribedResponse subscribedResponse);

        void OnDataReceive(JToken data);

        ///// <summary>
        ///// Создать агрегатор и добавить его в хранилище.
        ///// </summary>
        ///// <param name="aggregatorType">Тип агрегатора.</param>
        ///// <param name="channelId">ID канала.</param>
        ///// <param name="instrumentName">Имя инструмента.</param>
        //void CreateAggregator(AggregatorType aggregatorType, int channelId, string instrumentName);

        ///// <summary>
        ///// Получить агрегатор по ID канала.
        ///// </summary>
        ///// <param name="channelId">ID канала.</param>
        ///// <returns>Экземпляр агрегатора.</returns>
        //IAggregator GetAggregator(int channelId);
    }
}
