using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetryTest.Api.Data;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ProductsDataContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Add services to the container.
builder.Services.AddOpenTelemetry();
// Remove .StartWithHost() from the OpenTelemetry builder chain.
// The correct usage is to just configure OpenTelemetry as shown below.

builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation(opt =>
        {
            opt.EnrichWithHttpRequest = (activity, httpRequest) => activity.SetBaggage("UserId", "1234");
        })
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddSource("Tracing.NET")
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: "Tracing.NET")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

