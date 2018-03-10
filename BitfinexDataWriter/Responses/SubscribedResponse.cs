using Newtonsoft.Json;

namespace BitfinexDataWriter.Responses
{
    public class SubscribedResponse
    {
        public string Event { get; set; }
        public string Channel { get; set; }
        [JsonProperty("chanid")]
        public int ChannelId { get; set; }
        public string Pair { get; set; }
        public string Symbol { get; set; }
        public string Key { get; set; }
    }
}
