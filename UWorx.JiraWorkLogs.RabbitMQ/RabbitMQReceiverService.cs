using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace UWorx.JiraWorkLogs.RabbitMQ;

public class RabbitMQReceiverService : IServiceMessagingService
{
    readonly MessageReceiver messageReceiver;

    public event EventHandler<ActivityEventArgs> OnMessageReceived;

    public RabbitMQReceiverService(ILogger<RabbitMQReceiverService> logger,
        IConnection connection)
    {
        //RabbitMQHelper.CreateConnection(host, user, password);
        this.messageReceiver = new MessageReceiver(logger, connection);
    }

    public void StartConsumer()
    {
        this.messageReceiver.OnMessageReceived += (s, e) => this.OnMessageReceived?.Invoke(this, e);
        this.messageReceiver.StartConsumer();
    }
}
