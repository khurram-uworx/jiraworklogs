using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace UWorx.JiraWorkLogs.RabbitMQ;

public class RabbitMQSenderService : IWebAppMessagingService
{
    readonly ILogger<RabbitMQSenderService> logger;
    readonly IConnection connection;

    public RabbitMQSenderService(ILogger<RabbitMQSenderService> logger,
        IConnection connection)
    {
        this.logger = logger;
        this.connection = connection;
    }

    public void TriggerJiraSync()
    {
        //var connection = RabbitMQHelper.CreateConnection(host, user, password);
        var ms = new MessageSender(this.logger, this.connection);
            //RabbitMQConstants.RabbitMqHost, RabbitMQConstants.RabbitMqUser, RabbitMQConstants.RabbitMqPassword);
        ms.SendMessage();
    }
}
