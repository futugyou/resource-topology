FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR "/AwsAgent"
COPY ["src/AwsAgent/AwsAgent.csproj", "./"]
RUN dotnet restore "AwsAgent.csproj"
COPY ["src/AwsAgent", "./"]
RUN dotnet build "AwsAgent.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "AwsAgent.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AwsAgent.dll"]