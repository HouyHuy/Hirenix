/**
 * API configuration — Hirenix
 * Centralized resolution of the backend base URL across web/native/dev/prod.
 *
 * Resolution priority:
 *   1. process.env.EXPO_PUBLIC_API_URL
 *   2. Sensible fallback per platform (web: localhost, native: Expo host if available)
 */
import { Platform } from 'react-native';

const DEFAULT_PORT = 5189;

const readFromEnv = (): string | undefined => {
  const fromEnv = process.env.EXPO_PUBLIC_API_URL;
  if (fromEnv && fromEnv.trim().length > 0) {
    return fromEnv.trim();
  }
  return undefined;
};

const fallback = (): string => {
  if (Platform.OS === 'web') {
    return `http://127.0.0.1:${DEFAULT_PORT}`;
  }

  // For native dev, try Expo host (works when running through Expo Go on the same Wi-Fi).
  const expoManifest = (globalThis as any).__expo?.manifest;
  const hostUri = expoManifest?.debuggerHost || expoManifest?.hostUri;

  if (typeof hostUri === 'string' && hostUri.includes(':')) {
    const host = hostUri.split(':')[0];
    if (host && host !== 'localhost' && host !== '127.0.0.1') {
      return `http://${host}:${DEFAULT_PORT}`;
    }
  }

  // Last resort. Using localhost on a real device will fail; user must set EXPO_PUBLIC_API_URL.
  if (__DEV__) {
    console.warn(
      '[Hirenix][api] EXPO_PUBLIC_API_URL is not configured. ' +
        'Falling back to http://127.0.0.1 — real devices will not be able to reach the API.',
    );
  }
  return `http://127.0.0.1:${DEFAULT_PORT}`;
};

export const getApiBaseUrl = (): string => {
  return readFromEnv() ?? fallback();
};

export const API_BASE_URL = getApiBaseUrl();
