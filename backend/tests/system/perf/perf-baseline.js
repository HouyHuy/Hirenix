import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { baseUrl, login, authHeaders, getFirstJobId, candidateEmail, candidatePassword, employerEmail, employerPassword } from '../shared/k6-helpers.js';

const vus = Number(__ENV.HIRENIX_K6_VUS || 10);
const duration = __ENV.HIRENIX_K6_DURATION || '5m';

export const options = {
  scenarios: {
    baseline: {
      executor: 'constant-vus',
      vus,
      duration,
    },
  },
  thresholds: {
    'http_req_failed': ['rate<0.01'],
    'http_req_duration{endpoint:jobs-list}': ['p(95)<200', 'p(99)<500'],
    'http_req_duration{endpoint:job-detail}': ['p(95)<300', 'p(99)<700'],
    'http_req_duration{endpoint:auth-login}': ['p(95)<400', 'p(99)<900'],
    'http_req_duration{endpoint:employer-applications}': ['p(95)<400', 'p(99)<900'],
    'http_req_duration{endpoint:message-items}': ['p(95)<250', 'p(99)<600'],
  },
};

export function setup() {
  const candidate = login(candidateEmail, candidatePassword);
  const employer = login(employerEmail, employerPassword);
  const jobId = getFirstJobId();
  return { candidate, employer, jobId };
}

export default function (data) {
  group('jobs list', () => {
    const response = http.get(`${baseUrl}/api/Jobs?page=1&pageSize=20`, { tags: { endpoint: 'jobs-list' } });
    check(response, { 'jobs list 200': (r) => r.status === 200 });
  });

  group('candidate job detail', () => {
    const response = http.get(`${baseUrl}/api/Jobs/${data.jobId}/detail`, {
      headers: authHeaders(data.candidate.token),
      tags: { endpoint: 'job-detail' },
    });
    check(response, { 'job detail 200': (r) => r.status === 200 });
  });

  group('candidate me', () => {
    const response = http.get(`${baseUrl}/api/Auth/me`, {
      headers: authHeaders(data.candidate.token),
      tags: { endpoint: 'auth-me' },
    });
    check(response, { 'candidate me 200': (r) => r.status === 200 });
  });

  group('employer applications', () => {
    const response = http.get(`${baseUrl}/api/employer/applications`, {
      headers: authHeaders(data.employer.token),
      tags: { endpoint: 'employer-applications' },
    });
    check(response, { 'employer applications 200': (r) => r.status === 200 });
  });

  group('message conversations', () => {
    const response = http.get(`${baseUrl}/api/messages/conversations`, {
      headers: authHeaders(data.candidate.token),
      tags: { endpoint: 'message-items' },
    });
    check(response, { 'conversations 200': (r) => r.status === 200 });
  });

  sleep(1);
}
