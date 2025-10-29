ARG BASE_IMAGE=mcr.microsoft.com/dotnet/aspnet:6.0
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY MAP2ADAMOINT.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM $BASE_IMAGE AS base
USER root

# Install font packages for Oracle client and curl for healthcheck
RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu-core \
    curl \
    && rm -rf /var/lib/apt/lists/* \
    && fc-cache -fv

WORKDIR /app
COPY --from=build /app/publish .

# Copy Docker-specific configuration (uses host.docker.internal for localhost databases)
COPY appsettings.Docker.json ./appsettings.json

EXPOSE 8085

ENTRYPOINT ["dotnet", "MAP2ADAMOINT.dll"]

