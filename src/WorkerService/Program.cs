using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WorkerService;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configure metrics
builder.Services.AddOpenTelemetryMetrics(builder =>
{
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddTelemetrySdk().AddEnvironmentVariableDetector());
    builder.AddHttpClientInstrumentation();
    builder.AddAspNetCoreInstrumentation();
    builder.AddMeter("*");
    builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Configure tracing
builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddTelemetrySdk().AddEnvironmentVariableDetector());
    builder.AddHttpClientInstrumentation();
    builder.AddAspNetCoreInstrumentation();
    builder.AddSource("*");
    builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://localhost:4317"));
});

// Configure logging
builder.Logging.AddOpenTelemetry(builder =>
{
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddTelemetrySdk().AddEnvironmentVariableDetector());
    builder.IncludeFormattedMessage = true;
    builder.IncludeScopes = true;
    builder.ParseStateValues = true;
    builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
});

builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.Run();