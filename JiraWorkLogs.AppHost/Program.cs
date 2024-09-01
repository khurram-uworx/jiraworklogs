using Aspire.Hosting;
using System;

var builder = DistributedApplication.CreateBuilder(args);

//https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-python?tabs=bash
//builder.AddPythonProject("hello-python", "../hello-python", "main.py")
//       .WithEndpoint(targetPort: 8111, scheme: "http", env: "PORT")
//       //.WithEnvironment("OTEL_PYTHON_OTLP_TRACES_SSL", "false")
//       //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://localhost:21034")
//       .WithEnvironment("OTEL_EXPORTER_OTLP_INSECURE", "true");
//       //.WithEnvironment("OTEL_EXPORTER_OTLP_TRACES_INSECURE", "false");

var worklogsDB = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("worklogs");
var redis = builder.AddRedis("redis")
    //.WithRedisCommander()
    .WithDataVolume();
var rabbit = builder.AddRabbitMQ("rabbitmq");

var url = Environment.GetEnvironmentVariable("JIRA_URL");
var user = Environment.GetEnvironmentVariable("JIRA_USER");
var token = Environment.GetEnvironmentVariable("JIRA_TOKEN");

builder.AddProject<Projects.JiraWorkLogsService>("jiraworklogsservice")
    .WithEnvironment("JIRA_URL", url)
    .WithEnvironment("JIRA_USER", user)
    .WithEnvironment("JIRA_TOKEN", token)
    .WithReference(worklogsDB)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.JiraWorkLogsWebApp>("jiraworklogswebapp")
    .WithReference(worklogsDB)
    .WithReference(redis)
    .WithReference(rabbit);

builder.Build().Run();
