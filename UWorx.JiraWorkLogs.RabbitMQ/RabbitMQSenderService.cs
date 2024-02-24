using Microsoft.Extensions.Logging;

namespace UWorx.JiraWorkLogs.RabbitMQ;

public class RabbitMQSenderService : IWebAppMessagingService
{
    readonly ILogger<RabbitMQSenderService> logger;

    public RabbitMQSenderService(ILogger<RabbitMQSenderService> logger)
    {
        this.logger = logger;
    }

    public void TriggerJiraSync()
    {
        var ms = new MessageSender(this.logger,
            RabbitMQConstants.RabbitMqHost, RabbitMQConstants.RabbitMqUser, RabbitMQConstants.RabbitMqPassword);
        ms.SendMessage();
    }
}
