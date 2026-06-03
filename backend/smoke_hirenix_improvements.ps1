$ErrorActionPreference = 'Stop'
$baseUrl = $env:HIRENIX_BASE_URL
if ([string]::IsNullOrWhiteSpace($baseUrl)) {
    $baseUrl = 'http://localhost:5189'
}
$baseUrl = $baseUrl.TrimEnd('/')
$runId = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

function Print-Ok($name, $detail = '') {
    if ($detail) { Write-Host "PASS $name - $detail" }
    else { Write-Host "PASS $name" }
}

function Assert($condition, $message) {
    if (-not $condition) { throw $message }
}

function AuthHeaders($token) {
    if ([string]::IsNullOrWhiteSpace($token)) { return @{} }
    return @{ Authorization = "Bearer $token" }
}

function Invoke-ApiJson($method, $uri, $token = $null, $body = $null) {
    $params = @{
        Method = $method
        Uri = $uri
        Headers = AuthHeaders $token
    }

    if ($null -ne $body) {
        $params.ContentType = 'application/json'
        $params.Body = ($body | ConvertTo-Json -Depth 20)
    }

    try {
        return Invoke-RestMethod @params
    } catch {
        if ((Get-ResponseStatusCode $_) -eq 429 -and $uri -like '*/api/Auth/login') {
            Start-Sleep -Seconds 65
            return Invoke-RestMethod @params
        }

        throw
    }
}

function Get-ResponseStatusCode($errorRecord) {
    if ($null -eq $errorRecord.Exception.Response) { return $null }
    return [int]$errorRecord.Exception.Response.StatusCode
}

function Invoke-OptionalApiJson($method, $uri, $token = $null, $body = $null) {
    try {
        return Invoke-ApiJson $method $uri $token $body
    } catch {
        if ((Get-ResponseStatusCode $_) -eq 404) { return $null }
        throw
    }
}

function Get-First($items, $name) {
    $array = @($items)
    Assert ($array.Count -gt 0) "$name is empty"
    return $array[0]
}

function Invoke-ApplicationMultipart($token, $jobId, $cvPath, $coverLetter) {
    Add-Type -AssemblyName System.Net.Http
    $client = [System.Net.Http.HttpClient]::new()
    $fileStream = $null
    $form = $null

    try {
        $client.DefaultRequestHeaders.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $token)
        $form = [System.Net.Http.MultipartFormDataContent]::new()
        $form.Add([System.Net.Http.StringContent]::new([string]$jobId), 'JobId')
        $form.Add([System.Net.Http.StringContent]::new($coverLetter), 'CoverLetter')

        $fileStream = [System.IO.File]::OpenRead($cvPath)
        $fileContent = [System.Net.Http.StreamContent]::new($fileStream)
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('application/pdf')
        $form.Add($fileContent, 'CvFile', [System.IO.Path]::GetFileName($cvPath))

        $response = $client.PostAsync("$baseUrl/api/Applications", $form).GetAwaiter().GetResult()
        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "Application multipart submit failed: HTTP $([int]$response.StatusCode) $text"
        }

        return $text | ConvertFrom-Json
    } finally {
        if ($null -ne $form) { $form.Dispose() }
        if ($null -ne $fileStream) { $fileStream.Dispose() }
        $client.Dispose()
    }
}

function Ensure-SampleCv($path) {
    if (Test-Path $path) { return }
    $pdfBase64 = 'JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZyAvUGFnZXMgMiAwIFIgPj4KZW5kb2JqCjIgMCBvYmoKPDwgL1R5cGUgL1BhZ2VzIC9LaWRzIFszIDAgUl0gL0NvdW50IDEgPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhZ2UgL1BhcmVudCAyIDAgUiAvTWVkaWFCb3ggWzAgMCAzMDAgMTQ0XSAvQ29udGVudHMgNCAwIFIgL1Jlc291cmNlcyA8PCAvRm9udCA8PCAvRjEgNSAwIFIgPj4gPj4gPj4KZW5kb2JqCjQgMCBvYmoKPDwgL0xlbmd0aCA2MSA+PgpzdHJlYW0KQlQKL0YxIDE4IFRmCjUwIDgwIFRkCihIaXJlbml4IFNtb2tlIFRlc3QgQ1YpIFRqCkVUCmVuZHN0cmVhbQplbmRvYmoKNSAwIG9iago8PCAvVHlwZSAvRm9udCAvU3VidHlwZSAvVHlwZTEgL0Jhc2VGb250IC9IZWx2ZXRpY2EgPj4KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDAwOSAwMDAwMCBuIAowMDAwMDAwMDU4IDAwMDAwIG4gCjAwMDAwMDAxMTUgMDAwMDAgbiAKMDAwMDAwMDI1NyAwMDAwMCBuIAowMDAwMDAwMzY4IDAwMDAwIG4gCnRyYWlsZXIKPDwgL1NpemUgNiAvUm9vdCAxIDAgUiA+PgpzdGFydHhyZWYKNDM4CiUlRU9GCg=='
    [System.IO.File]::WriteAllBytes($path, [Convert]::FromBase64String($pdfBase64))
}

$swagger = Invoke-WebRequest "$baseUrl/swagger/v1/swagger.json" -UseBasicParsing
Assert ($swagger.StatusCode -eq 200) "Swagger failed: $($swagger.StatusCode)"
Print-Ok 'swagger-json' "HTTP $($swagger.StatusCode)"

$swaggerUi = Invoke-WebRequest "$baseUrl/swagger/index.html" -UseBasicParsing
Assert ($swaggerUi.StatusCode -eq 200) "Swagger UI failed: $($swaggerUi.StatusCode)"
Print-Ok 'swagger-ui' "HTTP $($swaggerUi.StatusCode)"

$allowedCors = Invoke-WebRequest "$baseUrl/api/taxonomy/industries" -UseBasicParsing -Headers @{ Origin = 'http://localhost:8081' }
Assert ([string]$allowedCors.Headers['Access-Control-Allow-Origin'] -eq 'http://localhost:8081') 'Allowed CORS header missing'
Print-Ok 'cors-allowed-origin' $allowedCors.Headers['Access-Control-Allow-Origin']

$blockedCors = Invoke-WebRequest "$baseUrl/api/taxonomy/industries" -UseBasicParsing -Headers @{ Origin = 'http://evil.com' }
Assert (-not $blockedCors.Headers['Access-Control-Allow-Origin']) 'Blocked CORS origin unexpectedly allowed'
Print-Ok 'cors-blocked-origin' 'no Access-Control-Allow-Origin header'

try {
    Invoke-WebRequest "$baseUrl/hubs/messages/negotiate?negotiateVersion=1" -UseBasicParsing -Method Post | Out-Null
    throw 'SignalR anonymous negotiate unexpectedly succeeded'
} catch {
    Assert ((Get-ResponseStatusCode $_) -eq 401) 'SignalR anonymous negotiate did not return HTTP 401'
    Print-Ok 'signalr-anonymous-negotiate' 'HTTP 401'
}

$candidateLogin = Invoke-ApiJson Post "$baseUrl/api/Auth/login" $null @{ identifier = 'candidate@hirenix.com'; password = 'Candidate@123' }
Assert $candidateLogin.success 'Candidate login failed'
$candidateToken = $candidateLogin.data.accessToken
$candidateUserId = $candidateLogin.data.userId
Print-Ok 'candidate-login' "userId=$candidateUserId"

$candidateMe = Invoke-ApiJson Get "$baseUrl/api/Auth/me" $candidateToken
Assert ($candidateMe.success -and $candidateMe.data.role -eq 'Candidate') 'Candidate /me failed'
Print-Ok 'candidate-me' "role=$($candidateMe.data.role)"

$employerLogin = Invoke-ApiJson Post "$baseUrl/api/Auth/login" $null @{ identifier = 'employer@hirenix.com'; password = 'Employer@123' }
Assert $employerLogin.success 'Employer login failed'
$employerToken = $employerLogin.data.accessToken
$employerUserId = $employerLogin.data.userId
Print-Ok 'employer-login' "userId=$employerUserId"

$employerMe = Invoke-ApiJson Get "$baseUrl/api/Auth/me" $employerToken
Assert ($employerMe.success -and $employerMe.data.role -eq 'Employer') 'Employer /me failed'
Print-Ok 'employer-me' "role=$($employerMe.data.role)"

$authNegotiate = Invoke-WebRequest "$baseUrl/hubs/messages/negotiate?negotiateVersion=1&access_token=$candidateToken" -UseBasicParsing -Method Post
Assert ($authNegotiate.StatusCode -eq 200) "SignalR authenticated negotiate failed: $($authNegotiate.StatusCode)"
Print-Ok 'signalr-authenticated-negotiate' "HTTP $($authNegotiate.StatusCode)"

$industries = Invoke-ApiJson Get "$baseUrl/api/taxonomy/industries"
Assert $industries.success 'Industries taxonomy failed'
$industry = Get-First $industries.data 'Industries'
Print-Ok 'taxonomy-industries' "industryId=$($industry.id)"

$locations = Invoke-ApiJson Get "$baseUrl/api/taxonomy/locations"
Assert $locations.success 'Locations taxonomy failed'
$location = Get-First $locations.data 'Locations'
Print-Ok 'taxonomy-locations' "locationId=$($location.id)"

$skills = Invoke-ApiJson Get "$baseUrl/api/taxonomy/skills"
Assert $skills.success 'Skills taxonomy failed'
$skill = Get-First $skills.data 'Skills'
Print-Ok 'taxonomy-skills' "skillId=$($skill.id)"

$profile = Invoke-OptionalApiJson Get "$baseUrl/api/EmployerProfile/me" $employerToken
if ($null -eq $profile) {
    $company = Invoke-ApiJson Post "$baseUrl/api/Company" $employerToken @{
        name = "Hirenix Smoke Company $runId"
        description = 'Company created by backend smoke test'
        website = 'https://example.com'
        industryId = [uint32]$industry.id
        cityId = [uint32]$location.id
        address = 'Smoke Test Address'
    }
    Assert $company.id 'Company create did not return id'
    Print-Ok 'company-create' "companyId=$($company.id)"

    $profile = Invoke-ApiJson Post "$baseUrl/api/EmployerProfile" $employerToken @{
        companyId = [uint64]$company.id
        fullName = 'Hirenix Smoke Employer'
        title = 'QA Smoke Tester'
        isAdmin = $true
    }
    Assert $profile.id 'Employer profile create did not return id'
    Print-Ok 'employer-profile-create' "profileId=$($profile.id)"
} else {
    Assert $profile.companyId 'Employer profile missing companyId'
    Print-Ok 'employer-profile-existing' "profileId=$($profile.id); companyId=$($profile.companyId)"
}

$expiryDate = (Get-Date).ToUniversalTime().Date.AddDays(30).ToString('yyyy-MM-dd')
$createdJob = Invoke-ApiJson Post "$baseUrl/api/employer/jobs" $employerToken @{
    title = "Hirenix Smoke Job $runId"
    description = 'Backend smoke test job description'
    requirements = 'Smoke test requirements'
    responsibilities = 'Smoke test responsibilities'
    industryId = [uint32]$industry.id
    locationId = [uint32]$location.id
    workType = 'Fulltime'
    level = 'Junior'
    salaryMin = 1000
    salaryMax = 2000
    isRemote = $true
    expiryDate = $expiryDate
    skillIds = @([uint32]$skill.id)
}
Assert $createdJob.id 'Employer job create did not return id'
$jobId = $createdJob.id
Print-Ok 'employer-job-create' "jobId=$jobId"

$jobSearch = [Uri]::EscapeDataString($createdJob.title)
$jobs = Invoke-ApiJson Get "$baseUrl/api/Jobs?page=1&pageSize=10&search=$jobSearch"
Assert $jobs.success 'Jobs list failed'
$listedJobs = @($jobs.data.data)
$matchedJobs = @($listedJobs | Where-Object { $_.id -eq $jobId })
Assert ($matchedJobs.Count -gt 0) "Created job $jobId was not found in public jobs list"
Print-Ok 'jobs-list' "createdJobId=$jobId"

$jobDetail = Invoke-ApiJson Get "$baseUrl/api/Jobs/$jobId/detail" $candidateToken
Assert ($jobDetail.success -and $jobDetail.data.id -eq $jobId) 'Candidate job detail failed'
Assert (-not $jobDetail.data.hasApplied) 'Candidate unexpectedly has already applied to newly created job'
Print-Ok 'candidate-job-detail' "title=$($jobDetail.data.title)"

$fixtureDir = Join-Path ([System.IO.Path]::GetTempPath()) 'hirenix-smoke-fixtures'
New-Item -ItemType Directory -Force -Path $fixtureDir | Out-Null
$cvPath = Join-Path $fixtureDir 'sample-cv.pdf'
Ensure-SampleCv $cvPath

$application = Invoke-ApplicationMultipart $candidateToken $jobId $cvPath "Smoke test cover letter $runId"
Assert $application.data.id 'Application submit did not return id'
Assert $application.data.cvUrl 'Application submit did not return cvUrl'
$applicationId = $application.data.id
Print-Ok 'application-submit-multipart' "applicationId=$applicationId; cvUrl=$($application.data.cvUrl)"

$myApplications = Invoke-ApiJson Get "$baseUrl/api/Applications/my" $candidateToken
$matchedApplication = @($myApplications.data) | Where-Object { $_.id -eq $applicationId } | Select-Object -First 1
Assert $matchedApplication "Application $applicationId was not found in candidate application list"
Assert $matchedApplication.cvUrl "Application $applicationId missing candidate cvUrl"
Print-Ok 'application-signed-url' $matchedApplication.cvUrl

$employerApplications = Invoke-ApiJson Get "$baseUrl/api/employer/applications?jobId=$jobId" $employerToken
$employerApplication = @($employerApplications) | Where-Object { $_.id -eq $applicationId } | Select-Object -First 1
Assert $employerApplication "Application $applicationId was not found in employer ATS list"
Assert $employerApplication.cvUrl "Application $applicationId missing employer cvUrl"
Print-Ok 'employer-application-list' "applicationId=$($employerApplication.id)"

$statusUpdate = Invoke-ApiJson Put "$baseUrl/api/employer/applications/$applicationId/status" $employerToken @{
    status = 'Reviewing'
    reviewNotes = "Smoke reviewed $runId"
}
Assert ($statusUpdate.message -like '*updated*') 'Employer application status update failed'
Print-Ok 'employer-application-status' 'Reviewing'

$conversation = Invoke-ApiJson Post "$baseUrl/api/messages/conversations" $candidateToken @{ participantUserId = [uint64]$employerUserId }
Assert $conversation.id 'Conversation create did not return id'
$conversationId = $conversation.id
Print-Ok 'conversation-create' "conversationId=$conversationId"

$messageContent = "SignalR smoke message $runId"
& dotnet run --project (Join-Path $PSScriptRoot 'tools\SignalRSmoke\SignalRSmoke.csproj') -- `
    --hub "$baseUrl/hubs/messages" `
    --listen-token $employerToken `
    --send-url "$baseUrl/api/messages/conversations/$conversationId/items" `
    --send-token $candidateToken `
    --content $messageContent `
    --expect-from $candidateUserId `
    --timeout 20
if ($LASTEXITCODE -ne 0) {
    throw "SignalRSmoke helper failed with exit code $LASTEXITCODE"
}
Print-Ok 'signalr-message-received' "conversationId=$conversationId"

$messages = Invoke-ApiJson Get "$baseUrl/api/messages/conversations/$conversationId/items?page=1&pageSize=10" $employerToken
$receivedMessage = @($messages) | Where-Object { $_.content -eq $messageContent -and $_.senderId -eq $candidateUserId } | Select-Object -First 1
Assert $receivedMessage 'Sent message was not found in conversation items'
Print-Ok 'message-items' "messageId=$($receivedMessage.id)"

$readResult = Invoke-ApiJson Post "$baseUrl/api/messages/conversations/$conversationId/read" $employerToken
Assert ($readResult.updated -ge 1) 'Mark-as-read did not update any messages'
$employerConversation = Invoke-ApiJson Get "$baseUrl/api/messages/conversations/$conversationId" $employerToken
Assert ($employerConversation.unreadCount -eq 0) 'Conversation unreadCount did not reset to 0'
Print-Ok 'message-mark-as-read' "updated=$($readResult.updated)"

$closeJob = Invoke-WebRequest "$baseUrl/api/employer/jobs/$jobId/close" -UseBasicParsing -Method Post -Headers (AuthHeaders $employerToken)
Assert ($closeJob.StatusCode -eq 200) "Close smoke job failed: $($closeJob.StatusCode)"
Print-Ok 'employer-job-close' "jobId=$jobId"

Write-Host 'SMOKE_TEST_DONE'
