import axios from 'axios';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

const getBaseUrl = () => {
  if (Platform.OS === 'web') {
    return 'http://127.0.0.1:5189';
  }
  // iOS Simulator và Android Emulator đều dùng IP của máy host
  return 'http://192.168.100.127:5189';
};

export const apiClient = axios.create({
  baseURL: getBaseUrl(),
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

apiClient.interceptors.request.use(
  async (config) => {
    try {
      let token = null;
      if (Platform.OS === 'web') {
        token = localStorage.getItem('hirenix_access_token');
      } else {
        token = await SecureStore.getItemAsync('hirenix_access_token');
      }
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    } catch (error) {
      console.error('Error getting token for request', error);
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Token refresh queue management
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: any) => void;
}> = [];

const processQueue = (error: any = null, token: string | null = null) => {
  failedQueue.forEach((promise) => {
    if (error) {
      promise.reject(error);
    } else if (token) {
      promise.resolve(token);
    }
  });
  failedQueue = [];
};

// Response interceptor for token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 error and haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      // If already refreshing, queue this request
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      isRefreshing = true;

      try {
        // Get refresh token
        let refreshToken = null;
        if (Platform.OS === 'web') {
          refreshToken = localStorage.getItem('hirenix_refresh_token');
        } else {
          refreshToken = await SecureStore.getItemAsync('hirenix_refresh_token');
        }

        if (!refreshToken) {
          throw new Error('No refresh token available');
        }

        // Call refresh endpoint
        const response = await axios.post(`${getBaseUrl()}/api/auth/refresh`, {
          refreshToken,
        });

        const { accessToken, refreshToken: newRefreshToken } = response.data;

        // Save new tokens
        if (Platform.OS === 'web') {
          localStorage.setItem('hirenix_access_token', accessToken);
          localStorage.setItem('hirenix_refresh_token', newRefreshToken);
        } else {
          await SecureStore.setItemAsync('hirenix_access_token', accessToken);
          await SecureStore.setItemAsync('hirenix_refresh_token', newRefreshToken);
        }

        // ⭐ Update global Authorization header
        apiClient.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;

        // Process queued requests
        processQueue(null, accessToken);

        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed - clear tokens and reject all queued requests
        console.error('Token refresh failed:', refreshError);
        
        processQueue(refreshError, null);
        
        // Clear tokens
        if (Platform.OS === 'web') {
          localStorage.removeItem('hirenix_access_token');
          localStorage.removeItem('hirenix_refresh_token');
        } else {
          await SecureStore.deleteItemAsync('hirenix_access_token');
          await SecureStore.deleteItemAsync('hirenix_refresh_token');
        }
        
        // Let the error propagate so AuthContext can handle logout
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
