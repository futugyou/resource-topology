#!/usr/bin/env bash

set -e

# curl -sfL https://get.k3s.io | sh -

# curl -s https://raw.githubusercontent.com/rancher/k3d/main/install.sh | bash

# wget -q https://raw.githubusercontent.com/dapr/cli/master/install/install.sh -O - | /bin/bash -s 1.14.1

# dotnet workload update
# dotnet dev-certs https

npm install -g @angular/cli@latest
npm install -g markdownlint-cli

# The container creation script is executed in a new Bash instance
# so we exit at the end to avoid the creation process lingering.
exit