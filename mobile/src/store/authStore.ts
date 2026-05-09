import { create } from 'zustand';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

interface AuthState {
  isAuthenticated: boolean;
  accessToken: string | null;
  user: any | null;
  setAuth: (token: string, user: any) => void;
  logout: () => void;
  initAuth: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set) => ({
  isAuthenticated: false,
  accessToken: null,
  user: null,

  setAuth: async (token, user) => {
    if (Platform.OS === 'web') {
      localStorage.setItem('accessToken', token);
    } else {
      await SecureStore.setItemAsync('accessToken', token);
    }
    set({ isAuthenticated: true, accessToken: token, user });
  },

  logout: async () => {
    if (Platform.OS === 'web') {
      localStorage.removeItem('accessToken');
    } else {
      await SecureStore.deleteItemAsync('accessToken');
    }
    set({ isAuthenticated: false, accessToken: null, user: null });
  },

  initAuth: async () => {
    let token = null;
    try {
      if (Platform.OS === 'web') {
        token = localStorage.getItem('accessToken');
      } else {
        token = await SecureStore.getItemAsync('accessToken');
      }
    } catch (error) {
      console.log('Error init auth', error);
    }
    if (token) {
      set({ isAuthenticated: true, accessToken: token });
    }
  },
}));
