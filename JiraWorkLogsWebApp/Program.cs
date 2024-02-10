using JiraWorkLogsWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Utils;

namespace JiraWorkLogsWebApp
{
    public class Program
    {
        internal static ActivitySource JiraActivitySource;
        internal static Counter<int> CountGreetings;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = Constants.DatabaseConnectionString ?? throw new InvalidOperationException("Connection string not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            JiraActivitySource = new ActivitySource(builder.Environment.ApplicationName);
            var greeterMeter = new Meter(builder.Environment.ApplicationName + ".Greeter");
            CountGreetings = greeterMeter.CreateCounter<int>("greetings.count", description: "Counts the number of greetings");

            var telemetryBuilder = builder.Services.AddOpenTelemetry();
            telemetryBuilder.ConfigureResource(b => b.AddService(builder.Environment.ApplicationName));
            telemetryBuilder.WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()                         // Metrics provider from OpenTelemetry
                .AddMeter(greeterMeter.Name)
                .AddMeter("Microsoft.AspNetCore.Hosting")               // Metrics provides by ASP.NET Core in .NET 8
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")        // Metrics provides by ASP.NET Core in .NET 8
                .AddPrometheusExporter());
            telemetryBuilder.WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(JiraActivitySource.Name)
                .AddZipkinExporter(b => b.Endpoint = Constants.ZipkinEndpoint)
                .AddConsoleExporter());
            // for otlp
            //tracing.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(tracingOtlpEndpoint));

            var app = builder.Build();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
