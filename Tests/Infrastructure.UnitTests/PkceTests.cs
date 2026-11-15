using System.Text.RegularExpressions;
using Application.Interfaces;
using Infrastructure.Security;

namespace Infrastructure.UnitTests;

[TestClass]
public sealed class PkceTests
{
    private readonly IOAuthChallengeFactory _factory = new OAuthChallengeFactory();

    /// <summary>
    /// Known-answer test from RFC 7636 Appendix B. This is the one property that must be exactly
    /// right — if the challenge doesn't match what Spotify derives from the verifier, every token
    /// exchange fails.
    /// </summary>
    [TestMethod]
    public void CodeChallenge_MatchesRfc7636ReferenceVector()
    {
        const string Verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        const string ExpectedChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";

        Assert.AreEqual(ExpectedChallenge, Pkce.CreateCodeChallenge(Verifier));
    }

    [TestMethod]
    public void CodeVerifier_LengthIsWithinRfcBounds()
    {
        var verifier = Pkce.CreateCodeVerifier();

        // RFC 7636 section 4.1 requires 43-128 characters.
        Assert.IsTrue(verifier.Length is >= 43 and <= 128, $"Length was {verifier.Length}.");
    }

    [TestMethod]
    public void GeneratedValues_AreBase64UrlSafe()
    {
        // Must survive a query string and an HTTP form without escaping.
        var pattern = new Regex("^[A-Za-z0-9_-]+$");

        Assert.IsTrue(pattern.IsMatch(Pkce.CreateCodeVerifier()));
        Assert.IsTrue(pattern.IsMatch(Pkce.CreateState()));
        Assert.IsTrue(pattern.IsMatch(Pkce.CreateCodeChallenge(Pkce.CreateCodeVerifier())));
    }

    [TestMethod]
    public void State_IsUnpredictable()
    {
        // The whole point of replacing the user id with a random value: it must not repeat.
        var values = Enumerable.Range(0, 500).Select(_ => Pkce.CreateState()).ToHashSet();

        Assert.AreEqual(500, values.Count, "State values must be unique.");
    }

    [TestMethod]
    public void Challenge_CarriesAMatchingVerifierAndChallenge()
    {
        var challenge = _factory.Create();

        Assert.AreEqual(
            Pkce.CreateCodeChallenge(challenge.CodeVerifier),
            challenge.CodeChallenge,
            "The challenge must be derived from the verifier it ships with.");
        Assert.AreNotEqual(challenge.State, challenge.CodeVerifier,
            "State and verifier must be independent secrets.");
    }
}
