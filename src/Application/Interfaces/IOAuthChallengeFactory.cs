using Application.Models;

namespace Application.Interfaces;

/// <summary>
/// Generates OAuth login challenges. A contract rather than a static helper so the cryptographic
/// primitives stay in Infrastructure and the controller has no reason to reach for them.
/// </summary>
public interface IOAuthChallengeFactory
{
    OAuthChallenge Create();
}
