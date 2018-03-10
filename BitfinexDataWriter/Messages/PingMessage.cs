using Newtonsoft.Json;

namespace BitfinexDataWriter.Messages
{
    public class PingMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("cid")]
        public int CId { get; set; }

        [JsonIgnore]
        public string Serialized => JsonConvert.SerializeObject(this);

        public static PingMessage CreateMessage(int cId)
        {
            return new PingMessage { Event = "ping", CId = cId };
        }
    }
}
