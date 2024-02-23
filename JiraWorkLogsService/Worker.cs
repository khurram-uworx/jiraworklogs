using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using JiraWorkLogsService.Helpers;
using UWorx.JiraWorkLogs.Redis;

namespace JiraWorkLogsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly MessageReceiver messageReceiver;

        public Worker(ILogger<Worker> logger,
            MessageReceiver messageReceiver)
        {
            this.logger = logger;
            this.messageReceiver = messageReceiver;
            this.messageReceiver.OnMessageReceived += messageReceived;
        }

        void messageReceived(object? sender, ActivityEventArgs e)
        {
            if (logger.IsEnabled(LogLevel.Information))
                this.logger.LogInformation("Incrementing greeting at: {time}", DateTimeOffset.Now);

            Program.CountGreetings.Add(1);
            e.MessageActivity?.SetTag("greeting", Program.CountGreetings);

            var jiraUrl = Constants.JiraUrl;
            if (!string.IsNullOrWhiteSpace(jiraUrl) && jiraUrl != "https://YOUR-COMPANY.atlassian.net")
            {
                try
                {
                    var jql = ServiceConstants.Jql;
                    if (string.IsNullOrEmpty(jql)) ArgumentException.ThrowIfNullOrEmpty("jql");
                    
                    var j = new JiraHelper(Constants.JiraUrl, Constants.JiraUser, Constants.JiraToken);
                    j.ListIssuesAsync(jql).Wait();
                    e.MessageActivity?.AddEvent(new ActivityEvent("Jira Queuried"));

                    try
                    {
                        var summarizer = new Summarizer(new RedisWebAppDataStore(Constants.RedisConnectionString));
                        int r = summarizer.ProcessAsync(ServiceConstants.Emails).Result;
                        e.MessageActivity?.AddEvent(new ActivityEvent("Cache updated"));
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Failed to update cache");
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to query Jira {url}", jiraUrl);
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            this.messageReceiver.StartConsumer();
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
