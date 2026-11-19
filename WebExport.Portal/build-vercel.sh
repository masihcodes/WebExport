#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

if ! command -v dotnet >/dev/null || [[ "$(dotnet --version)" != 10.* ]]; then
    sdk_dir="$(mktemp -d)"
    trap 'rm -rf -- "$sdk_dir"' EXIT
    curl --fail --silent --show-error --location \
        https://dot.net/v1/dotnet-install.sh --output "$sdk_dir/install.sh"
    bash "$sdk_dir/install.sh" --channel 10.0 --install-dir "$sdk_dir"
    export DOTNET_ROOT="$sdk_dir"
    export PATH="$sdk_dir:$PATH"
fi

dotnet publish WebExport.Portal/WebExport.Portal.csproj --configuration Release --output publish
test -s publish/wwwroot/index.html
test -s publish/wwwroot/o.css
