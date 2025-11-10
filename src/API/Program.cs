using System.Diagnostics;
using Application.Interfaces;
using Application.Services;
using Infrastructure;
using Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


var builder = WebApplication.CreateBuilder(args);
var corsPolicyName = "AllowAllOrigins";
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var logginService = "SpotifyLogger";

builder.Services.AddInfrastructureDI(builder.Configuration);
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name:corsPolicyName,
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

#region Sentry
SentrySdk.CaptureMessage("Hello Sentry");

builder.WebHost.UseSentry(opt =>
{
    opt.Dsn = builder.Configuration["Sentry:Dsn"];
    opt.TracesSampleRate = 1.0;
    opt.Debug = true;
});
#endregion

#region OpenTelemetry
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(logginService))
        .AddConsoleExporter();
});
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(logginService))
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddConsoleExporter())
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddConsoleExporter());
#endregion

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);
app.MapControllers();

app.Run();