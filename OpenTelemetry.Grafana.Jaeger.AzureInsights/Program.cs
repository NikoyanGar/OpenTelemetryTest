using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryTest.Api.Data;

namespace OpenTelemetry.Grafana.Jaeger.AzureInsights;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add DbContext with connection string from configuration
        builder.Services.AddDbContext<ProductsDataContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddOpenTelemetry()
            .WithTracing(traceProviderBuilder =>
            {
                traceProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("opTele"))
                .AddAzureMonitorTraceExporter(options =>
                {
                    options.ConnectionString = builder.Configuration["AzureMonitor:ConnectionString"];
                });
            })
            .WithMetrics(metrProviderBuilder =>
            {
                metrProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("opTele"))
                .AddAzureMonitorMetricExporter(options =>
                {
                    options.ConnectionString = builder.Configuration["AzureMonitor:ConnectionString"];
                });
            });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
