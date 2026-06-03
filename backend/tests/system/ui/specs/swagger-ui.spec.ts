import { expect, test } from '@playwright/test';

async function login(request: any, identifier: string, password: string): Promise<string> {
  const response = await request.post('/api/Auth/login', {
    data: { identifier, password },
  });
  expect(response.ok()).toBeTruthy();
  const body = await response.json();
  return body.data.accessToken;
}

test('Swagger UI loads and OpenAPI document is reachable', async ({ page, request }: any) => {
  const swagger = await request.get('/swagger/v1/swagger.json');
  expect(swagger.ok()).toBeTruthy();
  const openApi = await swagger.json();
  expect(openApi.openapi).toBeTruthy();
  expect(openApi.paths['/api/jobs']).toBeTruthy();

  await page.goto('/swagger/index.html');
  await expect(page.locator('section.swagger-ui')).toBeVisible();
  await expect(page.getByText('Hirenix API')).toBeVisible();
});

test('Authenticated jobs API call succeeds with a real candidate token', async ({ request }: any) => {
  const token = await login(request, 'candidate@hirenix.com', 'Candidate@123');
  const response = await request.get('/api/Jobs?page=1&pageSize=1', {
    headers: { Authorization: `Bearer ${token}` },
  });
  expect(response.ok()).toBeTruthy();
});

test('Swagger UI has stable responsive shell', async ({ page }: any) => {
  for (const viewport of [
    { width: 390, height: 844 },
    { width: 820, height: 1180 },
    { width: 1440, height: 900 },
  ]) {
    await page.setViewportSize(viewport);
    await page.goto('/swagger/index.html');
    await expect(page.locator('section.swagger-ui')).toBeVisible();
  }
});
