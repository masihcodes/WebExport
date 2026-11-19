#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

: "${PORTAL_API_URL:?Set PORTAL_API_URL in the Vercel project settings.}"

node <<'NODE'
const { writeFileSync } = require('node:fs');

const message = 'PORTAL_API_URL must be a public HTTPS origin, for example https://api.example.com/.';
if (!URL.canParse(process.env.PORTAL_API_URL)) {
    throw new Error(message);
}

const apiUrl = new URL(process.env.PORTAL_API_URL);
if (apiUrl.protocol !== 'https:' || apiUrl.username || apiUrl.password || apiUrl.pathname !== '/' || apiUrl.search || apiUrl.hash) {
    throw new Error(message);
}

writeFileSync('WebExport.Portal/wwwroot/appsettings.Production.json', JSON.stringify({ ApiUrl: apiUrl.href }) + '\n');
NODE

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
test -s publish/wwwroot/appsettings.Production.json
