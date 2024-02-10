using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Utils;

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            this.messageReceiver.StartConsumer();
            await Task.CompletedTask.ConfigureAwait(false);

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    var activityName = $"Tick";
            //    using var activity = Program.JiraActivitySource.StartActivity(activityName);

            //    if (logger.IsEnabled(LogLevel.Information))
            //        this.logger.LogInformation("Incrementing greeting at: {time}", DateTimeOffset.Now);

            //    Program.CountGreetings.Add(1);

            //    activity?.SetTag("greeting", Program.CountGreetings);

            //    await Task.Delay(3000, stoppingToken);
            //}
        }
    }
}
