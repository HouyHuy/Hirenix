# Swagger UI System Tests

Install once from this folder:

```powershell
npm install
npx playwright install chromium
```

Run through the orchestrator:

```powershell
powershell -ExecutionPolicy Bypass -File ..\..\..\scripts\run-system-tests.ps1 -Mode UI
```
