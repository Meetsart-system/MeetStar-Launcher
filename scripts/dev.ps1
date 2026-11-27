# Dev helper: start Vite dev server (new console window) + run backend in debug mode
$ErrorActionPreference = 'Stop'
$Root = Split-Path -Parent $PSScriptRoot

Write-Host '==> Starting Vite dev server (new window, http://localhost:5173)'
Start-Process cmd -ArgumentList '/k', 'pnpm dev' -WorkingDirectory (Join-Path $Root 'frontend')

Write-Host '==> Running backend (Debug, navigates to Vite dev server)'
dotnet run --project (Join-Path $Root 'MeetStar-Launcher.Backend')
