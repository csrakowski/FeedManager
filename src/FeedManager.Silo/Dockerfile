#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base stage: Pull in a base image and do some basic setup (VS Debugging uses this base image as well)
FROM csrakowski/aspnet:8-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443



# Build stage: Gather all source files needed, and run dotnet build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["global.json", ""]
COPY ["Directory.Packages.props", ""]
COPY ["Directory.Build.props", ""]
COPY ["src/FeedManager.Silo/FeedManager.Silo.csproj", "src/FeedManager.Silo/"]
COPY ["src/FeedManager.Abstractions/FeedManager.Abstractions.csproj", "src/FeedManager.Abstractions/"]
COPY ["src/FeedManager.Grains/FeedManager.Grains.csproj", "src/FeedManager.Grains/"]
COPY ["src/FeedManager.Shared/FeedManager.Shared.csproj", "src/FeedManager.Shared/"]
RUN dotnet restore "src/FeedManager.Silo/FeedManager.Silo.csproj"
COPY . .
WORKDIR "/src/src/FeedManager.Silo"
RUN dotnet build "FeedManager.Silo.csproj" -c Release -o /app/build



# Publish stage: Run a dotnet publish
FROM build AS publish
RUN dotnet publish "FeedManager.Silo.csproj" -c Release -o /app/publish /p:UseAppHost=false


# Final stage: Take the original base image, copy over the published output, and get running.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeedManager.Silo.dll"]