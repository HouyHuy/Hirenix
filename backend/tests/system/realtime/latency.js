import http from 'k6/http';
import ws from 'k6/ws';
import { check, fail } from 'k6';
import { Trend } from 'k6/metrics';
import { baseUrl, login, authHeaders, getOrCreateConversation, candidateEmail, candidatePassword, employerEmail, employerPassword } from '../shared/k6-helpers.js';

const e2eLatency = new Trend('signalr_message_e2e_latency_ms', true);

export const options = {
  scenarios: {
    realtime_latency: {
      executor: 'constant-vus',
      vus: Number(__ENV.HIRENIX_K6_VUS || 1),
      duration: __ENV.HIRENIX_K6_DURATION || '1m',
    },
  },
  thresholds: {
    signalr_message_e2e_latency_ms: ['p(95)<1000', 'p(99)<3000'],
  },
};

export function setup() {
  const candidate = login(candidateEmail, candidatePassword);
  const employer = login(employerEmail, employerPassword);
  const conversationId = getOrCreateConversation(candidate.token, employer.userId);
  return { candidate, employer, conversationId };
}

export default function (data) {
  const wsBase = baseUrl.replace(/^http:/, 'ws:').replace(/^https:/, 'wss:');
  const url = `${wsBase}/hubs/messages?access_token=${encodeURIComponent(data.employer.token)}`;
  const content = `k6 realtime ${__VU}-${__ITER}-${Date.now()}`;
  const sentAt = Date.now();
  let received = false;

  const response = ws.connect(url, {}, (socket) => {
    socket.on('open', () => {
      socket.send('{"protocol":"json","version":1}\u001e');
      http.post(
        `${baseUrl}/api/messages/conversations/${data.conversationId}/items`,
        JSON.stringify({ content }),
        { headers: authHeaders(data.candidate.token), tags: { endpoint: 'signalr-send-message' } },
      );
    });

    socket.on('message', (raw) => {
      const frames = raw.split('\u001e').filter((frame) => frame.length > 0);
      for (const frame of frames) {
        if (frame === '{}') {
          continue;
        }

        const payload = JSON.parse(frame);
        if (payload.type === 1 && payload.target === 'MessageReceived' && payload.arguments && payload.arguments.length > 0) {
          const message = payload.arguments[0];
          if (message.content === content || message.Content === content) {
            received = true;
            e2eLatency.add(Date.now() - sentAt);
            socket.close();
          }
        }
      }
    });

    socket.setTimeout(() => {
      socket.close();
    }, Number(__ENV.HIRENIX_SIGNALR_TIMEOUT_MS || 5000));
  });

  check(response, { 'websocket upgraded': (r) => r && r.status === 101 });
  if (!received) {
    fail(`MessageReceived event was not observed for ${content}`);
  }
}
