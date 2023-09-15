using Basic.CacheManager.Enums;

namespace Basic.CacheManager.Options
{
    internal class RedisClientOption
    {
        public string Environment { get; set; }

        public string Host { get; set; }

        public string Password { get; set; }

        public int Port { get; set; }

        public long Db { get; set; }

        public RedisClientType RedisClientType { get; set; }
    }
}
