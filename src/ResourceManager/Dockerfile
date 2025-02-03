FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
EXPOSE 5000 50001
USER app
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/ResourceManager/ResourceManager.csproj", "ResourceManager/"]
COPY ["src/ResourceContracts/ResourceContracts.csproj", "ResourceContracts/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
RUN dotnet restore "ResourceManager/ResourceManager.csproj"
COPY ["src/ResourceManager", "ResourceManager/"]
COPY ["src/ResourceContracts", "ResourceContracts/"]
COPY ["src/ServiceDefaults", "ServiceDefaults/"]
WORKDIR "/src/ResourceManager/"
RUN dotnet build "ResourceManager.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "ResourceManager.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResourceManager.dll"]