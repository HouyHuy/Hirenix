/**
 * AuthContext — Hirenix
 * Quản lý authentication state và user data
 */
import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';
import { authApi } from '../api/authApi';

interface User {
  userId: number;
  email?: string;
  phone?: string;
  role?: string;
}

interface AuthContextType {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (accessToken: string, refreshToken: string, user: User) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'hirenix_access_token',
  REFRESH_TOKEN: 'hirenix_refresh_token',
  USER: 'hirenix_user',
};

// Storage wrapper for web compatibility
const storage = {
  getItem: async (key: string): Promise<string | null> => {
    try {
      if (Platform.OS === 'web') {
        return localStorage.getItem(key);
      }
      return await SecureStore.getItemAsync(key);
    } catch (error) {
      console.error(`Error getting item ${key}:`, error);
      return null;
    }
  },
  setItem: async (key: string, value: string): Promise<void> => {
    try {
      if (Platform.OS === 'web') {
        localStorage.setItem(key, value);
        return;
      }
      await SecureStore.setItemAsync(key, value);
    } catch (error) {
      console.error(`Error setting item ${key}:`, error);
      throw error;
    }
  },
  removeItem: async (key: string): Promise<void> => {
    try {
      if (Platform.OS === 'web') {
        localStorage.removeItem(key);
        return;
      }
      await SecureStore.deleteItemAsync(key);
    } catch (error) {
      console.error(`Error removing item ${key}:`, error);
      throw error;
    }
  },
};

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Check authentication on app start
  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    try {
      const [storedAccessToken, storedRefreshToken, storedUser] = await Promise.all([
        storage.getItem(STORAGE_KEYS.ACCESS_TOKEN),
        storage.getItem(STORAGE_KEYS.REFRESH_TOKEN),
        storage.getItem(STORAGE_KEYS.USER),
      ]);

      if (storedAccessToken && storedRefreshToken && storedUser) {
        setAccessToken(storedAccessToken);
        setRefreshToken(storedRefreshToken);
        setUser(JSON.parse(storedUser));
      }
    } catch (error) {
      console.error('Error checking auth:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const login = async (newAccessToken: string, newRefreshToken: string, newUser: User) => {
    try {
      await Promise.all([
        storage.setItem(STORAGE_KEYS.ACCESS_TOKEN, newAccessToken),
        storage.setItem(STORAGE_KEYS.REFRESH_TOKEN, newRefreshToken),
        storage.setItem(STORAGE_KEYS.USER, JSON.stringify(newUser)),
      ]);

      setAccessToken(newAccessToken);
      setRefreshToken(newRefreshToken);
      setUser(newUser);
    } catch (error) {
      console.error('Error saving auth data:', error);
      throw error;
    }
  };

  const logout = async () => {
    try {
      // Call logout API if we have a refresh token
      if (refreshToken) {
        try {
          await authApi.logout(refreshToken);
        } catch (error: any) {
          // Ignore 401 errors - token might be expired, but local logout still works
          if (error?.response?.status !== 401) {
            console.error('Error calling logout API:', error);
          }
        }
      }

      // Clear local storage
      await Promise.all([
        storage.removeItem(STORAGE_KEYS.ACCESS_TOKEN),
        storage.removeItem(STORAGE_KEYS.REFRESH_TOKEN),
        storage.removeItem(STORAGE_KEYS.USER),
      ]);

      setAccessToken(null);
      setRefreshToken(null);
      setUser(null);
    } catch (error) {
      console.error('Error during logout:', error);
      throw error;
    }
  };

  const value: AuthContextType = {
    user,
    accessToken,
    refreshToken,
    isLoading,
    isAuthenticated: !!user && !!accessToken,
    login,
    logout,
    checkAuth,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
