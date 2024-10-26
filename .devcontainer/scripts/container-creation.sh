#!/usr/bin/env bash

set -e

dotnet workload update
dotnet dev-certs https

npm install -g @angular/cli@latest

# The container creation script is executed in a new Bash instance
# so we exit at the end to avoid the creation process lingering.
exit