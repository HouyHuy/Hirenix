param(
    [string]$BaseUrl = $env:HIRENIX_BASE_URL,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    $BaseUrl = 'http://localhost:5189'
}
$BaseUrl = $BaseUrl.TrimEnd('/')
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

$connections = Get-NetTCPConnection -State Listen -LocalPort 5189 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if (-not $connections) {
    throw 'No API process is listening on port 5189'
}

foreach ($processId in $connections) {
    Stop-Process -Id $processId -Force
}

$apiRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\Hirenix.API')
$process = Start-Process -FilePath 'dotnet' -ArgumentList @('run', '--no-build', '--launch-profile', 'http') -WorkingDirectory $apiRoot -PassThru -WindowStyle Hidden
$started = Get-Date

try {
    do {
        Start-Sleep -Seconds 2
        try {
            $response = Invoke-WebRequest "$BaseUrl/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                $rto = [Math]::Round(((Get-Date) - $started).TotalSeconds, 2)
                Write-Host "FAILOVER_API_RECOVERY_DONE rtoSeconds=$rto pid=$($process.Id)"
                exit 0
            }
        } catch {
        }
    } while ((Get-Date) -lt $deadline)

    throw "API did not recover within $TimeoutSeconds seconds"
} catch {
    if (-not $process.HasExited) { Stop-Process -Id $process.Id -Force }
    throw
}
