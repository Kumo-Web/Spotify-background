using System.Threading.RateLimiting;
using API.Auth;
using API.Middleware;
using Application.Exceptions;
using Application.Interfaces;
using Azure.Identity;
using Domain.Configuration;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var frontend = builder.Configuration.GetSection(FrontendSettings.SectionName).Get<FrontendSettings>()
               ?? new FrontendSettings();
var dataProtection = builder.Configuration.GetSection(DataProtectionSettings.SectionName)
                         .Get<DataProtectionSettings>() ?? new DataProtectionSettings();

const string CorsPolicy = "frontend";
const string GenerationRateLimit = "ai-generation";

// ---------------------------------------------------------------------------------------------
// Observability. Configured first, before any code can capture, and read from configuration
// rather than hardcoded — previously Debug and a 100% trace rate were on in every environment,
// and a CaptureMessage call ran *before* UseSentry, hitting an unconfigured hub and vanishing.
// ---------------------------------------------------------------------------------------------
builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"] ?? string.Empty;
    options.Environment = builder.Environment.EnvironmentName;

    options.Debug = builder.Configuration.GetValue("Sentry:Debug", false);

    // Errors: capture everything. 5,000/month is ample for a personal app.
    options.SampleRate = 1.0f;

    // Traces: sampled per transaction to protect the monthly span budget. See SentryScrubbing.
    options.TracesSampler = SentryScrubbing.SampleTransaction;

    options.MinimumBreadcrumbLevel = LogLevel.Information;
    options.MinimumEventLevel = LogLevel.Error;

    // Off deliberately: this app's PII is someone's music history and a Spotify id.
    options.SendDefaultPii = false;
    options.MaxRequestBodySize = Sentry.Extensibility.RequestSize.None;

    options.SetBeforeSend(SentryScrubbing.Scrub);

    // 404s are routine for an API and would otherwise fill the error budget.
    options.TraceIgnoreStatusCodes.Add(404);
});

// ---------------------------------------------------------------------------------------------
// Data Protection. Protects both the auth cookie and Spotify tokens at rest, so the key ring
// must outlive any single container: without a persisted ring, a restart would invalidate every
// session AND make stored refresh tokens permanently undecryptable.
// ---------------------------------------------------------------------------------------------
var dataProtectionBuilder = builder.Services
    .AddDataProtection()
    .SetApplicationName(dataProtection.ApplicationName);

if (!string.IsNullOrWhiteSpace(dataProtection.KeyRingBlobUri))
{
    var credential = new DefaultAzureCredential();

    dataProtectionBuilder.PersistKeysToAzureBlobStorage(
        new Uri(dataProtection.KeyRingBlobUri), credential);

    if (!string.IsNullOrWhiteSpace(dataProtection.KeyVaultKeyId))
    {
        dataProtectionBuilder.ProtectKeysWithAzureKeyVault(
            new Uri(dataProtection.KeyVaultKeyId), credential);
    }
}
// Otherwise the local filesystem ring is used, which is correct for development.

// ---------------------------------------------------------------------------------------------
// Application services
// ---------------------------------------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

// Container Apps terminates TLS at its ingress, so without this the app sees HTTP and would
// refuse to set a Secure cookie.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------------------------------------------------------------------------------------------
// Authentication — a session cookie, issued after Spotify OAuth. The Spotify access token is
// never given to the browser.
// ---------------------------------------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "spotify.session";
        options.Cookie.HttpOnly = true;              // unreadable from JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        // Lax, not Strict: the browser arrives here via Spotify's cross-site redirect, and a
        // Strict cookie would not be sent on that navigation.
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;

        // This is an API: answer with status codes, not redirects to a login page that doesn't exist.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Deny by default. Every endpoint requires authentication unless it opts out with
// [AllowAnonymous], so a new controller cannot be accidentally left public.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// ---------------------------------------------------------------------------------------------
// CORS — an explicit allowlist. AllowAnyOrigin (the previous policy) cannot carry credentials,
// so a cookie-authenticated API needs named origins anyway.
// ---------------------------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(frontend.AllowedOrigins)
        .AllowCredentials()
        .WithHeaders("Content-Type", "Accept")
        .WithMethods("GET", "POST", "PUT", "DELETE"));
});

// ---------------------------------------------------------------------------------------------
// Rate limiting. Partitioned by session where there is one, IP otherwise.
// ---------------------------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKeyFor(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
            }));

    // Deliberately strict. Each AI generation costs real money, so an unbounded endpoint is a
    // cost-denial-of-service vector, not merely a load one.
    options.AddPolicy(GenerationRateLimit, context =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKeyFor(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromHours(1),
            }));

    static string PartitionKeyFor(HttpContext context)
        => context.User.FindFirst(SessionClaims.SpotifyUserId)?.Value
           ?? context.Connection.RemoteIpAddress?.ToString()
           ?? "unknown";
});

var app = builder.Build();

// ---------------------------------------------------------------------------------------------
// Pipeline. Order matters: exception handling outermost, then transport, then identity.
// ---------------------------------------------------------------------------------------------
app.UseForwardedHeaders();

app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();

    // Map known failures to meaningful statuses; everything else is a 500 with no detail, so
    // internals are never echoed back to a client.
    var (status, title) = feature?.Error switch
    {
        SpotifyNotConnectedException => (StatusCodes.Status409Conflict, "Spotify is not connected"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status,
        Title = title,
        Instance = context.Request.Path,
    });
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseCors(CorsPolicy);
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Excluded from auth and from tracing (see the sampler) so probes are cheap and silent.
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/ready", () => Results.Ok(new { status = "ready" })).AllowAnonymous();

// Create the Cosmos database and container on first boot.
await InfrastructureDI.InitialiseCosmosAsync(app.Services);

await app.RunAsync();
