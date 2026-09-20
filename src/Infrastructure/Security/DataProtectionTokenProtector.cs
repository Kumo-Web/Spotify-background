using Application.Interfaces.Security;
using Microsoft.AspNetCore.DataProtection;

namespace Infrastructure.Security;

/// <summary>
/// Protects Spotify tokens using ASP.NET Core Data Protection.
/// <para>
/// Data Protection rather than hand-rolled AES: it gives authenticated encryption, key rotation
/// and purpose isolation for free, and the key ring is already persisted to Blob Storage and
/// wrapped by Key Vault for the auth cookie. One key story, not two.
/// </para>
/// </summary>
internal sealed class DataProtectionTokenProtector : ITokenProtector
{
    /// <summary>
    /// Purpose string. Isolates these payloads cryptographically from any other protector in the
    /// app, so a token ciphertext cannot be replayed as (say) a cookie. Versioned so the scheme
    /// can change later without ambiguity.
    /// </summary>
    private const string Purpose = "SpotifyToken.v1";

    private readonly IDataProtector _protector;

    public DataProtectionTokenProtector(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector(Purpose);

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}
