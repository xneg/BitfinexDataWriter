using Newtonsoft.Json;

namespace BitfinexDataWriter.Messages
{
    public class SubscribeMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonIgnore]
        public string Serialized => JsonConvert.SerializeObject(this);

        public static SubscribeMessage CreateMessage(string channel, string symbol)
        {
            return new SubscribeMessage { Event = "subscribe", Channel = channel, Symbol = symbol };
        }
    }
}
