# Hirenix System Test Run 20260528-222156

- Mode: Full
- BaseUrl: http://localhost:5189
- Strict: False
- EnableExternal: False
- EnableDestructive: False

| Area | Step | Status | Seconds | Detail |
| --- | --- | --- | ---: | --- |
| regression | solution-build | PASS | 2.71 | completed |
| regression | system-xunit | PASS | 21.44 | completed |
| smoke | deep-smoke | PASS | 11.84 | completed |
| regression | perf-mini | SKIP | 0.22 | k6 was not found in PATH |
| perf | baseline | SKIP | 0.14 | k6 was not found in PATH |
| load | ramp | SKIP | 0.19 | k6 was not found in PATH |
| load | stress | SKIP | 0.14 | k6 was not found in PATH |
| load | spike | SKIP | 0.17 | k6 was not found in PATH |
| realtime | latency | SKIP | 0.17 | k6 was not found in PATH |
| realtime | nbomber-concurrent | PASS | 69.87 | completed |
| security | zap-baseline | SKIP | 0 | requires -EnableExternal because ZAP may invoke Docker or external scanner binaries |
| security | jwt-abuse | PASS | 3.44 | completed |
| data | query-profile | PASS | 1.12 | completed |
| failover | api-recovery-probe | SKIP | 0 | requires -EnableDestructive because it stops the API process |
| ui | swagger-playwright | PASS | 4.05 | completed |
