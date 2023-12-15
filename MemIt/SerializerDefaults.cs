using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace MemIt
{
    public static class SerializerDefaults
    {
        public static string Serialize<TValue>(TValue value)
        {
            return JsonConvert.SerializeObject(value, JsonSerializerSettings);
        }

        public static TValue Deserialize<TValue>(string json)
        {
            return JsonConvert.DeserializeObject<TValue>(json, JsonSerializerSettings);
        }

        public static JsonSerializerSettings JsonSerializerSettings => ConfigureJsonSerializerSettings(new JsonSerializerSettings());
        public static JsonSerializerSettings ConfigureJsonSerializerSettings(JsonSerializerSettings options)
        {
            options.Formatting = Newtonsoft.Json.Formatting.Indented;
            options.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false,
                    OverrideSpecifiedNames = true
                }
            };
            options.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            return options;
        }

    }
}
