using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using JiraWorkLogsService.Helpers;

namespace JiraWorkLogsService
{
    public class Worker : BackgroundService
    {
        private readonly string jql = "Project = AppM AND worklogDate >= '2024-1-1' AND worklogDate < '2024-2-1'";
        private readonly string[] emails = new[] {
            "abdul.hai@juriba.com", "abdulraffay.saeed@juriba.com", "arslan.ahmad@juriba.com", "azeem.khan@juriba.com", "hamza.mehmood@juriba.com",
            "khurram.aziz@juriba.com", "mohammed.butt@juriba.com", "samia.saleem@juriba.com", "sana.fatehkhan@juriba.com", "tahreem.ahmad@juriba.com"
        };

        private readonly ILogger<Worker> logger;
        private readonly MessageReceiver messageReceiver;

        public Worker(ILogger<Worker> logger,
            MessageReceiver messageReceiver)
        {
            this.logger = logger;
            this.messageReceiver = messageReceiver;
            this.messageReceiver.OnMessageReceived += MessageReceived;
        }

        private void MessageReceived(object? sender, ActivityEventArgs e)
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
                    var j = new JiraHelper(Constants.JiraUrl, Constants.JiraUser, Constants.JiraToken);
                    j.ListIssuesAsync(jql).Wait();
                    e.MessageActivity?.AddEvent(new ActivityEvent("Jira Queuried"));

                    try
                    {
                        var summarizer = new Summarizer();
                        int r = summarizer.ProcessAsync(emails).Result;
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
