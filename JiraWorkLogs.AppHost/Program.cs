using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.AddPythonProject("hello-python", "../hello-python", "main.py")
       .WithEndpoint(targetPort: 8111, scheme: "http", env: "PORT")
       //.WithEnvironment("OTEL_PYTHON_OTLP_TRACES_SSL", "false")
       //.WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "https://localhost:21034")
       .WithEnvironment("OTEL_EXPORTER_OTLP_INSECURE", "true");
       //.WithEnvironment("OTEL_EXPORTER_OTLP_TRACES_INSECURE", "false");

builder.Build().Run();
