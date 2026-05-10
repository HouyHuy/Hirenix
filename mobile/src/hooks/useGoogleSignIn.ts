/**
 * useGoogleSignIn Hook
 * Handles Google OAuth authentication flow using expo-auth-session
 */
import { useState, useEffect } from 'react';
import { Platform } from 'react-native';
import * as Google from 'expo-auth-session/providers/google';
import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import { GOOGLE_CONFIG } from '../config/googleAuth';
import { authApi } from '../api/authApi';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';

// Required for web browser to close properly after authentication
WebBrowser.maybeCompleteAuthSession();

export const useGoogleSignIn = () => {
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const { showToast } = useToast();

  // Generate redirect URI
  // Auto-detect platform and use appropriate URI
  const redirectUri = AuthSession.makeRedirectUri();

  // Log redirect URI for debugging
  console.log('🔍 Google OAuth Redirect URI:', redirectUri);
  console.log('🔍 Platform:', Platform.OS);

  // Configure Google OAuth request
  const [request, response, promptAsync] = Google.useIdTokenAuthRequest({
    clientId: GOOGLE_CONFIG.webClientId,
    iosClientId: GOOGLE_CONFIG.iosClientId || undefined,
    androidClientId: GOOGLE_CONFIG.androidClientId || undefined,
    scopes: GOOGLE_CONFIG.scopes,
    redirectUri: redirectUri,
  });

  // Handle OAuth response
  useEffect(() => {
    if (response?.type === 'success') {
      const { id_token } = response.params;
      handleGoogleLogin(id_token);
    } else if (response?.type === 'error') {
      showToast('Đăng nhập Google thất bại. Vui lòng thử lại.', 'error');
      setLoading(false);
    } else if (response?.type === 'cancel') {
      setLoading(false);
    }
  }, [response]);

  const handleGoogleLogin = async (idToken: string) => {
    try {
      setLoading(true);
      
      // Send ID token to backend
      const result = await authApi.googleLogin(idToken);
      
      if (result.success && result.data) {
        // Save tokens and user info
        await login(
          result.data.accessToken,
          result.data.refreshToken,
          {
            userId: result.data.userId,
            email: result.data.email,
            phone: result.data.phone,
          }
        );
        
        showToast('Đăng nhập Google thành công!', 'success');
        return true;
      } else {
        showToast(result.message || 'Đăng nhập thất bại.', 'error');
        return false;
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại.';
      showToast(msg, 'error');
      return false;
    } finally {
      setLoading(false);
    }
  };

  const signInWithGoogle = async () => {
    if (!request) {
      showToast('Đang khởi tạo Google Sign-In...', 'info');
      return;
    }
    
    setLoading(true);
    try {
      await promptAsync();
    } catch (error) {
      showToast('Không thể mở Google Sign-In.', 'error');
      setLoading(false);
    }
  };

  return {
    signInWithGoogle,
    loading,
    isReady: !!request,
  };
};
