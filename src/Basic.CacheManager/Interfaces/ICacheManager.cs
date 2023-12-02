namespace Basic.CacheManager.Interfaces;

public interface ICacheManager
{
    T Get<T>(string key);

    object Get(string key);

    object Get(string key, Type type);

    void Add(string key, object data, int seconds, Type type);

    void Add(string key, object data, int seconds);

    void Add(string key, object data, Type type);

    void Add(string key, object data);

    bool IsAdd(string key);

    void Remove(string key);

    void RemoveByPattern(string pattern);
}