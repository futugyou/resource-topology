#!/usr/bin/env bash

set -e

dotnet workload update

# k3s kubectl get nodes

# k3d cluster create mycluster

dapr init
 
exit