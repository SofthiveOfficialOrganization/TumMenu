[CmdletBinding()]
param(
    [string] $ConnectionString = "Data Source=pleskserver.hostingdunyam.net;User Id=softhive;Password=zez02102025__;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Database=tummenudb",
    [string] $StartupProjectPath,
    [string] $Configuration = "Release",
    [switch] $DryRun,
    [switch] $NoPause
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

trap {
    Write-Host "`n=== ERROR ===" -ForegroundColor Red
    Write-Host ($_ | Out-String) -ForegroundColor Red
    if (-not $NoPause) { Read-Host "`nPress Enter to close..." | Out-Null }
    exit 1
}

if ([string]::IsNullOrWhiteSpace($StartupProjectPath)) {
    $StartupProjectPath = Join-Path $PSScriptRoot "..\WebUI\WebUI.csproj"
}

$StartupProjectPath = [System.IO.Path]::GetFullPath($StartupProjectPath)

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "ConnectionString bos olamaz."
}

if (!(Test-Path $StartupProjectPath)) {
    throw "Startup project bulunamadi: $StartupProjectPath"
}

$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$previousConnection = $env:ConnectionStrings__DefaultConnection

try {
    $env:ASPNETCORE_ENVIRONMENT = "Production"
    $env:ConnectionStrings__DefaultConnection = $ConnectionString

    Write-Host "Markdown blog seed baslatiliyor..." -ForegroundColor Cyan
    Write-Host "Environment: Production" -ForegroundColor DarkGray
    Write-Host "Connection: script parametresi -> ConnectionStrings__DefaultConnection" -ForegroundColor DarkGray

    if ($DryRun) {
        Write-Host "Dry run tamam: connection string ve proje yolu okunabiliyor. DB'ye yazilmadi." -ForegroundColor Yellow
        return
    }

    dotnet run `
        --project $StartupProjectPath `
        --configuration $Configuration `
        --no-launch-profile `
        -- `
        --seed-only

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet run seed-only failed with exit code $LASTEXITCODE"
    }

    Write-Host "Markdown blog seed tamamlandi." -ForegroundColor Green
}
finally {
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    $env:ConnectionStrings__DefaultConnection = $previousConnection
}

if (-not $NoPause) { Read-Host "`nPress Enter to close..." | Out-Null }
