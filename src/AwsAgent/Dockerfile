FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
EXPOSE 5000 50001
USER app
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
ENV SERVICEOPTION__WORKERINTERVAL=5

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/AwsAgent/AwsAgent.csproj", "AwsAgent/"]
COPY ["src/ResourceContracts/ResourceContracts.csproj", "ResourceContracts/"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/"]
RUN dotnet restore "AwsAgent/AwsAgent.csproj"
COPY ["src/AwsAgent", "AwsAgent/"]
COPY ["src/ResourceContracts", "ResourceContracts/"]
COPY ["src/ServiceDefaults", "ServiceDefaults/"]
WORKDIR "/src/AwsAgent/"
RUN dotnet build "AwsAgent.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AwsAgent.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AwsAgent.dll"]