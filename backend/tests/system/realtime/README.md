# Realtime System Tests

- `latency.js` is the strict SignalR latency scenario for k6 and keeps the p95 < 1s / p99 < 3s SLO.
- `Hirenix.RealtimeLoad` is a no-extra-dependency .NET concurrent delivery runner. It opens fresh WebSocket connections repeatedly, so its threshold is looser: p95 < 3s and p99 < 5s with error rate < 5%.

Run quick mode from `backend`:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\run-system-tests.ps1 -Mode Realtime
```
