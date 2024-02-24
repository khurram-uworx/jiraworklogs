using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using JiraWorkLogsService.Helpers;
using UWorx.JiraWorkLogs.Redis;
using UWorx.JiraWorkLogs;

namespace JiraWorkLogsService
{
    public class Worker : BackgroundService
    {
        readonly ILogger<Worker> logger;
        readonly IServiceMessagingService messageReceiver;

        public Worker(ILogger<Worker> logger,
            IServiceMessagingService messageReceiver)
        {
            this.logger = logger;
            this.messageReceiver = messageReceiver;
            this.messageReceiver.OnMessageReceived += messageReceived;
        }

        void messageReceived(object? sender, ActivityEventArgs e)
        {
            if (logger.IsEnabled(LogLevel.Information))
                this.logger.LogInformation("Incrementing greeting at: {time}", DateTimeOffset.Now);

            JiraWorkLogsService.CountGreetings.Add(1);
            e.MessageActivity?.SetTag("greeting", JiraWorkLogsService.CountGreetings);

            var jiraUrl = ServiceConstants.JiraUrl;
            if (!string.IsNullOrWhiteSpace(jiraUrl) && jiraUrl != "https://YOUR-COMPANY.atlassian.net")
            {
                try
                {
                    var jql = ServiceConstants.Jql;
                    if (string.IsNullOrEmpty(jql)) ArgumentException.ThrowIfNullOrEmpty("jql");
                    
                    var j = new JiraHelper(ServiceConstants.JiraUrl, ServiceConstants.JiraUser, ServiceConstants.JiraToken);
                    j.ListIssuesAsync(jql).Wait();
                    e.MessageActivity?.AddEvent(new ActivityEvent("Jira Queuried"));

                    try
                    {
                        var summarizer = new Summarizer(new RedisRepository(this.logger, JiraWorkLogConstants.RedisConnectionString));
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
