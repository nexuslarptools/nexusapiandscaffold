#!/bin/bash
echo "Starting SSH server..."
/usr/sbin/sshd

# Optional: Wait a few seconds for services to initialize.
sleep 3

echo "Starting TestRemoteDebug application..."
/otel-dotnet-auto/instrument.sh dotnet NEXUSDataLayerScaffold.dll --wait-for-debugger