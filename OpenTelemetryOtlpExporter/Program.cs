
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
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

        // Define service metadata for observability
        var serviceName = "DigitalBanking.Api";
        var serviceVersion = "1.0.0";

        // Add OpenTelemetry
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(o =>
                    {
                        //Protocol → Grpc(default, recommended) or HttpProtobuf.
                        o.Endpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

                    });
                    //.AddOtlpExporter(o =>
                    // {
                    //     o.Endpoint = new Uri("https://otlp-gateway.example.com:4318");
                    //     o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    //     o.Headers = "Authorization=Bearer my-secret-token";
                    // });
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                     .AddOtlpExporter(o =>
                     {
                         o.Endpoint = new Uri("http://otel-collector:4317"); // OTLP gRPC
                         o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                     });
            });




        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add CORS policy to allow all origins, methods, and headers
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowAll");

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
//🔹 Debugging Tips

//Check app logs for OpenTelemetry errors.
//If traces/metrics don’t appear, make sure ports are open:
//4317 → OTLP gRPC
//4318 → OTLP HTTP
//In Collector, enable logging exporter to verify data is received.
