namespace Basic.CacheManager.Caching.Redis;

public class RedisCacheManager : ICacheManager
{
    private readonly PooledRedisClientManager _pooledRedisClientManager;

    public RedisCacheManager(IConfiguration configuration)
    {
        var redisClients = configuration?.GetSection(nameof(RedisClientOption)).Get<List<RedisClientOption>>();

        if (redisClients != null && redisClients.Any())
        {
            var readWriteHosts = new List<string>();

            var readOnlyHosts = new List<string>();

            foreach (var redisClientInfo in redisClients!)
            {
                switch (redisClientInfo.RedisClientType)
                {
                    case Enums.RedisClientType.Master:
                    {
                        readWriteHosts.Add($"redis://{redisClientInfo.Host}:{redisClientInfo.Port}?db={redisClientInfo.Db}&password={redisClientInfo.Password.UrlEncode()}");
                        break;
                    }
                    case Enums.RedisClientType.Slave:
                    {
                        readOnlyHosts.Add($"redis://{redisClientInfo.Host}:{redisClientInfo.Port}?db={redisClientInfo.Db}&password={redisClientInfo.Password.UrlEncode()}");
                        break;
                    }
                    case Enums.RedisClientType.Sentinel:
                    {
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _pooledRedisClientManager = new PooledRedisClientManager(readWriteHosts, readOnlyHosts);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public T Get<T>(string key)
    {
        using var client = _pooledRedisClientManager.GetReadOnlyClient();
        return client.Get<T>(key);
    }

    public object Get(string key)
    {
        using var client = _pooledRedisClientManager.GetReadOnlyClient();
        return client.Get<object>(key);
    }

    public object Get(string key, Type type) =>
        typeof(Task)
            .GetMethod(nameof(Task.FromResult))?
            .MakeGenericMethod(type)
            .Invoke(this, new[]
            {
                JsonSerializer.DeserializeFromString(Get<string>(key), type)
            });

    public void Add(string key, object data, int seconds)
    {
        using var client = _pooledRedisClientManager.GetClient();
        RemoveByPattern(key);
        client.Add(key, data, TimeSpan.FromSeconds(seconds));
    }

    public void Add(string key, object data)
    {
        using var client = _pooledRedisClientManager.GetClient();
        RemoveByPattern(key);
        client.Add(key, data);
    }

    public void Add(string key, dynamic data, int seconds, Type type) => Add(key, JsonSerializer.SerializeToString(data.Result, type), seconds);

    public void Add(string key, dynamic data, Type type) => Add(key, JsonSerializer.SerializeToString(data.Result, type));

    public bool IsAdd(string key)
    {
        using var client = _pooledRedisClientManager.GetReadOnlyClient();
        return client.ContainsKey(key);
    }

    public void Remove(string key)
    {
        using var client = _pooledRedisClientManager.GetClient();
        client.Remove($"*:{key}");
    }

    public void RemoveByPattern(string pattern)
    {
        using var client = _pooledRedisClientManager.GetClient();
        client.RemoveByPattern($"*:{pattern}");
    }
}