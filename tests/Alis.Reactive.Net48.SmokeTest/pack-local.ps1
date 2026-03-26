# pack-local.ps1 - Build Alis.Reactive NuGet packages into the local feed
# Called automatically by the smoke test MSBuild BeforeBuild target.
# Can also be run manually: powershell -ExecutionPolicy Bypass -File pack-local.ps1

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

# Resolve paths relative to this script (lives in tests/Alis.Reactive.Net48.SmokeTest/)
$scriptDir = $PSScriptRoot
if (-not $scriptDir) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
}
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..\..")).Path

if ([string]::IsNullOrEmpty($OutputDir)) {
    $OutputDir = Join-Path $repoRoot "nupkgs"
}

Write-Host "Repo root: $repoRoot"
Write-Host "Output dir: $OutputDir"
Write-Host "Configuration: $Configuration"
Write-Host ""

# Library projects to pack (order matters - core first)
$projects = @(
    "Alis.Reactive\Alis.Reactive.csproj",
    "Alis.Reactive.Native\Alis.Reactive.Native.csproj",
    "Alis.Reactive.Fusion\Alis.Reactive.Fusion.csproj",
    "Alis.Reactive.FluentValidator\Alis.Reactive.FluentValidator.csproj"
)

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Write-Host "Created $OutputDir"
}

# Pack each project
foreach ($proj in $projects) {
    $fullPath = Join-Path $repoRoot $proj
    if (-not (Test-Path $fullPath)) {
        Write-Warning "Skipping $proj - file not found at $fullPath"
        continue
    }

    Write-Host "Packing $proj ..." -ForegroundColor Cyan

    # Try --no-build first (fast if already built), fall back to full build+pack
    $output = & dotnet pack $fullPath -c $Configuration -o $OutputDir --no-build 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  --no-build failed, building and packing..." -ForegroundColor Yellow
        & dotnet pack $fullPath -c $Configuration -o $OutputDir
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to pack $proj"
            exit 1
        }
    }
    Write-Host "  OK" -ForegroundColor Green
}

Write-Host ""
Write-Host "Local NuGet packages ready at: $OutputDir" -ForegroundColor Green
Get-ChildItem $OutputDir -Filter "*.nupkg" | ForEach-Object { Write-Host "  $($_.Name)" }
