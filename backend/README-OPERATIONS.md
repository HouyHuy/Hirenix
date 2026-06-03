# Hirenix Backend Operations

Quick backend runbook for local API development, database seeding, smoke testing, storage configuration, SignalR, and CORS.

## Run API in development

From `backend/Hirenix.API`:

```powershell
dotnet run --launch-profile http
```

Default development URL:

```text
http://localhost:5189
```

Swagger is available in `Development`:

```text
http://localhost:5189/swagger
```

## Seed default data

From `backend/Hirenix.API`:

```powershell
dotnet run -- --seed
```

Default smoke-test users:

| Role | Email | Password |
| --- | --- | --- |
| Candidate | `candidate@hirenix.com` | `Candidate@123` |
| Employer | `employer@hirenix.com` | `Employer@123` |
| Admin | `admin@hirenix.com` | `Admin@123` |

`UsersSeeder` is idempotent for these default emails, so rerunning seed adds only missing default accounts.

## Run backend smoke test

Keep the API running, then from `backend`:

```powershell
powershell -ExecutionPolicy Bypass -File .\smoke_hirenix_improvements.ps1
```

Optional custom API URL:

```powershell
$env:HIRENIX_BASE_URL = 'http://localhost:5189'
powershell -ExecutionPolicy Bypass -File .\smoke_hirenix_improvements.ps1
```

The smoke test verifies:

- Swagger JSON/UI.
- CORS allowed and blocked origins.
- Candidate and employer auth.
- Taxonomy endpoints.
- Employer profile/company/job prerequisites.
- Public jobs and candidate job detail.
- Candidate multipart CV application upload.
- Candidate/employer CV access URL projection.
- Employer ATS application list and status update.
- SignalR authenticated negotiation and real `MessageReceived` event.
- Messaging REST items and mark-as-read.

## Run backend system tests

The system test scaffold lives under:

```text
backend/tests/system
```

Run the default regression set from `backend`:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-system-tests.ps1 -Mode Regression
```

Available modes:

| Mode | What it runs |
| --- | --- |
| `Smoke` | Deep backend smoke only |
| `Regression` | Build, xUnit system tests, deep smoke, optional k6 mini baseline |
| `Perf` | k6 performance baseline |
| `Load` | k6 ramp, stress, and spike |
| `Realtime` | k6 SignalR latency plus .NET concurrent SignalR runner |
| `Security` | JWT/security checks; add `-EnableExternal` for OWASP ZAP |
| `Data` | MySQL query profiling with `EXPLAIN` |
| `Failover` | Requires `-EnableDestructive`; stops/restarts local API and measures RTO |
| `UI` | Swagger UI Playwright checks |
| `Full` | Runs all areas except destructive/external steps unless enabled |
| `Soak` | 24-hour realtime soak scenario |

Optional tools for full coverage:

```text
k6
Docker Desktop
OWASP ZAP or Docker
Node.js + npm
Playwright Chromium
mysql CLI
```

Install Playwright UI dependencies once:

```powershell
cd .\tests\system\ui
npm install
npx playwright install chromium
```

Reports are written to:

```text
backend/tests/system/reports
backend/tests/system/REPORT.md
```

## Storage providers

Configure storage under `Storage` in `appsettings*.json` or environment variables.

| Provider | `Storage:Provider` | Required keys |
| --- | --- | --- |
| Local | `Local` | none; files are stored under `wwwroot/uploads` |
| S3 | `S3` | `Storage:S3:Region`, `Storage:S3:BucketName`, optional `Storage:S3:BasePath`, `Storage:S3:SignedUrlMinutes` |
| Azure Blob | `AzureBlob` | `Storage:AzureBlob:ConnectionString`, `Storage:AzureBlob:ContainerName`, optional `Storage:AzureBlob:BasePath`, `Storage:AzureBlob:SignedUrlMinutes` |

The application uses `IFileStorageService.GetAccessUrl(...)` so local storage returns local upload URLs and cloud providers can return signed URLs.

## SignalR messaging

Hub endpoint:

```text
/hubs/messages
```

JWT is accepted through the normal Authorization header for negotiated clients and through the `access_token` query string for hub connections:

```text
/hubs/messages?access_token=<jwt>
```

Mobile integration lives in `mobile/src/services/messageHub.ts` and subscribes to:

```text
MessageReceived
```

The backend smoke helper at `backend/tools/SignalRSmoke` opens a WebSocket connection, sends a message through the REST API, and asserts the hub event is received.

## CORS

Allowed origins are configured under:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:8081"
  ]
}
```

Use production frontend/mobile dev server origins before deployment. If the list is empty, the API falls back to `AllowAnyOrigin`, which is convenient for local development but should not be used for production.
