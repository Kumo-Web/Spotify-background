namespace Application.Helper;

public class TokenHelper
{
    public static DateTime CalculateTokenExpiryTime(DateTime receivedAtUtc, int expiresInSeconds)
    {
        return receivedAtUtc.AddSeconds(expiresInSeconds);
    }

    public static bool IsTokenExpired(DateTime expirationUtc)
    {
        DateTime currentTimeUtc = DateTime.UtcNow;
        return expirationUtc <= currentTimeUtc;
    }
}
