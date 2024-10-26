FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
EXPOSE 5000 
USER app
ENV ASPNETCORE_URLS=http://+:5000
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR "/ResourceManager"
COPY ["src/ResourceManager/ResourceManager.csproj", "./"]
RUN dotnet restore "ResourceManager.csproj"
COPY ["src/ResourceManager", "./"]
RUN dotnet build "ResourceManager.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ResourceManager.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResourceManager.dll"]