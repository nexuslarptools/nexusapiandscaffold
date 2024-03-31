#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["NEXUSDataLayerScaffold.csproj", "."]
RUN dotnet restore "./NEXUSDataLayerScaffold.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NEXUSDataLayerScaffold.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NEXUSDataLayerScaffold.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG OTEL_VERSION=1.4.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y curl unzip &&     OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh &&     chmod +x /otel-dotnet-auto/instrument.sh

WORKDIR /app
COPY --from=publish /app/publish .
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://prometheus-agent-agent-1:4318
ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"
ENTRYPOINT ["/otel-dotnet-auto/instrument.sh", "dotnet", "NEXUSDataLayerScaffold.dll"]
#ENTRYPOINT ["dotnet", "NEXUSDataLayerScaffold.dll"]