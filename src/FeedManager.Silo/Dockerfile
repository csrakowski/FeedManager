#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/FeedManager.Silo/FeedManager.Silo.csproj", "src/FeedManager.Silo/"]
COPY ["src/FeedManager.Abstractions/FeedManager.Abstractions.csproj", "src/FeedManager.Abstractions/"]
COPY ["src/FeedManager.Grains/FeedManager.Grains.csproj", "src/FeedManager.Grains/"]
RUN dotnet restore "src/FeedManager.Silo/FeedManager.Silo.csproj"
COPY . .
WORKDIR "/src/src/FeedManager.Silo"
RUN dotnet build "FeedManager.Silo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FeedManager.Silo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

#ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_JitNoMemoryBarriers=1
ENV DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP3SUPPORT=1

ENTRYPOINT ["dotnet", "FeedManager.Silo.dll"]