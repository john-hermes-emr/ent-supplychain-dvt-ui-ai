#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base

RUN apt-get update && \
        apt-get install -y --no-install-recommends \        
        telnet \
        dnsutils \
        curl \
        postgresql-client \
        iputils-ping \
        && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src

COPY ["DVT.Api/DVT.Api.csproj", "DVT.Api/"]
COPY ["DVT.Core/DVT.Core.csproj", "DVT.Core/"]
COPY ["DVT.Data/DVT.Data.csproj", "DVT.Data/"]

RUN dotnet restore "DVT.Api/DVT.Api.csproj"
COPY . .
WORKDIR "/src/DVT.Api"
RUN dotnet build "DVT.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DVT.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DVT.Api.dll"]