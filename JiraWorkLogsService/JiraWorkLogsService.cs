using Dapplo.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using UWorx.JiraWorkLogs;
using UWorx.JiraWorkLogs.RabbitMQ;

namespace JiraWorkLogsService;

public class JiraWorkLogsService
{
    internal static ActivitySource JiraActivitySource;
    internal static Counter<int> CountGreetings;

    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        JiraActivitySource = new ActivitySource(builder.Environment.ApplicationName);
        var greeterMeter = new Meter(builder.Environment.ApplicationName + ".Greeter");
        CountGreetings = greeterMeter.CreateCounter<int>("greetings.count", description: "Counts the number of greetings");

        var telemetryBuilder = builder.Services.AddOpenTelemetry();
        telemetryBuilder.ConfigureResource(b => b.AddService(builder.Environment.ApplicationName));
        telemetryBuilder.WithMetrics(metrics => metrics
            .AddMeter(greeterMeter.Name));
        telemetryBuilder.WithTracing(tracing => tracing
            .AddHttpClientInstrumentation()
            .AddSource(JiraActivitySource.Name)
            .AddZipkinExporter(b => b.Endpoint = JiraWorkLogConstants.ZipkinEndpoint)
            .AddConsoleExporter());
        // for otlp
        //tracing.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint));

        builder.Services.AddSingleton<IServiceMessagingService>(f =>
        {
            var logger = f.GetRequiredService<ILogger<RabbitMQReceiverService>>();
            return new RabbitMQReceiverService(logger);
        });

        var host = builder.Build();
        host.Run();
    }
}