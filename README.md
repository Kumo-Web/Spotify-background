# Spotify AI Music Analysis Platform

A full-stack application that integrates with Spotify to provide AI-powered music analysis and intelligent playlist generation. Users can authenticate with their Spotify accounts, analyze their listening habits, and create personalized playlists using natural language prompts powered by Anthropic Claude.

## 🎵 Features

### Current Features
- **Spotify OAuth Authentication** - Secure OAuth 2.0 integration with automatic token refresh
- **User Profile Management** - Access and display Spotify user information
- **Music Data Collection**
  - Retrieve top tracks and artists (short-term, medium-term, long-term)
  - Access recently played tracks
  - View followed artists
  - Get saved/liked tracks
- **Audio Feature Analysis** - Extract detailed audio characteristics (energy, danceability, valence, tempo, etc.)
- **Playlist Management** - Create and manage Spotify playlists programmatically

### 🚧 In Development
- **AI-Powered Playlist Generation** - Natural language playlist creation using Anthropic Claude
- **Music Preference Analysis** - Intelligent analysis of listening patterns and preferences
- **Smart Recommendations** - AI-driven track recommendations based on user taste
- **Playlist History Tracking** - View and manage AI-generated playlists

## 🛠️ Tech Stack

### Backend
- **Framework:** ASP.NET Core 9.0 (C#)
- **Architecture:** Clean Architecture (DDD)
  - Domain Layer - Entities and business rules
  - Application Layer - Use cases and interfaces
  - Infrastructure Layer - External integrations
  - API Layer - REST endpoints
- **Database:** SQL Server / SQLite with Entity Framework Core 9.0
- **Authentication:** OAuth 2.0 via Spotify
- **API Integration:** SpotifyAPI.Web v7.2.1
- **AI Integration:** Anthropic Claude API (claude-sonnet-4-5)
- **Observability:** OpenTelemetry, Sentry

### Frontend
- **Framework:** Next.js 15.1.2 (App Router)
- **Language:** TypeScript
- **UI Library:** React 19
- **Styling:** Tailwind CSS 3.4.1
- **Icons:** React Icons 5.4.0

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm
- [SQL Server](https://www.microsoft.com/sql-server) or SQLite
- [Spotify Developer Account](https://developer.spotify.com/)
- [Anthropic API Key](https://console.anthropic.com/) (for AI features)
- [ngrok](https://ngrok.com/) (for local development with OAuth)

## 🚀 Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd Spotify-background
```

### 2. Spotify App Configuration

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Create a new app
3. Note your `Client ID` and `Client Secret`
4. Add redirect URI: `https://your-ngrok-url.ngrok-free.app/api/spotifyAuth/callback`

### 3. Backend Setup

#### Configure Application Settings
```bash
cd src/API
```

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SpotifyDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "SpotifySettings": {
    "ClientId": "YOUR_SPOTIFY_CLIENT_ID",
    "ClientSecret": "YOUR_SPOTIFY_CLIENT_SECRET",
    "RedirectUrl": "https://your-ngrok-url.ngrok-free.app/api/spotifyAuth/callback",
    "Scopes": "user-read-private user-read-email user-top-read user-library-read user-follow-read user-read-recently-played playlist-read-private playlist-modify-public playlist-modify-private"
  },
  "AnthropicSettings": {
    "ApiKey": "",
    "Model": "claude-sonnet-4-5-20250929",
    "MaxTokens": 4096,
    "Temperature": 1.0
  }
}
```

#### Store Secrets Securely
```bash
dotnet user-secrets init
dotnet user-secrets set "SpotifySettings:ClientSecret" "YOUR_SPOTIFY_CLIENT_SECRET"
dotnet user-secrets set "AnthropicSettings:ApiKey" "YOUR_ANTHROPIC_API_KEY"
```

#### Install Dependencies & Run Migrations
```bash
# Restore NuGet packages
dotnet restore

# Create database migration
dotnet ef migrations add InitialMigration -p ../Infrastructure -s . -c Infrastructure.DatabaseContext.SpotifyDbContext

# Apply migration to database
dotnet ef database update -p ../Infrastructure -s .
```

### 4. Frontend Setup

```bash
cd src/API/UI

# Install dependencies
npm install

# Configure environment variables
cp .env.example .env.local
```

Edit `.env.local`:
```bash
NEXT_PUBLIC_API_URL=https://localhost:7249
```

### 5. Setup ngrok for Local Development

In a separate terminal:
```bash
ngrok http https://localhost:7249
```

Copy the HTTPS URL (e.g., `https://abc123.ngrok-free.app`) and update:
- Spotify Developer Dashboard redirect URI
- `appsettings.Development.json` → `SpotifySettings:RedirectUrl`

## ▶️ Running the Application

### Start Backend (Terminal 1)
```bash
cd src/API
dotnet run
```
Backend will run on `https://localhost:7249`

### Start Frontend (Terminal 2)
```bash
cd src/API/UI
npm run dev
```
Frontend will run on `http://localhost:3000`

### Start ngrok (Terminal 3)
```bash
ngrok http https://localhost:7249
```

### Access the Application
1. Open browser to `http://localhost:3000`
2. Click "Login with Spotify"
3. Authorize the application
4. You'll be redirected to the dashboard

## 📁 Project Structure

```
Spotify-background/
├── src/
│   ├── API/                        # Web API & Frontend Host
│   │   ├── Controllers/            # REST API endpoints
│   │   ├── UI/                     # Next.js frontend application
│   │   │   ├── src/
│   │   │   │   ├── app/            # Next.js pages (App Router)
│   │   │   │   ├── components/     # React components
│   │   │   │   ├── hooks/          # Custom React hooks
│   │   │   │   ├── services/       # API service layer
│   │   │   │   └── types/          # TypeScript type definitions
│   │   ├── Program.cs              # Application startup
│   │   └── appsettings.json        # Configuration
│   │
│   ├── Application/                # Application layer (use cases)
│   │   ├── Interfaces/             # Service contracts
│   │   └── Services/               # Application services
│   │
│   ├── Domain/                     # Domain layer (entities)
│   │   ├── Entities/               # Domain entities
│   │   └── SpotifySettings.cs      # Configuration models
│   │
│   └── Infrastructure/             # Infrastructure layer
│       ├── DatabaseContext/        # EF Core DbContext
│       ├── Interfaces/             # Infrastructure contracts
│       ├── Migrations/             # Database migrations
│       ├── Repositories/           # Data access
│       ├── Services/               # External service integrations
│       └── InfrastructureDI.cs     # Dependency injection setup
│
└── Tests/                          # Test projects
```

## 🔌 API Endpoints

### Authentication
- `POST /api/spotifyauth/login` - Initiate Spotify OAuth flow
- `GET /api/spotifyauth/callback` - OAuth callback handler

### User Data
- `GET /api/spotifyuser/getUserInfo/{userId}` - Get user profile information

### AI Playlists (Coming Soon)
- `POST /api/AIPlaylist/generate` - Generate AI-powered playlist
- `GET /api/AIPlaylist/status/{requestId}` - Check generation status
- `GET /api/AIPlaylist/history/{userId}` - Get playlist generation history

## 🗄️ Database Migrations

### Create a New Migration
```bash
dotnet ef migrations add MigrationName \
  -p src/Infrastructure \
  -s src/API \
  -c Infrastructure.DatabaseContext.SpotifyDbContext
```

### Apply Migrations
```bash
dotnet ef database update -p src/Infrastructure -s src/API
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName -p src/Infrastructure -s src/API
```

## 🔧 Development Workflow

### Backend Development
1. Make changes to C# files
2. Application auto-reloads with hot reload
3. Test endpoints using browser or Postman

### Frontend Development
1. Edit React/TypeScript files in `src/API/UI/src`
2. Changes hot-reload automatically
3. View in browser at `http://localhost:3000`

### Database Changes
1. Modify entities in `src/Domain/Entities/`
2. Update DbContext if needed
3. Create and apply migration (see Database Migrations section)

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/YourTestProject
```

## 📝 Configuration

### Required Spotify OAuth Scopes
- `user-read-private` - Read user profile
- `user-read-email` - Read user email
- `user-top-read` - Read top tracks/artists
- `user-library-read` - Read saved tracks
- `user-follow-read` - Read followed artists
- `user-read-recently-played` - Read recently played tracks
- `playlist-read-private` - Read private playlists
- `playlist-modify-public` - Modify public playlists
- `playlist-modify-private` - Modify private playlists

### Environment Variables

**Backend** (User Secrets):
- `SpotifySettings:ClientSecret` - Spotify app secret
- `AnthropicSettings:ApiKey` - Anthropic API key

**Frontend** (.env.local):
- `NEXT_PUBLIC_API_URL` - Backend API URL

## 🏗️ Architecture Overview

This application follows **Clean Architecture** principles:

### Dependency Flow
```
API Layer → Application Layer → Domain Layer
    ↓
Infrastructure Layer → Domain Layer
```

### Key Design Patterns
- **Repository Pattern** - Data access abstraction
- **Factory Pattern** - Spotify client creation
- **Dependency Injection** - Loose coupling
- **Options Pattern** - Configuration management
- **Service Layer** - Business logic encapsulation

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- [SpotifyAPI.Web](https://github.com/JohnnyCrazy/SpotifyAPI-NET) - Spotify API wrapper
- [Anthropic](https://www.anthropic.com/) - AI-powered playlist generation
- [Next.js](https://nextjs.org/) - React framework
- [Tailwind CSS](https://tailwindcss.com/) - Styling framework

## 📧 Support

For issues and questions:
- Open an issue on GitHub
- Check existing documentation
- Review the implementation plan in `.claude/plans/`

## 🗺️ Roadmap

- [x] Spotify OAuth authentication
- [x] User data retrieval (top tracks, artists, saved tracks)
- [x] Audio feature extraction
- [x] Playlist creation and management
- [ ] AI-powered playlist generation with Claude
- [ ] Music preference analysis
- [ ] Playlist recommendation engine
- [ ] User analytics dashboard
- [ ] Social features (share playlists)
- [ ] Mobile application

---

**Built with ❤️ using ASP.NET Core, Next.js, and Anthropic Claude**
