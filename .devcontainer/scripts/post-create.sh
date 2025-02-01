#!/usr/bin/env bash

set -e

dotnet workload update --include-previews

# k3s kubectl get nodes

# k3d cluster create mycluster

# TODO: ❌  could not connect to docker. docker may not be installed or running
# dapr init
# dapr uninstall

exit