using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Utils;
using Utils.Messaging;
using OpenTelemetry.Trace;
using System;

namespace JiraWorkLogsService;
public class Program
{
    public static void Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        CreateHostBuilder(args).Build().Run();
        //var builder = Host.CreateApplicationBuilder(args);
        //builder.Services.AddHostedService<Worker>();

        //var host = builder.Build();
        //host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Worker>();

                services.AddSingleton<MessageReceiver>();

                services.AddOpenTelemetry()
                    .WithTracing(builder => builder
                        .AddSource(nameof(MessageReceiver))
                        .AddZipkinExporter(b =>
                        {
                            b.Endpoint = Constants.ZipkinEndpoint;
                        }));
            });
}