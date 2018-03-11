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
    }
}
