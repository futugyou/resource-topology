FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
ENV SERVICEOPTION__WORKERINTERVAL=5

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/AwsAgent/AwsAgent.csproj", "AwsAgent/"]
COPY ["src/ResourceContracts/ResourceContracts.csproj", "ResourceContracts/"]
RUN dotnet restore "AwsAgent/AwsAgent.csproj"
COPY ["src/AwsAgent", "AwsAgent/"]
COPY ["src/ResourceContracts", "ResourceContracts/"]
WORKDIR "/src/AwsAgent/"
RUN dotnet build "AwsAgent.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AwsAgent.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AwsAgent.dll"]