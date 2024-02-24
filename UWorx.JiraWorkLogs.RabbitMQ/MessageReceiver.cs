// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UWorx.JiraWorkLogs.RabbitMQ;

class MessageReceiver : IDisposable
{
    private static readonly ActivitySource ActivitySource = new(nameof(MessageReceiver));
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly ILogger logger;
    private readonly IConnection connection;
    private readonly IModel channel;

    public event EventHandler<ActivityEventArgs> OnMessageReceived;

    public MessageReceiver(ILogger logger, string host, string user, string password)
    {
        this.logger = logger;
        this.connection = RabbitMQHelper.CreateConnection(host, user, password);
        this.channel = RabbitMQHelper.CreateModelAndDeclareTestQueue(this.connection);
    }

    public void Dispose()
    {
        this.channel.Dispose();
        this.connection.Dispose();
    }

    public void StartConsumer()
    {
        RabbitMQHelper.StartConsumer(this.channel, this.ReceiveMessage);
    }

    public void ReceiveMessage(BasicDeliverEventArgs ea)
    {
        // Extract the PropagationContext of the upstream parent from the message headers.
        var parentContext = Propagator.Extract(default, ea.BasicProperties, this.ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/messaging/messaging-spans.md#span-name
        var activityName = $"{ea.RoutingKey} receive";

        using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.Span.ToArray());

            this.logger.LogInformation($"Message received: [{message}]");

            activity?.SetTag("message", message);

            // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
            RabbitMQHelper.AddMessagingTags(activity);

            if (null != this.OnMessageReceived)
                this.OnMessageReceived(this, new ActivityEventArgs { MessageActivity = activity }); ;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Message processing failed.");
        }
    }

    private IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties props, string key)
    {
        try
        {
            if (props.Headers.TryGetValue(key, out var value))
            {
                var bytes = value as byte[];
                return new[] { Encoding.UTF8.GetString(bytes) };
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to extract trace context.");
        }

        return Enumerable.Empty<string>();
    }
}
