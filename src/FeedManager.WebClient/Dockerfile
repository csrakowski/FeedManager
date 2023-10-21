#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV DOTNET_JitNoMemoryBarriers=1
ENV DOTNET_SYSTEM_NET_HTTP_SOCKETSHTTPHANDLER_HTTP3SUPPORT=1
RUN apk update
RUN apk upgrade
RUN apk add curl


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["global.json", ""]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/FeedManager.WebClient/FeedManager.WebClient.csproj", "src/FeedManager.WebClient/"]
RUN dotnet restore "src/FeedManager.WebClient/FeedManager.WebClient.csproj"
COPY . .
WORKDIR "/src/src/FeedManager.WebClient"
RUN dotnet build "FeedManager.WebClient.csproj" -c Release -o /app/build


FROM build AS publish
RUN dotnet publish "FeedManager.WebClient.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FeedManager.WebClient.dll"]