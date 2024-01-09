using System;

namespace Utils
{
    public static class Constants
    {
        public static Uri ZipkinEndpoint
        {
            get
            {
                //var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                var zipkinHostName = "zipkin";
                return new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
            }
        }

        public static string RedisConnectionString
        {
            get
            {
                return "redis:6379,abortConnect=false";
            }
        }

        internal static string RabbitMqHost { get => "rabbitmq"; }

        internal static string RabbitMqUser { get => "guest"; }

        internal static string RabbitMqPassword { get => "guest"; }

        internal static string DatabaseConnectionString
        {
            get
            {
                return "Server=postgres;UserId=postgres;Password=password;Database=jiraworklogs;";
            }
        }

        internal static string JiraUrl { get => Environment.GetEnvironmentVariable("JIRA_URL"); }

        internal static string JiraUser { get => Environment.GetEnvironmentVariable("JIRA_USER"); }

        internal static string JiraToken { get => Environment.GetEnvironmentVariable("JIRA_TOKEN"); }
    }
}
