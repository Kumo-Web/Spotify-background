# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET 10)
```bash
# Run from solution root
dotnet restore
dotnet build

# Run the API (from src/API)
dotnet run --project src/API

# Run all tests
dotnet test

# Run a single test project
dotnet test Tests/Application.UnitTests
dotnet test Tests/Infrastructure.UnitTests
dotnet test Tests/Domain.UnitTest

# EF Core migrations (run from solution root)
dotnet ef migrations add <MigrationName> -p src/Infrastructure -s src/API -c Infrastructure.DatabaseContext.SpotifyDbContext
dotnet ef database update -p src/Infrastructure -s src/API
```

### Frontend (Next.js, lives at `src/API/UI`)
```bash
cd src/API/UI
npm install
npm run dev    # http://localhost:3000
```

## Architecture

This is a Clean Architecture (.NET 9) + Next.js App Router fullstack app. The backend follows strict layer dependency rules:

```
API → Application → Domain
Infrastructure → Application + Domain
```

**Layer responsibilities:**
- `src/Domain` — Entities (`User`, `SpotifyToken`) and config models (`SpotifySettings`). No external dependencies.
- `src/Application` — Interfaces (contracts) and application services (`TokenService`). Defines `ISpotifyAuthClient`, `ITokenService`, `ITokenRepository`, `IUserRepository`.
- `src/Infrastructure` — Implements all Application interfaces. Contains EF Core `SpotifyDbContext`, repositories, and all Spotify API interaction via `SpotifyAPI.Web`. Wired up in `InfrastructureDI.cs`.
- `src/API` — ASP.NET Core controllers, `Program.cs`, and the Next.js frontend under `UI/`.

**Key design patterns:**

`SpotifyClientFactory` is the central entry point for all Spotify API calls. Every service that needs to call Spotify receives `ISpotifyClientFactory`, calls `CreateSpotifyClient(userId)`, and gets back an authenticated `SpotifyClient`. The factory internally delegates to `ITokenService` which handles token expiry and refresh transparently.

Token lifecycle: `TokenService.GetValidAccessTokenAsync` checks expiry via `TokenHelper`, refreshes via `ISpotifyAuthClient` if expired, persists the new token, and returns it. The `SpotifyToken` entity uses `UserId` (Guid) as its primary key.

OAuth flow: `SpotifyAuthController.Login` generates a Spotify auth URL with the userId encoded as the OAuth `state` parameter. The `Callback` endpoint exchanges the code for tokens and redirects the browser to the Next.js frontend with the access token in the query string.

**Infrastructure interfaces split:** Infrastructure-specific service contracts (e.g., `ISpotifyClientFactory`, `ISpotifyUserService`, `ISpotifyTrackService`, `IPlaylistService`) live in `src/Infrastructure/Interfaces/` and are NOT in the Application layer — only repositories and auth client contracts live in `src/Application/Interfaces/`.

## Configuration

Backend reads from `appsettings.Development.json`. Secrets go in .NET User Secrets:
```bash
dotnet user-secrets set "SpotifySettings:ClientSecret" "..."
dotnet user-secrets set "AnthropicSettings:ApiKey" "..."
dotnet user-secrets set "Sentry:Dsn" "..."
```

Frontend reads `NEXT_PUBLIC_API_URL` from `src/API/UI/.env.local`.

Local OAuth requires ngrok: the `SpotifySettings:RedirectUrl` must be the ngrok HTTPS URL pointing to `/api/spotifyAuth/callback`.

## Testing

Tests use MSTest + Moq. No real database or HTTP calls — services are fully mocked at the interface level. Test projects mirror source layer names: `Application.UnitTests`, `Infrastructure.UnitTests`, `Domain.UnitTest`.

## In-Progress / TODOs

- `User` ↔ `SpotifyToken` relationship is commented out in both entities (pending schema work).
- Several `PlaylistService` methods throw `NotImplementedException` — the implementations are commented out above each stub.
- AI playlist generation (Anthropic Claude integration) is not yet implemented.
- The hardcoded `frontendUrl = "http://localhost:3000"` in `SpotifyAuthController.Callback` needs to become configurable.
