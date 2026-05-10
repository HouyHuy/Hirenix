import { apiClient } from './apiClient';

export interface LoginRequest {
  identifier: string; // Email or phone
  password: string;
}

export interface RegisterRequest {
  email?: string;
  phone?: string;
  password: string;
  role?: 'candidate' | 'employer';
}

export interface VerifyOtpRequest {
  email: string;
  otpCode: string;
}

export interface ResendOtpRequest {
  email: string;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface FacebookLoginRequest {
  accessToken: string;
}

export const authApi = {
  checkEmail: async (email: string) => {
    const response = await apiClient.get(`/api/Auth/check-email?email=${encodeURIComponent(email)}`);
    return response.data;
  },

  checkPhone: async (phone: string) => {
    const response = await apiClient.get(`/api/Auth/check-phone?phone=${encodeURIComponent(phone)}`);
    return response.data;
  },

  login: async (data: LoginRequest) => {
    const response = await apiClient.post('/api/Auth/login', data);
    return response.data;
  },

  register: async (data: RegisterRequest) => {
    const response = await apiClient.post('/api/Auth/register', data);
    return response.data;
  },

  verifyOtp: async (data: VerifyOtpRequest) => {
    const response = await apiClient.post('/api/Auth/verify-otp', data);
    return response.data;
  },

  resendOtp: async (data: ResendOtpRequest) => {
    const response = await apiClient.post('/api/Auth/resend-otp', data);
    return response.data;
  },

  logout: async (refreshToken: string) => {
    const response = await apiClient.post('/api/Auth/logout', { refreshToken });
    return response.data;
  },

  googleLogin: async (idToken: string) => {
    const response = await apiClient.post('/api/Auth/google', { idToken });
    return response.data;
  },

  facebookLogin: async (accessToken: string) => {
    const response = await apiClient.post('/api/Auth/facebook', { accessToken });
    return response.data;
  },
};
