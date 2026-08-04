# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base

RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV TZ=America/Costa_Rica

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Dhole.Reports.Workers/Dhole.Reports.Workers.csproj", "src/Dhole.Reports.Workers/"]
COPY ["src/Dhole.Reports.Application/Dhole.Reports.Application.csproj", "src/Dhole.Reports.Application/"]
COPY ["src/Dhole.Reports.Contracts/Dhole.Reports.Contracts.csproj", "src/Dhole.Reports.Contracts/"]
COPY ["src/Dhole.Reports.Domain/Dhole.Reports.Domain.csproj", "src/Dhole.Reports.Domain/"]
COPY ["src/Dhole.Reports.Infrastructure/Dhole.Reports.Infrastructure.csproj", "src/Dhole.Reports.Infrastructure/"]
COPY ["src/Dhole.Reports.Persistence/Dhole.Reports.Persistence.csproj", "src/Dhole.Reports.Persistence/"]

RUN dotnet restore "src/Dhole.Reports.Workers/Dhole.Reports.Workers.csproj"

COPY . .

WORKDIR "/src/src/Dhole.Reports.Workers"
RUN dotnet build "Dhole.Reports.Workers.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/build \
    --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/Dhole.Reports.Workers"

RUN dotnet publish "Dhole.Reports.Workers.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Dhole.Reports.Workers.dll"]
