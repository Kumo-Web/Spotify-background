namespace Domain.Configuration;

/// <summary>
/// Where the browser-facing app lives. Replaces the hardcoded
/// <c>frontendUrl = "http://localhost:3000"</c> in the OAuth callback.
/// </summary>
public sealed class FrontendSettings
{
    public const string SectionName = "Frontend";

    /// <summary>Base URL the OAuth callback redirects back to after login.</summary>
    public string BaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// Exact origins permitted by CORS. An explicit allowlist — the previous policy was
    /// <c>AllowAnyOrigin</c>, which is incompatible with cookie credentials anyway.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000"];
}
