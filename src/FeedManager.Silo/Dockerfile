#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base stage: Pull in a base image and do some basic setup (VS Debugging uses this base image as well)
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
#ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_JitNoMemoryBarriers=1
ENV DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP3SUPPORT=1
RUN apk update
RUN apk upgrade


# Build stage: Gather all source files needed, and run dotnet build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/FeedManager.Silo/FeedManager.Silo.csproj", "src/FeedManager.Silo/"]
COPY ["src/FeedManager.Abstractions/FeedManager.Abstractions.csproj", "src/FeedManager.Abstractions/"]
COPY ["src/FeedManager.Grains/FeedManager.Grains.csproj", "src/FeedManager.Grains/"]
COPY ["Directory.Packages.props", ""]
COPY ["Directory.Build.props", ""]
COPY ["global.json", ""]
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