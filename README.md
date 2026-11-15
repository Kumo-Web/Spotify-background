# Spotify AI Playlists

A full-stack app that connects to your Spotify account: your profile, top artists and tracks,
what's playing now, and your playlists. AI playlist generation with Anthropic Claude is planned
but not built yet.

## Tech stack

**Backend** (`src/`)
- ASP.NET Core on .NET 10, Clean Architecture (`Domain` ← `Application` ← `Infrastructure`, `API`)
- Azure Cosmos DB through the Cosmos SDK. There is **no Entity Framework and no migrations**:
  the API creates the database and container itself at startup when it runs against the
  emulator.
- Spotify OAuth (authorisation code + PKCE), with SpotifyAPI.Web 7.4.2
- HttpOnly cookie sessions. The Spotify access token never reaches the browser.
- Sentry for errors

**Frontend** (`web/`)
- Next.js 15 (App Router), React 19, TypeScript, Tailwind, three.js
- pnpm

## How the pieces fit together locally

```
Browser ──► https://<your-ngrok-domain> ──► ngrok ──► Next.js  http://localhost:3000
                                                        │
                                                        │  /api/* is proxied (next.config.ts)
                                                        ▼
                                                   .NET API  https://localhost:7249
                                                        │
                                                        ▼
                                                 Cosmos emulator  https://localhost:8081
```

The browser only ever talks to **one host**, the Next.js site. Next.js forwards `/api/*` to
the API behind the scenes. This matters because the session cookie belongs to whichever host
answered the OAuth callback. If the browser talked to the API on a different host, the cookie
would never come back and every request after login would 401.

That's also why **ngrok points at the frontend (port 3000), not at the API**, and why you
should browse the app through the ngrok URL rather than `localhost:3000` when you want to log in.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/installation)
- [Docker](https://www.docker.com/) (for the Cosmos emulator)
- [ngrok](https://ngrok.com/) with a free static dev domain
- A [Spotify developer app](https://developer.spotify.com/dashboard)

## First-time setup

### 1. Claim your ngrok static domain

Free ngrok accounts get one domain that never changes, so you only configure it once.

1. Sign in at [dashboard.ngrok.com](https://dashboard.ngrok.com) and open **Domains**.
   Claim the free domain if you haven't yet. It looks like `something-random.ngrok-free.dev`.
2. Connect the CLI to your account (one time):
   ```bash
   ngrok config add-authtoken <token from the dashboard>
   ```

Everywhere below, `<your-domain>` means that hostname with no `https://` and no trailing slash.

### 2. Register the redirect URI with Spotify

In the [Spotify dashboard](https://developer.spotify.com/dashboard), open your app's settings
and add this redirect URI:

```
https://<your-domain>/api/auth/callback
```

It has to match `SpotifySettings:RedirectUrl` (step 3) character for character.

### 3. Configure the API

```bash
cp src/API/appsettings.Example.json src/API/appsettings.Development.json
```

`appsettings.Development.json` is git-ignored. Edit it:

```jsonc
"SpotifySettings": {
  "RedirectUrl": "https://<your-domain>/api/auth/callback",
  ...
},
"Cosmos": {
  "Endpoint": "https://localhost:8081",
  // The emulator's well-known key. It is the same on every install, so it is not a secret.
  "Key": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
  ...
},
"Frontend": {
  // Where the API sends the browser after login. No trailing slash.
  "BaseUrl": "https://<your-domain>",
  // Exact origins: scheme + host, no trailing slash, or they will never match.
  "AllowedOrigins": [ "http://localhost:3000", "https://<your-domain>" ]
}
```

Put the real secrets in user secrets, never in the file:

```bash
dotnet user-secrets set "SpotifySettings:ClientId"     "<client id>"     --project src/API
dotnet user-secrets set "SpotifySettings:ClientSecret" "<client secret>" --project src/API
dotnet user-secrets set "Sentry:Dsn"                   "<dsn>"           --project src/API   # optional
```

### 4. Trust the .NET development certificate

The API serves HTTPS with the .NET dev certificate. Your browser and the Next.js proxy both
need to trust it.

```bash
dotnet dev-certs https --trust     # the browser and OS trust it
cd web && pnpm install && pnpm certs   # exports it to web/.certs/ for the Next.js proxy
```

`pnpm dev` passes that file to Node with `NODE_EXTRA_CA_CERTS`. Without it, the proxy refuses
the API's certificate and the UI shows "Can't reach the API".

### 5. Frontend environment

`web/.env.development` is committed and already right for local development:

```bash
NEXT_PUBLIC_API_URL=                      # empty = same origin, go through the proxy
API_PROXY_TARGET=https://localhost:7249   # where the proxy forwards /api/* (server side only)
```

Leave `NEXT_PUBLIC_API_URL` empty. Setting it makes the browser call the API directly, which
breaks the session cookie as described above.

## Running it

Use four terminals, started in this order.

**1. Cosmos emulator**
```bash
docker run --detach --name cosmos \
  -p 8081:8081 -p 8080:8080 -p 1234:1234 \
  mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest \
  --protocol https
```
After the first time, use `docker start cosmos`. Wait until `http://localhost:8080/ready`
answers before starting the API. `--protocol https` is required, because the .NET SDK cannot
talk to the emulator over HTTP. The `vnext-latest` image also runs on Apple Silicon.

Data Explorer is at `http://localhost:1234`.

**2. API**
```bash
dotnet run --project src/API      # https://localhost:7249
```
The API refuses to start if it cannot reach Cosmos, and the error says why.

**3. Frontend**
```bash
cd web
pnpm dev                          # http://localhost:3000
```

**4. ngrok**
```bash
ngrok http 3000 --url https://<your-domain>
```
(Older ngrok versions use `--domain <your-domain>` instead of `--url`.)

**Then open `https://<your-domain>`** and log in with Spotify. ngrok's free tier shows a
one-time "You are about to visit" warning page per browser. Click through it.

`http://localhost:3000` still works for UI work that doesn't need a session. But logging in
from there won't work: Spotify sends you back to the ngrok host, so the cookie is set on the
ngrok host, not on localhost.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| "Can't reach the API" in the UI | API not running, or `pnpm certs` not run (the proxy rejects the dev certificate) |
| Spotify says `INVALID_CLIENT: Invalid redirect URI` | Dashboard redirect URI and `SpotifySettings:RedirectUrl` differ, even by one character |
| Login completes but you land back on `/login` | You browsed `localhost:3000` instead of the ngrok domain, or `Frontend:BaseUrl` points somewhere else |
| ngrok `ERR_NGROK_8012` / bad gateway | `pnpm dev` isn't running on port 3000 |
| API fails at startup mentioning Cosmos | Emulator not ready yet, or started without `--protocol https` |

## Tests and checks

```bash
dotnet test                                  # all backend tests (MSTest + Moq, no real DB/HTTP)
dotnet test Tests/Application.UnitTests      # a single project

cd web
pnpm typecheck
pnpm lint
pnpm build                                   # must pass before committing
```

## API

Every endpoint is deny-by-default and needs the session cookie, except the ones marked
*anonymous*. No endpoint takes a user id: the user is always "me", taken from the session.

| Method | Path | |
|---|---|---|
| GET | `/api/auth/login?returnPath=` | *anonymous*. Starts OAuth and redirects to Spotify |
| GET | `/api/auth/callback` | *anonymous*. Spotify returns here and the session cookie is issued |
| GET | `/api/auth/session` | *anonymous*. `{ authenticated, displayName? }` |
| POST | `/api/auth/logout` | Ends the session |
| GET | `/api/me` | Profile |
| GET | `/api/me/top-artists?range=&limit=` | Top artists |
| GET | `/api/me/top-tracks?range=&limit=` | Top tracks |
| GET | `/api/me/now-playing` | Currently playing track |
| GET | `/api/playlists?limit=` | Your playlists |
| GET | `/api/playlists/{playlistId}` | One playlist |
| DELETE | `/api/playlists/{playlistId}` | Unfollow it (Spotify has no real delete) |

## Project structure

```
Spotify-background/
├── src/
│   ├── Domain/           # Entities (SpotifyUser, SpotifyToken, OAuthState) and settings models
│   ├── Application/      # Interfaces, repository contracts, TokenService
│   ├── Infrastructure/   # Cosmos persistence (CosmosStore), Spotify client, PKCE, token protection
│   └── API/              # Controllers, Program.cs, cookie auth, Dockerfile
├── web/                  # Next.js frontend
│   └── src/
│       ├── app/          # Routes; (signed-in)/ holds the pages that need a session
│       ├── features/     # auth, playlists, turntable
│       ├── lib/          # api.ts (fetch wrapper), security.ts, features.ts (feature flags)
│       └── services/     # One function per API endpoint
└── Tests/                # Application, Infrastructure and Domain unit tests
```

See `CLAUDE.md` for the architecture and the reasoning behind the auth design.

## Roadmap

- [x] Spotify OAuth with server-side sessions
- [x] Profile, top artists and tracks, now playing, playlists
- [x] Cosmos DB persistence
- [ ] Deploy to Azure (Container Apps + Cosmos free tier), in progress
- [ ] Playback controls and playlist tracks (UI is built and switched off in `web/src/lib/features.ts`)
- [ ] AI playlist generation with Claude
