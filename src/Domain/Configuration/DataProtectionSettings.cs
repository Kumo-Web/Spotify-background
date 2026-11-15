namespace Domain.Configuration;

/// <summary>
/// Where the Data Protection key ring lives.
/// <para>
/// Container Apps replicas are ephemeral, so the default in-memory/filesystem key ring would be
/// regenerated on every restart — invalidating session cookies and, worse, making previously
/// encrypted Spotify refresh tokens permanently undecryptable. The ring must outlive the
/// container.
/// </para>
/// </summary>
public sealed class DataProtectionSettings
{
    public const string SectionName = "DataProtection";

    /// <summary>Blob URI holding the key ring XML. Empty locally, where the filesystem ring is fine.</summary>
    public string? KeyRingBlobUri { get; set; }

    /// <summary>Key Vault key used to wrap the key ring at rest.</summary>
    public string? KeyVaultKeyId { get; set; }

    /// <summary>Distinguishes key rings if several apps ever share a store.</summary>
    public string ApplicationName { get; set; } = "spotify-ai-playlists";
}
