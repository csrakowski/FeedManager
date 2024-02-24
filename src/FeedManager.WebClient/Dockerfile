#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM csrakowski/aspnet:8-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443



FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["global.json", ""]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/FeedManager.WebClient/FeedManager.WebClient.csproj", "src/FeedManager.WebClient/"]
COPY ["src/FeedManager.Abstractions/FeedManager.Abstractions.csproj", "src/FeedManager.Abstractions/"]
COPY ["src/FeedManager.Shared/FeedManager.Shared.csproj", "src/FeedManager.Shared/"]
RUN dotnet restore "src/FeedManager.WebClient/FeedManager.WebClient.csproj"
COPY . .
WORKDIR "/src/src/FeedManager.WebClient"
RUN dotnet build "FeedManager.WebClient.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "FeedManager.WebClient.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
LABEL org.opencontainers.image.source=https://github.com/csrakowski/FeedManager
LABEL org.opencontainers.image.authors="https://github.com/csrakowski/"
LABEL org.opencontainers.image.description="FeedManager WebClient"
LABEL org.opencontainers.image.licenses=MIT
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeedManager.WebClient.dll"]
