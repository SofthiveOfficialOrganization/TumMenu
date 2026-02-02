# Clone-ProdToDevDb.ps1
[CmdletBinding()]
param(
  [string] $Server = "(localdb)\MSSQLLocalDB",
  [string] $SourceDatabase = "TumMenuDb",
  [string] $TargetDatabase = "TumMenuDb_Dev",
  [string] $BackupDir = (Join-Path $PSScriptRoot "db-backups"),
  [switch] $Force,
  [switch] $NoPause
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Pause-IfConsole([string] $message) {
  # Hata durumunda her zaman duracağız; başarıda -NoPause geçerli
  try {
    Read-Host $message | Out-Null
  } catch {
    # Non-interactive host olursa en azından biraz beklet
    Start-Sleep -Seconds 10
  }
}

trap {
  Write-Host "`n=== ERROR ===" -ForegroundColor Red
  Write-Host ($_ | Out-String) -ForegroundColor Red

  # İSTEK: hata alsa bile pencere kapanmasın -> her zaman bekle
  Pause-IfConsole "`nPress Enter to close..."
  exit 1
}

function Escape-SqlLiteral([string] $value) {
  return $value.Replace("'", "''")
}

function New-SqlConnection([string] $databaseName) {
  $connStr = "Server=$Server;Database=$databaseName;Trusted_Connection=True;TrustServerCertificate=True"
  $conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
  $conn.Open()
  return $conn
}

function Invoke-SqlNonQuery([System.Data.SqlClient.SqlConnection] $conn, [string] $sql) {
  $cmd = $conn.CreateCommand()
  $cmd.CommandTimeout = 0
  $cmd.CommandText = $sql
  [void] $cmd.ExecuteNonQuery()
}

function Invoke-SqlQuery([System.Data.SqlClient.SqlConnection] $conn, [string] $sql) {
  $cmd = $conn.CreateCommand()
  $cmd.CommandTimeout = 0
  $cmd.CommandText = $sql

  $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
  $dt = New-Object System.Data.DataTable
  [void] $da.Fill($dt)

  # DataTable enumerable olduğu için PowerShell bazen DataRow'lara "açar".
  Write-Output -NoEnumerate $dt
}

function Invoke-SqlScalar([System.Data.SqlClient.SqlConnection] $conn, [string] $sql) {
  $cmd = $conn.CreateCommand()
  $cmd.CommandTimeout = 0
  $cmd.CommandText = $sql
  return $cmd.ExecuteScalar()
}

function Write-ConnectionInfo([System.Data.SqlClient.SqlConnection] $conn) {
  Write-Host "`nConnection:" -ForegroundColor DarkGray
  Write-Host "  Server param : $Server" -ForegroundColor DarkGray
  Write-Host "  DataSource   : $($conn.DataSource)" -ForegroundColor DarkGray
  Write-Host "  Database     : $($conn.Database)" -ForegroundColor DarkGray

  $info = Invoke-SqlQuery $conn @"
SELECT
  CAST(SERVERPROPERTY('ServerName') AS nvarchar(256))   AS ServerName,
  CAST(SERVERPROPERTY('InstanceName') AS nvarchar(256)) AS InstanceName,
  CAST(SERVERPROPERTY('Edition') AS nvarchar(256))      AS Edition,
  CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(256)) AS ProductVersion,
  ORIGINAL_LOGIN() AS OriginalLogin;
"@
  if ($info.Rows.Count -gt 0) {
    $r = $info.Rows[0]
    Write-Host "  ServerName   : $($r.ServerName)" -ForegroundColor DarkGray
    Write-Host "  InstanceName : $($r.InstanceName)" -ForegroundColor DarkGray
    Write-Host "  Edition      : $($r.Edition)" -ForegroundColor DarkGray
    Write-Host "  Version      : $($r.ProductVersion)" -ForegroundColor DarkGray
    Write-Host "  Login        : $($r.OriginalLogin)" -ForegroundColor DarkGray
  }
}

New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupPath = Join-Path (Resolve-Path $BackupDir).Path "$SourceDatabase-$timestamp.bak"

Write-Host "Cloning '$SourceDatabase' -> '$TargetDatabase'" -ForegroundColor Cyan
Write-Host "Backup file: $backupPath" -ForegroundColor DarkGray

$master = $null
try {
  $master = New-SqlConnection "master"
  Write-ConnectionInfo $master

  $safeTarget = Escape-SqlLiteral $TargetDatabase

  # Debug: Target adına benzeyen DB'leri listele (instance karışıklığını yakalamak için)
  $matches = Invoke-SqlQuery $master "SELECT name FROM sys.databases WHERE name LIKE N'$safeTarget%';"
  Write-Host "`nDatabases matching '$TargetDatabase*':" -ForegroundColor DarkGray
  if ($matches.Rows.Count -eq 0) {
    Write-Host "  (none)" -ForegroundColor DarkGray
  } else {
    foreach ($row in $matches.Rows) {
      Write-Host "  - $($row.name)" -ForegroundColor DarkGray
    }
  }

  # EXISTS check (scalar)
  $exists = [int](Invoke-SqlScalar $master "SELECT COUNT(1) FROM sys.databases WHERE name = N'$safeTarget';")
  Write-Host "`nTarget exists count: $exists" -ForegroundColor DarkGray

  if ($exists -gt 0) {
    if (-not $Force) { throw "Target DB '$TargetDatabase' exists. Use -Force to overwrite." }

    Write-Host "Dropping existing target DB '$TargetDatabase'..." -ForegroundColor Yellow
    Invoke-SqlNonQuery $master @"
ALTER DATABASE [$TargetDatabase] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [$TargetDatabase];
"@
  }

  Write-Host "Backing up '$SourceDatabase'..." -ForegroundColor Cyan
  Invoke-SqlNonQuery $master @"
BACKUP DATABASE [$SourceDatabase]
TO DISK = N'$backupPath'
WITH COPY_ONLY, INIT;
"@

  $fileList = Invoke-SqlQuery $master "RESTORE FILELISTONLY FROM DISK = N'$backupPath';"
  if ($fileList.Rows.Count -lt 2) { throw "Backup seems invalid: $backupPath" }

  $dataRows = $fileList.Select("Type = 'D'")
  $logRows  = $fileList.Select("Type = 'L'")
  if ($dataRows.Count -lt 1 -or $logRows.Count -lt 1) {
    throw "Could not find data/log logical names in RESTORE FILELISTONLY output."
  }

  $logicalDataName = [string]$dataRows[0]["LogicalName"]
  $logicalLogName  = [string]$logRows[0]["LogicalName"]

  $sourceFiles = Invoke-SqlQuery $master @"
SELECT mf.type_desc, mf.physical_name
FROM sys.master_files mf
JOIN sys.databases d ON d.database_id = mf.database_id
WHERE d.name = N'$(Escape-SqlLiteral $SourceDatabase)';
"@

  $sourceDataRows = $sourceFiles.Select("type_desc = 'ROWS'")
  if ($sourceDataRows.Count -lt 1) { throw "Could not resolve source MDF path for '$SourceDatabase'." }

  $sourceData = [string]$sourceDataRows[0]["physical_name"]
  $dir = Split-Path -Parent $sourceData

  $targetMdf = Join-Path $dir "$TargetDatabase.mdf"
  $targetLdf = Join-Path $dir "$TargetDatabase`_log.ldf"

  Write-Host "Restoring to:" -ForegroundColor DarkGray
  Write-Host "  $targetMdf" -ForegroundColor DarkGray
  Write-Host "  $targetLdf" -ForegroundColor DarkGray

  Write-Host "Restoring '$TargetDatabase'..." -ForegroundColor Cyan
  Invoke-SqlNonQuery $master @"
RESTORE DATABASE [$TargetDatabase]
FROM DISK = N'$backupPath'
WITH
  MOVE N'$logicalDataName' TO N'$targetMdf',
  MOVE N'$logicalLogName'  TO N'$targetLdf',
  REPLACE,
  RECOVERY;
"@

  Write-Host "Clone completed." -ForegroundColor Green
}
finally {
  if ($master) { $master.Close() }

  # Başarıda pencere kapanmasın istiyorsan -NoPause KULLANMA
  if (-not $NoPause) { Pause-IfConsole "`nPress Enter to close..." }
}
