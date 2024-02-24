using System;

namespace UWorx.JiraWorkLogs.Redis;

class RedisConstants
{
    public static string RedisConnectionString
    {
        get
        {
            var redisHostName = Environment.GetEnvironmentVariable("REDIS_HOSTNAME") ?? "redis";
            return $"{redisHostName}:6379,abortConnect=false";
        }
    }
}
