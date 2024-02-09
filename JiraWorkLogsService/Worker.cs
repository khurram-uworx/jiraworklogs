namespace JiraWorkLogsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                var activityName = $"Tick";
                using var activity = Program.JiraActivitySource.StartActivity(activityName);

                await Task.Delay(random.Next(2000));

                if (logger.IsEnabled(LogLevel.Information))
                    this.logger.LogInformation("Incrementing greeting at: {time}", DateTimeOffset.Now);

                Program.CountGreetings.Add(1);

                activity?.SetTag("greeting", Program.CountGreetings);

                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
