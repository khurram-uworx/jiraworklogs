using System;

namespace Utils
{
    public static class Constants
    {
        public static Uri ZipkinEndpoint
        {
            get
            {
                var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "zipkin";
                return new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
            }
        }

        public static string DatabaseConnectionString
        {
            get
            {
                var postgresHostName = Environment.GetEnvironmentVariable("POSTGRES_HOSTNAME") ?? "postgres";
                return $"Server={postgresHostName};Database=postgres;UserName=postgres;Password=password;";
            }
        }

        public static string RedisConnectionString
        {
            get
            {
                var redisHostName = Environment.GetEnvironmentVariable("REDIS_HOSTNAME") ?? "redis";
                return $"{redisHostName}:6379,abortConnect=false";
            }
        }

        internal static string RabbitMqHost { get => "rabbitmq"; }

        internal static string RabbitMqUser { get => "guest"; }

        internal static string RabbitMqPassword { get => "guest"; }
    }
}
