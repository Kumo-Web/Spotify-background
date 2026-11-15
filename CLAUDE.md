# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET 10)
```bash
# Run from solution root
dotnet restore
dotnet build

# Run the API
dotnet run --project src/API          # https://localhost:7249

# Run all tests
dotnet test

# Run a single test project
dotnet test Tests/Application.UnitTests
dotnet test Tests/Infrastructure.UnitTests
dotnet test Tests/Domain.UnitTest
```

There are no EF Core migrations. Persistence is the Cosmos SDK directly. Against the emulator the
database and container are created at startup by `InfrastructureDI.InitialiseCosmosAsync`; in
Azure, `infra/main.bicep` creates them and startup only verifies they exist, because managed
identity (data-plane RBAC) cannot create databases or containers.

### Frontend (Next.js 15, lives at `web/`)
```bash
cd web
pnpm install
pnpm dev        # http://localhost:3000
pnpm build      # must pass before committing
pnpm typecheck
pnpm lint
```

### Local Cosmos emulator (required — the API refuses to boot without it)
```bash
docker run --detach --name cosmos \
  -p 8081:8081 -p 8080:8080 -p 1234:1234 \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest \
  --protocol https
```
`--protocol https` is not optional: the emulator defaults to HTTP and the .NET SDK has no HTTP
mode. Wait for `http://localhost:8080/ready` before starting the API. Data explorer is on
`http://localhost:1234`. The `vnext-latest` image is the one that runs on Apple Silicon; the
older `:latest` tag is amd64-only.

## Architecture

Clean Architecture (.NET 10) + Next.js App Router. Strict layer dependency rules:

```
API → Application → Domain
Infrastructure → Application + Domain
```

**Layer responsibilities:**
- `src/Domain` — Entities (`SpotifyUser`, `SpotifyToken`, `OAuthState`) and config models
  (`SpotifySettings`, `CosmosSettings`, `FrontendSettings`, `DataProtectionSettings`). No
  external dependencies.
- `src/Application` — Interfaces and application services (`TokenService`). Owns
  `ISpotifyAuthClient`, `ITokenService`, `ICurrentUserAccessor`, `IOAuthChallengeFactory`,
  the repository contracts under `Interfaces/Repositories/`, and `ITokenProtector`.
- `src/Infrastructure` — Implements the Application interfaces. Cosmos persistence, Spotify API
  access via `SpotifyAPI.Web`, PKCE and token protection. Wired up in `InfrastructureDI.cs`.
- `src/API` — ASP.NET Core controllers, `Program.cs`, middleware, cookie auth.
- `web/` — the Next.js frontend. It is a sibling of `src/`, not nested inside the API project.

**Key design patterns:**

`SpotifyClientFactory` is the central entry point for all Spotify API calls. Services take
`ISpotifyClientFactory` and call `CreateAsync()` — note it takes no user id, because identity
comes from the authenticated principal via `ICurrentUserAccessor`. The factory delegates to
`ITokenService`, which handles expiry and refresh transparently.

Persistence is one Cosmos container holding every document type, partitioned by
`/spotifyUserId` with a `type` discriminator field. `CosmosStore` is the only place Cosmos
plumbing lives; repositories depend on it rather than on `CosmosClient`. Every access pattern is
a point read or a single-partition query, which keeps usage inside the free tier. `OAuthState`
expiry is enforced by Cosmos per-document TTL, so there is no cleanup job.

**Auth flow — read this before touching anything auth-related.** `GET /api/auth/login` generates
a high-entropy server-side `state` plus a PKCE verifier, persists them, and redirects to Spotify.
`GET /api/auth/callback` consumes the state single-use (ETag-guarded, so a replay loses the
race), exchanges the code, reads the profile to establish identity from the token itself, then
issues an **HttpOnly cookie session**. The Spotify access token is never sent to the browser and
never appears in a URL. Endpoints are deny-by-default via a fallback authorization policy —
public ones must opt out with `[AllowAnonymous]`.

No endpoint accepts a user id. Every route is "me", resolved from the session. Reintroducing a
user id parameter would reintroduce the IDOR this design removed.

## Configuration

Backend reads `appsettings.Development.json` (git-ignored). `appsettings.Example.json` is the
committed template. Secrets go in .NET User Secrets:
```bash
dotnet user-secrets set "SpotifySettings:ClientId" "..."     --project src/API
dotnet user-secrets set "SpotifySettings:ClientSecret" "..." --project src/API
dotnet user-secrets set "Sentry:Dsn" "..."                   --project src/API
```
The Cosmos key in `appsettings.Development.json` is the emulator's well-known public key, not a
secret — it is identical on every install.

Frontend reads `NEXT_PUBLIC_API_URL` from `web/.env.development`.

Local OAuth requires ngrok: `SpotifySettings:RedirectUrl` must be the ngrok HTTPS URL pointing at
`/api/auth/callback`, and must match a redirect URI registered in the Spotify dashboard exactly.
The free ngrok URL changes on every restart, so both sides need updating each session.

## Testing

MSTest + Moq. No real database or HTTP calls — services are mocked at the interface level. Test
projects mirror source layer names: `Application.UnitTests`, `Infrastructure.UnitTests`,
`Domain.UnitTest`.

## Conventions

- Comments explain *why*, especially where the current shape fixes a past bug. Several files
  carry notes about the vulnerability or misconfiguration they replaced — preserve that context
  rather than trimming it.
- British spelling in identifiers and comments (`Initialise`, `Sanitise`, `authorisation`).
- Code analysis runs as warnings; `CA1848` (LoggerMessage delegates) is widely unaddressed and is
  not worth fixing opportunistically.

## In-Progress / TODOs

- AI playlist generation (Anthropic Claude integration) is not yet implemented; the
  `ai-generation` rate-limit policy in `Program.cs` is defined ready for it but unused.
- `MeController` and `PlaylistsController` use `PrivateUser.Followers` and `FullPlaylist.Tracks`,
  both marked obsolete by SpotifyAPI.Web 7.4.2 following Spotify's Feb 2026 API changes. They
  still build but need migrating.
- Azure deployment is in progress and being built by hand as a learning exercise (Container
  Apps + Cosmos free tier + Blob/Key Vault key ring via managed identity). Until it is done,
  `DataProtection:KeyRingBlobUri` / `KeyVaultKeyId` are unset and the key ring is
  local-filesystem only.
