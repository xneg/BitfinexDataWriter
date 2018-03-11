using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitfinexDataWriter.Responses
{
    public class RawBookConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RawBook);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            return JArrayToTradingTicker(array);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private RawBook JArrayToTradingTicker(JArray array)
        {
            return new RawBook
            {
                OrderId = (int)array[0],
                Price = (double)array[1],
                Amount = (double)array[2],
            };
        }
    }
}
