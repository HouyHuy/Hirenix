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
        token = localStorage.getItem('accessToken');
      } else {
        token = await SecureStore.getItemAsync('accessToken');
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
