using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace BitfinexDataWriter
{
    public static class BitfinexJsonSerializer
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            Converters = new List<JsonConverter>() { new StringEnumConverter() { CamelCaseText = true } },
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static JsonSerializer Serializer => JsonSerializer.Create(Settings);

        public static T Deserialize<T>(string msg)
        {
            return JsonConvert.DeserializeObject<T>(msg, BitfinexJsonSerializer.Settings);
        }
    }
}
