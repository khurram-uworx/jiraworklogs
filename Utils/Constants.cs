﻿using System;

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
                return $"Server={postgresHostName};Database=postgres;UserName=postgres;Password=uworx;";
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

        public static string JiraUrl { get => Environment.GetEnvironmentVariable("JIRA_URL") ?? ""; }

        public static string JiraUser { get => Environment.GetEnvironmentVariable("JIRA_USER") ?? ""; }

        public static string JiraToken { get => Environment.GetEnvironmentVariable("JIRA_TOKEN") ?? ""; }
    }
}
