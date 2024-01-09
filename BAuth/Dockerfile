ARG SDK_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
ARG PUBLISH_CONFIGURATION=Release
ARG PUBLISH_FRAMEWORK=net8.0
WORKDIR /app
COPY . .
RUN dotnet publish ./BAuth.csproj -c "${PUBLISH_CONFIGURATION}" -f "${PUBLISH_FRAMEWORK}" -o /out -p:IntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:${SDK_VERSION} AS runtime
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /out ./
ENTRYPOINT ["dotnet", "BAuth.dll"]
