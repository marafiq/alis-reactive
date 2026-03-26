# pack-local.ps1 — Build Alis.Reactive NuGet packages into the local feed
# Called automatically by the smoke test MSBuild BeforeBuild target.
# Can also be run manually: powershell -File pack-local.ps1

param(
    [string]$Configuration = "Release",
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

# Resolve paths relative to this script (which lives in the smoke test folder)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Resolve-Path (Join-Path $scriptDir "..\..")

if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "nupkgs"
}

# Library projects to pack (order matters — core first)
$projects = @(
    "Alis.Reactive\Alis.Reactive.csproj",
    "Alis.Reactive.Native\Alis.Reactive.Native.csproj",
    "Alis.Reactive.Fusion\Alis.Reactive.Fusion.csproj",
    "Alis.Reactive.FluentValidator\Alis.Reactive.FluentValidator.csproj"
)

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Pack each project
foreach ($proj in $projects) {
    $fullPath = Join-Path $repoRoot $proj
    if (-not (Test-Path $fullPath)) {
        Write-Warning "Skipping $proj — file not found at $fullPath"
        continue
    }

    Write-Host "Packing $proj -> $OutputDir" -ForegroundColor Cyan
    dotnet pack $fullPath -c $Configuration -o $OutputDir --no-build 2>$null

    if ($LASTEXITCODE -ne 0) {
        Write-Host "  --no-build failed, building and packing..." -ForegroundColor Yellow
        dotnet pack $fullPath -c $Configuration -o $OutputDir
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to pack $proj"
            exit 1
        }
    }
}

Write-Host ""
Write-Host "Local NuGet packages ready at: $OutputDir" -ForegroundColor Green
Get-ChildItem $OutputDir -Filter "*.nupkg" | ForEach-Object { Write-Host "  $_" }
