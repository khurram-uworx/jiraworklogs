using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Utils;

namespace JiraWorkLogsService
{
    public class Program
    {
        internal static ActivitySource JiraActivitySource;
        internal static Counter<int> CountGreetings;

        public static void Main(string[] args)
        {
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
                .AddZipkinExporter(b => b.Endpoint = Constants.ZipkinEndpoint)
                .AddConsoleExporter());
            // for otlp
            //tracing.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint));

            var host = builder.Build();
            host.Run();
        }
    }
}