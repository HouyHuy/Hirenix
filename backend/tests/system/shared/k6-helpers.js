import http from 'k6/http';
import { check, fail } from 'k6';

export const baseUrl = (__ENV.HIRENIX_BASE_URL || 'http://localhost:5189').replace(/\/$/, '');
export const candidateEmail = __ENV.HIRENIX_CANDIDATE_EMAIL || 'candidate@hirenix.com';
export const candidatePassword = __ENV.HIRENIX_CANDIDATE_PASSWORD || 'Candidate@123';
export const employerEmail = __ENV.HIRENIX_EMPLOYER_EMAIL || 'employer@hirenix.com';
export const employerPassword = __ENV.HIRENIX_EMPLOYER_PASSWORD || 'Employer@123';

export function login(identifier, password) {
  const response = http.post(`${baseUrl}/api/Auth/login`, JSON.stringify({ identifier, password }), {
    headers: { 'Content-Type': 'application/json' },
    tags: { endpoint: 'auth-login' },
  });

  check(response, {
    'login status is 200': (r) => r.status === 200,
    'login has token': (r) => Boolean(r.json('data.accessToken')),
  });

  if (response.status !== 200 || !response.json('data.accessToken')) {
    fail(`Login failed for ${identifier}: ${response.status} ${response.body}`);
  }

  return {
    token: response.json('data.accessToken'),
    userId: response.json('data.userId'),
  };
}

export function authHeaders(token) {
  return {
    Authorization: `Bearer ${token}`,
    'Content-Type': 'application/json',
  };
}

export function getFirstJobId() {
  const response = http.get(`${baseUrl}/api/Jobs?page=1&pageSize=1`, { tags: { endpoint: 'jobs-list' } });
  check(response, { 'jobs list status is 200': (r) => r.status === 200 });
  const id = response.json('data.data.0.id');
  if (!id) {
    fail(`Could not resolve a job id: ${response.status} ${response.body}`);
  }
  return id;
}

export function getOrCreateConversation(candidateToken, employerUserId) {
  const response = http.post(
    `${baseUrl}/api/messages/conversations`,
    JSON.stringify({ participantUserId: employerUserId }),
    { headers: authHeaders(candidateToken), tags: { endpoint: 'conversation-create' } },
  );

  check(response, { 'conversation create status is 200': (r) => r.status === 200 });
  const id = response.json('id');
  if (!id) {
    fail(`Could not create conversation: ${response.status} ${response.body}`);
  }
  return id;
}
