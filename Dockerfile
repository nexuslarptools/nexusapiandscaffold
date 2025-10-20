#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["NEXUSDataLayerScaffold.csproj", "."]
RUN dotnet restore -a $TARGETARCH "./NEXUSDataLayerScaffold.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NEXUSDataLayerScaffold.csproj" -c Release -o /app/build

FROM build AS publish
ARG TARGETARCH
RUN dotnet publish "NEXUSDataLayerScaffold.csproj" -c Release -a $TARGETARCH -o /app/publish

FROM base AS final
ARG OTEL_VERSION=1.12.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y curl unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh && \
    chmod +x /otel-dotnet-auto/instrument.sh && \
    useradd -m -s /bin/bash appuser

WORKDIR /app
COPY --from=publish /app/publish .
# Ensure non-root user can access app files and otel agent
RUN chown -R appuser:appuser /app /otel-dotnet-auto

# Run as non-root user
USER appuser

ENV OTEL_DOTNET_AUTO_LOGS_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_DOTNET_AUTO_METRICS_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_DOTNET_AUTO_TRACES_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_SERVICE_NAME="nexusapi"
ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"

# Healthcheck for container
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["/otel-dotnet-auto/instrument.sh", "dotnet", "NEXUSDataLayerScaffold.dll"]
#ENTRYPOINT ["dotnet", "NEXUSDataLayerScaffold.dll"]