import http from 'k6/http';
import { check, sleep } from 'k6';
import { baseUrl } from '../shared/k6-helpers.js';

export const options = {
  scenarios: {
    stress: {
      executor: 'ramping-vus',
      stages: [
        { duration: '1m', target: 100 },
        { duration: '2m', target: 250 },
        { duration: '2m', target: 500 },
        { duration: '2m', target: 750 },
        { duration: '1m', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.10'],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/api/Jobs?page=1&pageSize=20`, { tags: { endpoint: 'jobs-list' } });
  check(response, { 'jobs list does not return 5xx': (r) => r.status < 500 });
  sleep(1);
}
