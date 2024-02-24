using Microsoft.Extensions.Logging;
using System;

namespace UWorx.JiraWorkLogs.RabbitMQ;

public class RabbitMQReceiverService : IServiceMessagingService
{
    readonly MessageReceiver messageReceiver;

    public event EventHandler<ActivityEventArgs> OnMessageReceived;

    public RabbitMQReceiverService(ILogger<RabbitMQReceiverService> logger)
    {
        this.messageReceiver = new MessageReceiver(logger,
            RabbitMQConstants.RabbitMqHost, RabbitMQConstants.RabbitMqUser, RabbitMQConstants.RabbitMqPassword);
    }

    public void StartConsumer()
    {
        this.messageReceiver.OnMessageReceived += (s, e) => this.OnMessageReceived?.Invoke(this, e);
        this.messageReceiver.StartConsumer();
    }
}
