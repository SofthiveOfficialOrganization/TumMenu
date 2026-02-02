# Update-DevDb.ps1
[CmdletBinding()]
param(
  [string] $Server = "(localdb)\MSSQLLocalDB",
  [string] $Database = "TumMenuDb_Dev",
  [string] $Context = "ApplicationDbContext",
  [string] $Configuration = "Debug",
  [string] $ProjectPath = (Join-Path $PSScriptRoot "Infrastructure\Infrastructure.csproj"),
  [string] $StartupProjectPath = (Join-Path $PSScriptRoot "WebUI\WebUI.csproj"),
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

if (!(Test-Path $ProjectPath)) { throw "EF Project not found: $ProjectPath" }
if (!(Test-Path $StartupProjectPath)) { throw "Startup Project not found: $StartupProjectPath" }

$connection = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True"

$args = @(
  "ef", "database", "update",
  "--project", $ProjectPath,
  "--startup-project", $StartupProjectPath,
  "--context", $Context,
  "--configuration", $Configuration,
  "--connection", $connection
)

Write-Host "Updating DEV DB: $Database" -ForegroundColor Cyan
Write-Host "dotnet $($args -join ' ')" -ForegroundColor DarkGray

dotnet @args
if ($LASTEXITCODE -ne 0) { throw "dotnet ef failed with exit code $LASTEXITCODE" }

Write-Host "Done." -ForegroundColor Green
if (-not $NoPause) { Read-Host "`nPress Enter to close..." | Out-Null }
