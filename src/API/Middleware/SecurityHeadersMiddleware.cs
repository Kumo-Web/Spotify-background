namespace API.Middleware;

/// <summary>
/// Adds response security headers. None of these were present before.
/// </summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // This is a JSON API that renders nothing, so the policy can be maximally restrictive:
        // no scripts, no styles, no framing, no plugins.
        headers["Content-Security-Policy"] =
            "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

        // Stop browsers second-guessing declared content types (JSON sniffed as HTML is an XSS vector).
        headers["X-Content-Type-Options"] = "nosniff";

        headers["X-Frame-Options"] = "DENY";

        // Never leak API paths — which can carry ids — to third parties via Referer.
        headers["Referrer-Policy"] = "no-referrer";

        // This API needs none of these device capabilities.
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Belt and braces alongside the CORS policy: no cross-origin resource access.
        headers["Cross-Origin-Resource-Policy"] = "same-site";

        await next(context);
    }
}

internal static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
