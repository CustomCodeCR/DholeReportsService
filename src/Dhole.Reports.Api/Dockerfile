# syntax=docker/dockerfile:1

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        tzdata \
        curl \
        chromium \
        weasyprint \
        fonts-dejavu-core \
        fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 5208

ENV TZ=America/Costa_Rica \
    ASPNETCORE_URLS=http://+:5208 \
    Reports__Generation__ChromiumExecutablePath=/usr/bin/chromium \
    Reports__Generation__WeasyPrintExecutablePath=/usr/bin/weasyprint

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_ACTOR=github-actions
WORKDIR /src

COPY ["NuGet.Config", "./NuGet.Config"]
COPY ["src/Dhole.Reports.Api/Dhole.Reports.Api.csproj", "src/Dhole.Reports.Api/"]
COPY ["src/Dhole.Reports.Application/Dhole.Reports.Application.csproj", "src/Dhole.Reports.Application/"]
COPY ["src/Dhole.Reports.Contracts/Dhole.Reports.Contracts.csproj", "src/Dhole.Reports.Contracts/"]
COPY ["src/Dhole.Reports.Domain/Dhole.Reports.Domain.csproj", "src/Dhole.Reports.Domain/"]
COPY ["src/Dhole.Reports.Infrastructure/Dhole.Reports.Infrastructure.csproj", "src/Dhole.Reports.Infrastructure/"]
COPY ["src/Dhole.Reports.Persistence/Dhole.Reports.Persistence.csproj", "src/Dhole.Reports.Persistence/"]

RUN --mount=type=secret,id=github_token,required=true \
    export NuGetPackageSourceCredentials_github="Username=${GITHUB_ACTOR};Password=$(cat /run/secrets/github_token);ValidAuthenticationTypes=Basic" && \
    dotnet restore "src/Dhole.Reports.Api/Dhole.Reports.Api.csproj" --configfile /src/NuGet.Config

COPY . .

WORKDIR "/src/src/Dhole.Reports.Api"
RUN dotnet build "Dhole.Reports.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/build \
    --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/Dhole.Reports.Api"

RUN dotnet publish "Dhole.Reports.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl --fail --silent http://localhost:5208/health || exit 1

ENTRYPOINT ["dotnet", "Dhole.Reports.Api.dll"]
