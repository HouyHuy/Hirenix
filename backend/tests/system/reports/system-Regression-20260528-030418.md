# Hirenix System Test Run 20260528-030418

- Mode: Regression
- BaseUrl: http://localhost:5189
- Strict: False
- EnableExternal: False
- EnableDestructive: False

| Area | Step | Status | Seconds | Detail |
| --- | --- | --- | ---: | --- |
| regression | solution-build | PASS | 1.11 | completed |
| regression | system-xunit | FAIL | 8.92 | dotnet failed with exit code 1 |
| smoke | deep-smoke | FAIL | 2.58 | powershell failed with exit code 1 |
| regression | perf-mini | SKIP | 0.03 | k6 was not found in PATH |
