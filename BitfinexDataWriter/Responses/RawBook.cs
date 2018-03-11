using Newtonsoft.Json;

namespace BitfinexDataWriter.Responses
{
    [JsonConverter(typeof(RawBookConverter))]
    public class RawBook : BaseResponse
    {
        public int OrderId { get; set; }

        public double Price { get; set; }

        public double Amount { get; set; }
    }
}
