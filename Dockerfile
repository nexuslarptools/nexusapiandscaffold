#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 22

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["NEXUSDataLayerScaffold.csproj", "."]
RUN dotnet restore -a $TARGETARCH "./NEXUSDataLayerScaffold.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "NEXUSDataLayerScaffold.csproj" -c Debug -o /app/build

FROM build AS publish
ARG TARGETARCH
RUN dotnet publish "NEXUSDataLayerScaffold.csproj" -c Debug -a $TARGETARCH -o /app/publish

FROM base AS final
ARG OTEL_VERSION=1.12.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y --no-install-recommends openssh-server curl unzip && \
    rm -rf /var/lib/apt/lists/* && \
    mkdir /var/run/sshd && \
    echo 'root:password123!' | chpasswd && \
    sed -i 's/^#*PermitRootLogin .*/PermitRootLogin yes/' /etc/ssh/sshd_config && \
    sed -i 's/^#*PasswordAuthentication .*/PasswordAuthentication yes/' /etc/ssh/sshd_config && \
    echo "PermitUserEnvironment yes" >> /etc/ssh/sshd_config && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh && \
    chmod +x /otel-dotnet-auto/instrument.sh && \
    useradd -m -s /bin/bash appuser | \
    curl -sSL https://aka.ms/getvsdbgsh | \
    bash /dev/stdin -v latest -l /vsdbg && \
    chmod -R 755 /vsdbg && \
    chmod +x /vsdbg/vsdbg && \
    mkdir -p /root/.ssh && chmod 700 /root/.ssh

WORKDIR /app
COPY --from=publish /app/publish .
# Ensure non-root user can access app files and otel agent
RUN chown -R appuser:appuser /app /otel-dotnet-auto /vsdbg
RUN /usr/sbin/sshd


# Run as non-root user
USER appuser

ENV OTEL_DOTNET_AUTO_LOGS_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_DOTNET_AUTO_METRICS_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_DOTNET_AUTO_TRACES_CONSOLE_EXPORTER_ENABLED="true"
ENV OTEL_SERVICE_NAME="nexusapi"
ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"

ENTRYPOINT ["/otel-dotnet-auto/instrument.sh", "dotnet", "NEXUSDataLayerScaffold.dll", "--wait-for-debugger"]
#ENTRYPOINT ["dotnet", "NEXUSDataLayerScaffold.dll"]