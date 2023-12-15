using Newtonsoft.Json;

namespace MemIt
{
    public class MemContext
    {
        private const string contextFileName = "context.json";
        public long LastEpoch { get; set; }

        public HashSet<long> ChatIds { get; set; }

        public MemContext()
        {
            
        }

        public void Restore()
        {
            if (File.Exists(contextFileName))
            {
                var last = File.ReadAllText(contextFileName);
                var deserialized = JsonConvert.DeserializeObject<MemContext>(last);
                LastEpoch = deserialized.LastEpoch;
                ChatIds = deserialized.ChatIds;
                return;
            }

            var now = DateTime.UtcNow;
            var dayAgo = now.AddHours(-24);
            LastEpoch = new DateTimeOffset(dayAgo).ToUnixTimeSeconds();
            ChatIds = new HashSet<long>();
        }

        public void Save()
        {
            File.WriteAllText(contextFileName, JsonConvert.SerializeObject(this));
        }
    }
}
