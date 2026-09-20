namespace Application.Interfaces.Security;

/// <summary>
/// Encrypts and decrypts Spotify tokens so they are never stored in plaintext.
/// <para>
/// Previously both the access and refresh token were persisted as unbounded plaintext columns.
/// A refresh token is a long-lived credential to someone's Spotify account, so at-rest
/// encryption is the baseline.
/// </para>
/// </summary>
public interface ITokenProtector
{
    string Protect(string plaintext);

    /// <summary>
    /// Reverses <see cref="Protect"/>.
    /// </summary>
    /// <exception cref="System.Security.Cryptography.CryptographicException">
    /// If the payload was tampered with, or the key ring that encrypted it is gone.
    /// </exception>
    string Unprotect(string ciphertext);
}
