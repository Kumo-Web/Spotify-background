using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Security;

/// <summary>
/// PKCE (RFC 7636) and OAuth state generation.
/// <para>
/// PKCE was absent entirely before this. It binds the authorisation code to the client that
/// requested it, so an intercepted code cannot be redeemed by anyone else.
/// </para>
/// </summary>
internal static class Pkce
{
    /// <summary>
    /// 32 bytes of cryptographic randomness, base64url-encoded to 43 characters — the minimum
    /// RFC 7636 allows, and ample entropy.
    /// </summary>
    public static string CreateCodeVerifier() => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    /// <summary>The S256 challenge. Plain is permitted by the RFC but weaker; we never use it.</summary>
    public static string CreateCodeChallenge(string codeVerifier)
        => Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

    /// <summary>
    /// A single-use CSRF token for the OAuth <c>state</c> parameter. Unguessable, and carries no
    /// meaning — it is purely a key into server-side state, unlike the user id it replaces.
    /// </summary>
    public static string CreateState() => Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
