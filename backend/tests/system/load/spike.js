import http from 'k6/http';
import { check, sleep } from 'k6';
import { baseUrl } from '../shared/k6-helpers.js';

export const options = {
  scenarios: {
    spike: {
      executor: 'ramping-vus',
      stages: [
        { duration: '30s', target: 10 },
        { duration: '30s', target: 500 },
        { duration: '30s', target: 10 },
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.10'],
    http_req_duration: ['p(95)<2000'],
  },
};

export default function () {
  const response = http.get(`${baseUrl}/api/Jobs?page=1&pageSize=20`, { tags: { endpoint: 'jobs-list' } });
  check(response, { 'jobs list status below 500': (r) => r.status < 500 });
  sleep(1);
}
