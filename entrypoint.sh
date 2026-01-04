#!/bin/bash
echo "Starting SSH server..."
sudo /usr/sbin/sshd

# Optional: Wait a few seconds for services to initialize.
sleep 2

echo "Starting TestRemoteDebug application..."
/otel-dotnet-auto/instrument.sh dotnet NEXUSDataLayerScaffold.dll --wait-for-debugger