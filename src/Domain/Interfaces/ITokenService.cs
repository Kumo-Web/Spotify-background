namespace Domain.Interfaces;

public interface ITokenService
{
    Task<string> GetValidAccessTokenAsync();
}
