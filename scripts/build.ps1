# Build script: frontend (pnpm) + backend publish (dotnet)
# Usage:
#   powershell -File scripts/build.ps1               # framework-dependent publish -> out/publish
#   powershell -File scripts/build.ps1 -SelfContained # self-contained publish
param(
    [switch]$SelfContained
)

$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

Write-Host '==> [1/2] Build frontend (pnpm)'
Push-Location (Join-Path $Root 'frontend')
try {
    pnpm install --frozen-lockfile
    if ($LASTEXITCODE -ne 0) { throw 'pnpm install failed' }
    pnpm run build
    if ($LASTEXITCODE -ne 0) { throw 'frontend build failed' }
}
finally {
    Pop-Location
}

Write-Host '==> [2/2] Publish backend (dotnet)'
$OutDir = Join-Path $Root 'out\publish'
$ScArgs = if ($SelfContained) { '--self-contained', 'true' } else { '--self-contained', 'false' }
dotnet publish (Join-Path $Root 'MeetStar-Launcher.slnx') -c Release -r win-x64 @ScArgs -o $OutDir
if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed' }

Write-Host ""
Write-Host "Done. Output: $OutDir\MeetStar-Launcher.Backend.exe"
