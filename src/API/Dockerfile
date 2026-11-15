# .NET API image. Build it from the repository ROOT, not from src/API:
#
#   docker build -f src/API/Dockerfile -t spotify-api .
#
# The trailing "." is the build context: the only folder COPY can read from. It has to be the
# root because the build needs Directory.Build.props, Directory.Packages.props and nuget.config,
# which live there rather than in src/API.

# ---------------------------------------------------------------------------------------------
# Stage 1: "build". The full SDK (~800 MB) can compile code. None of this stage ships; only the
# compiled output is copied out of it at the end.
# ---------------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the files that decide WHICH packages are needed, then restore. Docker caches each
# step and reruns it only when its inputs change, so editing a .cs file later skips the slow
# package download entirely. Paths are kept identical to the repo so project references resolve.
COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/API/WebApi.csproj src/API/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
RUN dotnet restore src/API/WebApi.csproj

# Now the actual source code. This layer is invalidated by any code change, but restore above
# stays cached.
COPY src/ src/
RUN dotnet publish src/API/WebApi.csproj -c Release --no-restore -o /app

# ---------------------------------------------------------------------------------------------
# Stage 2: "runtime". The ASP.NET runtime only (~200 MB): it can run the app but not build it.
# This is the image that actually gets deployed.
# ---------------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Take the published output from stage 1. The SDK and the source code are left behind.
COPY --from=build /app .

# The base image already listens on port 8080 and defines a non-root user. Running as root
# would let a compromised app do anything inside the container.
USER $APP_UID
EXPOSE 8080

# What runs when a container starts. If this process exits, the container stops.
ENTRYPOINT ["dotnet", "WebApi.dll"]
