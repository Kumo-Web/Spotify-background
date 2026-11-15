using Sentry;
using Sentry.Extensibility;

namespace API.Middleware;

/// <summary>
/// Strips credentials and personal data from events before they leave the process.
/// <para>
/// Sentry is a third party. Anything sent there has left our trust boundary, so tokens, cookies
/// and auth headers must never be in the payload — an error report is not worth leaking a live
/// Spotify credential.
/// </para>
/// </summary>
internal static class SentryScrubbing
{
    private static readonly string[] SensitiveHeaders =
    [
        "Authorization", "Cookie", "Set-Cookie", "X-Api-Key",
    ];

    private static readonly string[] SensitiveKeyFragments =
    [
        "token", "secret", "password", "code_verifier", "client_secret", "refresh",
    ];

    public static SentryEvent? Scrub(SentryEvent @event)
    {
        // The hostname identifies infrastructure and adds nothing diagnostically here.
        @event.ServerName = null;

        if (@event.Request is not null)
        {
            foreach (var header in SensitiveHeaders)
            {
                @event.Request.Headers.Remove(header);
            }

            // Query strings can carry an OAuth code or state; the path alone is enough to debug.
            @event.Request.QueryString = null;
        }

        // Extra and Tags are exposed read-only, so redact through the event's own API rather
        // than mutating the dictionaries.
        foreach (var key in @event.Extra.Keys.Where(IsSensitive).ToList())
        {
            @event.SetExtra(key, "[redacted]");
        }

        foreach (var key in @event.Tags.Keys.Where(IsSensitive).ToList())
        {
            @event.UnsetTag(key);
        }

        return @event;
    }

    private static bool IsSensitive(string key)
        => SensitiveKeyFragments.Any(f => key.Contains(f, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Sampling decisions per transaction. Chosen to protect the free plan's 5M-span monthly
    /// budget: Container Apps health probes fire every few seconds, which at 100% sampling would
    /// generate tens of thousands of transactions a day on their own and crowd out real traffic.
    /// </summary>
    public static double? SampleTransaction(TransactionSamplingContext context)
    {
        var name = context.TransactionContext.Name;

        if (name.Contains("/health", StringComparison.OrdinalIgnoreCase)
            || name.Contains("/ready", StringComparison.OrdinalIgnoreCase))
        {
            return 0.0;
        }

        // Rare, slow and expensive — always worth a full trace.
        if (name.Contains("/generate", StringComparison.OrdinalIgnoreCase)
            || name.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }

        return 0.25;
    }
}
