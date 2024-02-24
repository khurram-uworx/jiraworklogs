using Microsoft.Extensions.Logging;

namespace UWorx.JiraWorkLogs.RabbitMQ;

public class RabbitMQSenderService : IWebAppMessagingService
{
    readonly ILogger<RabbitMQSenderService> logger;
    readonly string host, user, password;

    public RabbitMQSenderService(ILogger<RabbitMQSenderService> logger, string host, string user, string password)
    {
        this.logger = logger;
        this.host = host;
        this.user = user;
        this.password = password;
    }

    public void TriggerJiraSync()
    {
        var ms = new MessageSender(this.logger, this.host, this.user, this.password);
        ms.SendMessage();
    }
}
