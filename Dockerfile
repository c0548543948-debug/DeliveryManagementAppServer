# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level config (TargetFramework + package versions defined here)
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY global.json .

# Copy all csproj files and restore (layer cache)
COPY src/Domain/Domain.csproj                     src/Domain/
COPY src/Application/Application.csproj           src/Application/
COPY src/Infrastructure/Infrastructure.csproj     src/Infrastructure/
COPY src/ServiceDefaults/ServiceDefaults.csproj   src/ServiceDefaults/
COPY src/Shared/Shared.csproj                     src/Shared/
COPY src/Web/Web.csproj                           src/Web/

RUN dotnet restore src/Web/Web.csproj

# Copy the rest of the source
COPY src/ src/

# Publish
RUN dotnet publish src/Web/Web.csproj \
    --no-restore \
    -c Release \
    -o /app/publish \
    /p:SkipSpaPublish=true

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Port the app listens on
EXPOSE 8080

ENTRYPOINT ["dotnet", "DeliveryManagementApp.Web.dll"]
