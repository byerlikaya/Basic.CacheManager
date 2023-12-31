﻿namespace Basic.CacheManager.Options;

internal class RedisClientOption
{
    public string Environment { get; set; }

    public string Host { get; set; }

    public string Password { get; set; }

    public int Port { get; set; }

    public long Db { get; set; }

    public Enums.RedisClientType RedisClientType { get; set; }
}