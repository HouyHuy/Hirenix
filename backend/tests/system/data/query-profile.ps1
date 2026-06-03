param(
    [string]$HostName = 'localhost',
    [int]$Port = 3306,
    [string]$Database = 'hirenix',
    [string]$User = 'root',
    [string]$Password = 'root',
    [string]$ReportDirectory = $null
)

$ErrorActionPreference = 'Stop'

if (-not (Get-Command 'mysql' -ErrorAction SilentlyContinue)) {
    throw 'mysql CLI was not found in PATH'
}

if ([string]::IsNullOrWhiteSpace($ReportDirectory)) {
    $ReportDirectory = Join-Path $PSScriptRoot '..\reports\data'
}
New-Item -ItemType Directory -Force -Path $ReportDirectory | Out-Null

$sqlPath = Join-Path $PSScriptRoot 'query-profile.sql'
$reportPath = Join-Path $ReportDirectory "query-profile-$(Get-Date -Format yyyyMMdd-HHmmss).txt"

& mysql -h $HostName -P $Port -u $User "-p$Password" $Database -t -vvv --table --comments --show-warnings --execute "source $sqlPath" | Tee-Object -FilePath $reportPath
if ($LASTEXITCODE -ne 0) { throw "mysql query profiling failed with exit code $LASTEXITCODE" }

Write-Host "DATA_PROFILE_DONE report=$reportPath"
