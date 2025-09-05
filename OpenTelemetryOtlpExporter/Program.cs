using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.Xml;
using static System.Net.WebRequestMethods;
//When to Use OTLP Exporter?

//✅ If you use OpenTelemetry Collector (recommended)
//→ Your app → OTLP → Collector → (Prometheus, Tempo, Jaeger, Loki, Grafana, etc.)

//✅ If your backend (Grafana Cloud, Datadog, NewRelic, etc.) supports OTLP natively
//→ Your app → OTLP → Backend

namespace OpenTelemetryPrometeusExporter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ---- Configuration constants ----
        const string serviceName = "DigitalBanking.Api";
        const string serviceVersion = "1.0.0";
        var otlpEndpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC

        // ---- Logging (remove default providers, add console + OTLP) ----
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.AddOtlpExporter(o =>
            {
                o.Endpoint = otlpEndpoint;
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
        });

        // ---- OpenTelemetry (traces + metrics, no duplication) ----
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                                     .AddAttributes(new KeyValuePair<string, object>[]
                                     {
                                         new("deployment.environment", builder.Environment.EnvironmentName)
                                     }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = otlpEndpoint;
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = otlpEndpoint;
                    o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                }));

        // ---- MVC / Swagger / CORS ----
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", p =>
                p.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader());
        });

        var app = builder.Build();

        var enableSwagger = builder.Configuration.GetValue("ENABLE_SWAGGER", true);
        if (enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Optional: remove if only HTTP is exposed
        app.UseHttpsRedirection();

        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapControllers();

        // Startup log
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Service {Service} v{Version} started at {UtcTime} (Environment={Env})",
            serviceName, serviceVersion, DateTimeOffset.UtcNow, app.Environment.EnvironmentName);

        app.Run();
    }
}
//🔹 Debugging Tips

//Check app logs for OpenTelemetry errors.
//If traces/metrics don’t appear, make sure ports are open:
//4317 → OTLP gRPC
//4318 → OTLP HTTP
//In Collector, enable logging exporter to verify data is received.

//ASP.NET Core App
//   (push OTLP gRPC/HTTP)
//           ↓
//   OpenTelemetry Collector
//   (aggregates, transforms)
//           ↓
//   Prometheus scrapes Collector(/metrics)
//           ↓
//   Grafana visualizes

//🔹 Why This Setup Is Better?

//Separation of concerns:
//Your app only knows how to send telemetry to the Collector.

//Centralized configuration:
//Collector decides what to keep, drop, or transform before sending to Prometheus, Tempo, Loki, etc.

//Scalability:
//Apps don’t expose /metrics endpoints individually; Prometheus scrapes fewer, centralized endpoints.

//Flexibility:
//You can fan out metrics to multiple backends without changing app code.

