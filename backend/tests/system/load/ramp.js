import http from 'k6/http';
import { check, sleep } from 'k6';
import { baseUrl } from '../shared/k6-helpers.js';

export const options = {
  scenarios: {
    ramp: {
      executor: 'ramping-vus',
      stages: [
        { duration: __ENV.HIRENIX_RAMP_STAGE || '2m', target: Number(__ENV.HIRENIX_RAMP_1 || 50) },
        { duration: __ENV.HIRENIX_RAMP_STAGE || '2m', target: Number(__ENV.HIRENIX_RAMP_2 || 200) },
        { duration: __ENV.HIRENIX_RAMP_STAGE || '2m', target: Number(__ENV.HIRENIX_RAMP_3 || 500) },
        { duration: '1m', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1000'],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/api/Jobs?page=1&pageSize=20`, { tags: { endpoint: 'jobs-list' } });
  check(response, { 'jobs list 200': (r) => r.status === 200 });
  sleep(1);
}
