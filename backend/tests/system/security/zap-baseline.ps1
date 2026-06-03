param(
    [string]$BaseUrl = $env:HIRENIX_BASE_URL,
    [string]$ReportDirectory = $null
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    $BaseUrl = 'http://localhost:5189'
}
$BaseUrl = $BaseUrl.TrimEnd('/')
if ([string]::IsNullOrWhiteSpace($ReportDirectory)) {
    $ReportDirectory = Join-Path $PSScriptRoot '..\reports\security'
}

New-Item -ItemType Directory -Force -Path $ReportDirectory | Out-Null
$swaggerUrl = "$BaseUrl/swagger/v1/swagger.json"
$reportName = "zap-api-$(Get-Date -Format yyyyMMdd-HHmmss)"

if (Get-Command 'zap-api-scan.py' -ErrorAction SilentlyContinue) {
    & zap-api-scan.py -t $swaggerUrl -f openapi -J "$reportName.json" -r "$reportName.html"
    if ($LASTEXITCODE -ne 0) { throw "zap-api-scan.py failed with exit code $LASTEXITCODE" }
    Move-Item -Force "$reportName.json" (Join-Path $ReportDirectory "$reportName.json")
    Move-Item -Force "$reportName.html" (Join-Path $ReportDirectory "$reportName.html")
    Write-Host "ZAP_DONE report=$ReportDirectory"
    exit 0
}

if (Get-Command 'docker' -ErrorAction SilentlyContinue) {
    $volume = "${ReportDirectory}:/zap/wrk:rw"
    & docker run --rm -v $volume ghcr.io/zaproxy/zaproxy:stable zap-api-scan.py -t $swaggerUrl -f openapi -J "$reportName.json" -r "$reportName.html"
    if ($LASTEXITCODE -ne 0) { throw "docker ZAP scan failed with exit code $LASTEXITCODE" }
    Write-Host "ZAP_DONE report=$ReportDirectory"
    exit 0
}

throw 'Neither zap-api-scan.py nor docker was found. Install OWASP ZAP or Docker to run this scan.'
