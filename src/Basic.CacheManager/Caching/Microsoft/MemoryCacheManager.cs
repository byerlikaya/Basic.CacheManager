namespace Basic.CacheManager.Caching.Microsoft;

public class MemoryCacheManager : ICacheManager
{
    private readonly IMemoryCache _memoryCache;
    public MemoryCacheManager(IMemoryCache memoryCache) => _memoryCache = memoryCache;

    public void Add(string key, object data, int duration) => _memoryCache.Set(key, data, TimeSpan.FromMinutes(duration));

    public void Add(string key, object data) => _memoryCache.Set(key, data);

    public void Add(string key, dynamic data, int duration, Type type) => Add(key, JsonSerializer.SerializeToString(data.Result, type), duration);

    public void Add(string key, dynamic data, Type type) => Add(key, JsonSerializer.SerializeToString(data.Result, type));

    public T Get<T>(string key) => _memoryCache.Get<T>(key);

    public object Get(string key) => _memoryCache.Get(key);

    public object Get(string key, Type type) =>
        typeof(Task)
            .GetMethod(nameof(Task.FromResult))?
            .MakeGenericMethod(type)
            .Invoke(this, new[]
            {
                JsonSerializer.DeserializeFromString(Get<string>(key), type)
            });

    public bool IsAdd(string key) => _memoryCache.TryGetValue(key, out _);

    public void Remove(string key) => _memoryCache.Remove(key);

    public void RemoveByPattern(string pattern)
    {
        var coherentState = typeof(MemoryCache)
            .GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);

        var coherentStateValue = coherentState?.GetValue(_memoryCache);

        var cacheEntriesCollectionDefinition = coherentStateValue?
            .GetType()
            .GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);


        var cacheEntriesCollection = cacheEntriesCollectionDefinition?
            .GetValue(coherentStateValue) as ICollection;

        var cacheCollectionValues = new List<string>();

        if (cacheEntriesCollection != null)
        {
            foreach (var item in cacheEntriesCollection)
            {
                var methodInfo = item.GetType().GetProperty("Key");

                var val = methodInfo?.GetValue(item);

                cacheCollectionValues.Add(val?.ToString());
            }
        }

        var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var keysToRemove = cacheCollectionValues
            .Where(d => regex.IsMatch(d))
            .Select(d => d)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
        }
    }
}