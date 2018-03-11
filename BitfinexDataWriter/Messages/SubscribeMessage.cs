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

        [JsonProperty("prec")]
        public string Precision { get; set; }

        [JsonIgnore]
        public string Serialized => JsonConvert.SerializeObject(this);

        public static SubscribeMessage SubscribeToBookMessage(string symbol)
        {
            return new SubscribeMessage { Event = "subscribe", Channel = "book", Symbol = symbol };
        }

        public static SubscribeMessage SubscribeToRawBookMessage(string symbol)
        {
            return new SubscribeMessage
            {
                Event = "subscribe",
                Channel = "book",
                Symbol = symbol,
                Precision = "R0",
            };
        }
    }
}
