param(
    [ValidateSet('Smoke', 'Regression', 'Perf', 'Load', 'Realtime', 'Security', 'Data', 'Failover', 'UI', 'Full', 'Soak')]
    [string]$Mode = 'Regression',
    [string]$BaseUrl = $env:HIRENIX_BASE_URL,
    [switch]$Strict,
    [switch]$EnableExternal,
    [switch]$EnableDestructive
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    $BaseUrl = 'http://localhost:5189'
}
$BaseUrl = $BaseUrl.TrimEnd('/')
$scriptRoot = $PSScriptRoot
$backendRoot = Split-Path $scriptRoot -Parent
$systemRoot = Join-Path $backendRoot 'tests\system'
$reportsRoot = Join-Path $systemRoot 'reports'
$runStamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$runReport = Join-Path $reportsRoot "system-$Mode-$runStamp.md"
$results = [System.Collections.Generic.List[object]]::new()

New-Item -ItemType Directory -Force -Path $reportsRoot | Out-Null

function Add-Result($area, $name, $status, $detail, $elapsed) {
    $results.Add([pscustomobject]@{
        Area = $area
        Name = $name
        Status = $status
        Detail = $detail
        ElapsedSeconds = [Math]::Round($elapsed.TotalSeconds, 2)
    }) | Out-Null

    Write-Host "$status [$area] $name - $detail"
}

function Skip-Step($reason) {
    throw "SKIP: $reason"
}

function Assert-Command($commandName) {
    if (-not (Get-Command $commandName -ErrorAction SilentlyContinue)) {
        Skip-Step "$commandName was not found in PATH"
    }
}

function Invoke-Native($file, [string[]]$arguments) {
    & $file @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$file failed with exit code $LASTEXITCODE"
    }
}

function Invoke-Step($area, $name, [scriptblock]$action, [switch]$Optional) {
    $started = Get-Date
    try {
        & $action
        Add-Result $area $name 'PASS' 'completed' ((Get-Date) - $started)
    } catch {
        $message = $_.Exception.Message
        if ($message.StartsWith('SKIP:')) {
            if ($Strict -and -not $Optional) {
                Add-Result $area $name 'FAIL' $message ((Get-Date) - $started)
            } else {
                Add-Result $area $name 'SKIP' $message.Substring(5).Trim() ((Get-Date) - $started)
            }
        } else {
            Add-Result $area $name 'FAIL' $message ((Get-Date) - $started)
        }
    }
}

function Invoke-K6($area, $name, $scriptPath, $duration = $null, $vus = $null) {
    Invoke-Step $area $name {
        Assert-Command 'k6'
        $env:HIRENIX_BASE_URL = $BaseUrl
        if ($duration) { $env:HIRENIX_K6_DURATION = $duration }
        if ($vus) { $env:HIRENIX_K6_VUS = [string]$vus }
        $summaryPath = Join-Path $reportsRoot "$area-$name-$runStamp.json"
        Invoke-Native 'k6' @('run', '--summary-export', $summaryPath, $scriptPath)
    } -Optional
}

function Invoke-Smoke {
    Invoke-Step 'smoke' 'deep-smoke' {
        $smokeScript = Join-Path $backendRoot 'smoke_hirenix_improvements.ps1'
        if (-not (Test-Path $smokeScript)) { throw "Smoke script not found: $smokeScript" }
        $env:HIRENIX_BASE_URL = $BaseUrl
        Invoke-Native 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $smokeScript)
    }
}

function Invoke-Regression {
    Invoke-Step 'regression' 'solution-build' {
        Invoke-Native 'dotnet' @('build', (Join-Path $backendRoot 'Hirenix.sln'))
    }

    Invoke-Step 'regression' 'system-xunit' {
        $testProject = Join-Path $systemRoot 'Hirenix.SystemTests\Hirenix.SystemTests.csproj'
        if (-not (Test-Path $testProject)) { Skip-Step "system test project not found: $testProject" }
        $env:HIRENIX_BASE_URL = $BaseUrl
        Invoke-Native 'dotnet' @('test', $testProject, '--no-build')
    } -Optional

    Invoke-Smoke
    Invoke-K6 'regression' 'perf-mini' (Join-Path $systemRoot 'perf\perf-baseline.js') '1m' 5
}

function Invoke-Perf {
    Invoke-K6 'perf' 'baseline' (Join-Path $systemRoot 'perf\perf-baseline.js')
}

function Invoke-Load {
    Invoke-K6 'load' 'ramp' (Join-Path $systemRoot 'load\ramp.js')
    Invoke-K6 'load' 'stress' (Join-Path $systemRoot 'load\stress.js')
    Invoke-K6 'load' 'spike' (Join-Path $systemRoot 'load\spike.js')
}

function Invoke-Realtime {
    Invoke-K6 'realtime' 'latency' (Join-Path $systemRoot 'realtime\latency.js')

    Invoke-Step 'realtime' 'nbomber-concurrent' {
        $project = Join-Path $systemRoot 'realtime\Hirenix.RealtimeLoad\Hirenix.RealtimeLoad.csproj'
        if (-not (Test-Path $project)) { Skip-Step "NBomber project not found: $project" }
        $env:HIRENIX_BASE_URL = $BaseUrl
        Invoke-Native 'dotnet' @('run', '--project', $project, '--', '--duration', '00:01:00', '--connections', '25')
    } -Optional
}

function Invoke-Security {
    Invoke-Step 'security' 'zap-baseline' {
        if (-not $EnableExternal) { Skip-Step 'requires -EnableExternal because ZAP may invoke Docker or external scanner binaries' }
        $zapScript = Join-Path $systemRoot 'security\zap-baseline.ps1'
        Invoke-Native 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $zapScript, '-BaseUrl', $BaseUrl)
    } -Optional

    Invoke-Step 'security' 'jwt-abuse' {
        $jwtScript = Join-Path $systemRoot 'security\jwt-abuse.ps1'
        Invoke-Native 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $jwtScript, '-BaseUrl', $BaseUrl)
    }
}

function Invoke-Data {
    Invoke-Step 'data' 'query-profile' {
        $dataScript = Join-Path $systemRoot 'data\query-profile.ps1'
        Invoke-Native 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $dataScript)
    } -Optional
}

function Invoke-Failover {
    Invoke-Step 'failover' 'api-recovery-probe' {
        if (-not $EnableDestructive) { Skip-Step 'requires -EnableDestructive because it stops the API process' }
        $failoverScript = Join-Path $systemRoot 'failover\api-recovery-probe.ps1'
        Invoke-Native 'powershell' @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $failoverScript, '-BaseUrl', $BaseUrl)
    } -Optional
}

function Invoke-UI {
    Invoke-Step 'ui' 'swagger-playwright' {
        $uiRoot = Join-Path $systemRoot 'ui'
        $playwright = Join-Path $uiRoot 'node_modules\.bin\playwright.cmd'
        if (-not (Test-Path $playwright)) { Skip-Step "Playwright is not installed under $uiRoot" }
        $env:HIRENIX_BASE_URL = $BaseUrl
        Push-Location $uiRoot
        try {
            Invoke-Native $playwright @('test')
        } finally {
            Pop-Location
        }
    } -Optional
}

switch ($Mode) {
    'Smoke' { Invoke-Smoke }
    'Regression' { Invoke-Regression }
    'Perf' { Invoke-Perf }
    'Load' { Invoke-Load }
    'Realtime' { Invoke-Realtime }
    'Security' { Invoke-Security }
    'Data' { Invoke-Data }
    'Failover' { Invoke-Failover }
    'UI' { Invoke-UI }
    'Soak' { Invoke-K6 'realtime' 'soak-24h' (Join-Path $systemRoot 'realtime\latency.js') '24h' 25 }
    'Full' {
        Invoke-Regression
        Invoke-Perf
        Invoke-Load
        Invoke-Realtime
        Invoke-Security
        Invoke-Data
        Invoke-Failover
        Invoke-UI
    }
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Hirenix System Test Run $runStamp") | Out-Null
$lines.Add('') | Out-Null
$lines.Add("- Mode: $Mode") | Out-Null
$lines.Add("- BaseUrl: $BaseUrl") | Out-Null
$lines.Add("- Strict: $Strict") | Out-Null
$lines.Add("- EnableExternal: $EnableExternal") | Out-Null
$lines.Add("- EnableDestructive: $EnableDestructive") | Out-Null
$lines.Add('') | Out-Null
$lines.Add('| Area | Step | Status | Seconds | Detail |') | Out-Null
$lines.Add('| --- | --- | --- | ---: | --- |') | Out-Null
foreach ($result in $results) {
    $detail = ([string]$result.Detail).Replace('|', '\|')
    $lines.Add("| $($result.Area) | $($result.Name) | $($result.Status) | $($result.ElapsedSeconds) | $detail |") | Out-Null
}

Set-Content -Path $runReport -Value $lines -Encoding UTF8
Set-Content -Path (Join-Path $systemRoot 'REPORT.md') -Value $lines -Encoding UTF8

$failures = @($results | Where-Object { $_.Status -eq 'FAIL' })
if ($failures.Count -gt 0) {
    Write-Host "SYSTEM_TEST_DONE FAIL report=$runReport"
    exit 1
}

Write-Host "SYSTEM_TEST_DONE PASS report=$runReport"
