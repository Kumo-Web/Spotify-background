using Application.Interfaces;
using Application.Services;
using Infrastructure;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var logginService = "SpotifyLogger";

builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(logginService))
        .AddConsoleExporter();
});
SentrySdk.CaptureMessage("Hello Sentry");

builder.WebHost.UseSentry(opt =>
{
    opt.Dsn = builder.Configuration["Sentry:Dsn"];
    opt.TracesSampleRate = 1.0;
    opt.Debug = true;
});
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(logginService))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddConsoleExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
