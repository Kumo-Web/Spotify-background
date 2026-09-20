using Application.Interfaces;
using Application.Models;

namespace Infrastructure.Security;

internal sealed class OAuthChallengeFactory : IOAuthChallengeFactory
{
    public OAuthChallenge Create()
    {
        var verifier = Pkce.CreateCodeVerifier();

        return new OAuthChallenge(
            State: Pkce.CreateState(),
            CodeVerifier: verifier,
            CodeChallenge: Pkce.CreateCodeChallenge(verifier));
    }
}
