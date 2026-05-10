/**
 * useFacebookSignIn Hook
 * Handles Facebook OAuth authentication flow using expo-auth-session
 */
import { useState, useEffect } from 'react';
import { Platform } from 'react-native';
import * as AuthSession from 'expo-auth-session';
import * as WebBrowser from 'expo-web-browser';
import { FACEBOOK_CONFIG } from '../config/facebookAuth';
import { authApi } from '../api/authApi';
import { useAuth } from '../contexts/AuthContext';
import { useToast } from '../contexts/ToastContext';

// Required for web browser to close properly after authentication
WebBrowser.maybeCompleteAuthSession();

export const useFacebookSignIn = () => {
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const { showToast } = useToast();

  // Generate redirect URI - auto-detect platform
  const redirectUri = AuthSession.makeRedirectUri();

  // Log redirect URI for debugging
  console.log('🔍 Facebook OAuth Redirect URI:', redirectUri);
  console.log('🔍 Platform:', Platform.OS);

  // Facebook OAuth discovery
  const discovery = {
    authorizationEndpoint: 'https://www.facebook.com/v18.0/dialog/oauth',
    tokenEndpoint: 'https://graph.facebook.com/v18.0/oauth/access_token',
  };

  // Configure Facebook OAuth request
  const [request, response, promptAsync] = AuthSession.useAuthRequest(
    {
      clientId: FACEBOOK_CONFIG.appId,
      scopes: FACEBOOK_CONFIG.scopes,
      redirectUri: redirectUri,
      responseType: AuthSession.ResponseType.Token,
    },
    discovery
  );

  // Handle OAuth response
  useEffect(() => {
    if (response?.type === 'success') {
      const { access_token } = response.params;
      handleFacebookLogin(access_token);
    } else if (response?.type === 'error') {
      showToast('Đăng nhập Facebook thất bại. Vui lòng thử lại.', 'error');
      setLoading(false);
    } else if (response?.type === 'cancel') {
      setLoading(false);
    }
  }, [response]);

  const handleFacebookLogin = async (accessToken: string) => {
    try {
      setLoading(true);
      
      // Send access token to backend
      const result = await authApi.facebookLogin(accessToken);
      
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
        
        showToast('Đăng nhập Facebook thành công!', 'success');
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

  const signInWithFacebook = async () => {
    if (!request) {
      showToast('Đang khởi tạo Facebook Sign-In...', 'info');
      return;
    }
    
    setLoading(true);
    try {
      await promptAsync();
    } catch (error) {
      showToast('Không thể mở Facebook Sign-In.', 'error');
      setLoading(false);
    }
  };

  return {
    signInWithFacebook,
    loading,
    isReady: !!request,
  };
};
