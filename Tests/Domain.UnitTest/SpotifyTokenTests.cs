using Domain.Entities;

namespace Domain.UnitTest;

[TestClass]
public sealed class SpotifyTokenTests
{
    private static SpotifyToken TokenExpiring(DateTimeOffset expiresAt) => new()
    {
        SpotifyUserId = "user",
        AccessToken = "access",
        RefreshToken = "refresh",
        ExpiresAt = expiresAt,
    };

    [TestMethod]
    public void IsExpired_False_WhenComfortablyInFuture()
        => Assert.IsFalse(
            TokenExpiring(DateTimeOffset.UtcNow.AddHours(1)).IsExpired(TimeSpan.FromSeconds(60)));

    [TestMethod]
    public void IsExpired_True_WhenPast()
        => Assert.IsTrue(
            TokenExpiring(DateTimeOffset.UtcNow.AddMinutes(-1)).IsExpired(TimeSpan.FromSeconds(60)));

    [TestMethod]
    public void IsExpired_True_WhenInsideSkewWindow()
        => Assert.IsTrue(
            TokenExpiring(DateTimeOffset.UtcNow.AddSeconds(30)).IsExpired(TimeSpan.FromSeconds(60)),
            "A token expiring sooner than the skew must be treated as expired.");

    [TestMethod]
    public void IsExpired_False_JustOutsideSkewWindow()
        => Assert.IsFalse(
            TokenExpiring(DateTimeOffset.UtcNow.AddSeconds(120)).IsExpired(TimeSpan.FromSeconds(60)));
}
