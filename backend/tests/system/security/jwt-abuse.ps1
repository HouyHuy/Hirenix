param(
    [string]$BaseUrl = $env:HIRENIX_BASE_URL
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($BaseUrl)) {
    $BaseUrl = 'http://localhost:5189'
}
$BaseUrl = $BaseUrl.TrimEnd('/')

function Invoke-Json($method, $uri, $body = $null, $token = $null) {
    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($token)) { $headers.Authorization = "Bearer $token" }
    $params = @{ Method = $method; Uri = $uri; Headers = $headers }
    if ($null -ne $body) {
        $params.ContentType = 'application/json'
        $params.Body = ($body | ConvertTo-Json -Depth 10)
    }
    Invoke-RestMethod @params
}

function Expect-Status($name, $method, $uri, $status, $token = $null, $body = $null) {
    try {
        Invoke-Json $method $uri $body $token | Out-Null
        throw "$name unexpectedly succeeded"
    } catch {
        if ($_.Exception.Message -like '*unexpectedly succeeded*') { throw }
        $actual = [int]$_.Exception.Response.StatusCode
        if ($actual -ne $status) { throw "$name expected HTTP $status but got HTTP $actual" }
        Write-Host "PASS $name HTTP $actual"
    }
}

$candidateLogin = Invoke-Json Post "$BaseUrl/api/Auth/login" @{ identifier = 'candidate@hirenix.com'; password = 'Candidate@123' }
$candidateToken = $candidateLogin.data.accessToken

Expect-Status 'candidate-cannot-list-employer-applications' Get "$BaseUrl/api/employer/applications" 403 $candidateToken
Expect-Status 'missing-token-auth-me' Get "$BaseUrl/api/Auth/me" 401
Expect-Status 'invalid-token-auth-me' Get "$BaseUrl/api/Auth/me" 401 'not-a-jwt'
Expect-Status 'bad-password-login' Post "$BaseUrl/api/Auth/login" 401 $null @{ identifier = 'candidate@hirenix.com'; password = 'WrongPassword@123' }

$me = Invoke-WebRequest "$BaseUrl/api/Auth/me" -Headers @{ Authorization = "Bearer $candidateToken" } -UseBasicParsing
if ($me.Content -match 'password|passwordHash|password_hash') {
    throw 'Sensitive password field leaked in /api/Auth/me response'
}
Write-Host 'PASS auth-me-sensitive-data'
