/**
 * LoginScreen — Hirenix
 * Đăng nhập: Email/Phone + Password, OAuth, ForgotPassword link
 */
import React, { useState, useRef } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  StatusBar, KeyboardAvoidingView, Platform, Animated,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';
import { Button } from '../../components/Button';
import { TextInput } from '../../components/TextInput';
import { authApi } from '../../api/authApi';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';
import { useGoogleSignIn } from '../../hooks/useGoogleSignIn';
import { useFacebookSignIn } from '../../hooks/useFacebookSignIn';

interface LoginScreenProps {
  onBack: () => void;
  onLogin: () => void;
  onForgotPassword: () => void;
  onRegister: () => void;
}

export const LoginScreen: React.FC<LoginScreenProps> = ({
  onBack, onLogin, onForgotPassword, onRegister,
}) => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState<{ email?: string; password?: string }>({});

  const validate = () => {
    const newErrors: typeof errors = {};
    if (!email.trim()) newErrors.email = 'Vui lòng nhập email hoặc số điện thoại';
    if (!password) newErrors.password = 'Vui lòng nhập mật khẩu';
    else if (password.length < 6) newErrors.password = 'Mật khẩu tối thiểu 6 ký tự';
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const { login } = useAuth();
  const insets = useSafeAreaInsets();
  const { showToast } = useToast();
  const { signInWithGoogle, loading: googleLoading } = useGoogleSignIn();
  const { signInWithFacebook, loading: facebookLoading } = useFacebookSignIn();

  const handleLogin = async () => {
    if (!validate()) return;
    setLoading(true);
    try {
      const response = await authApi.login({
        identifier: email,
        password: password,
      });
      if (response.success && response.data) {
        await login(
          response.data.accessToken,
          response.data.refreshToken,
          {
            userId: response.data.userId,
            email: response.data.email,
            phone: response.data.phone,
            role: response.data.role,
          }
        );
        showToast('Đăng nhập thành công!', 'success');
        onLogin();
      } else {
        showToast(response.message || 'Tài khoản hoặc mật khẩu không chính xác.', 'error');
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
      showToast(msg, 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.white} />
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        <ScrollView
          contentContainerStyle={[styles.scrollContent, { paddingBottom: Math.max(insets.bottom, Spacing.xl) }]}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {/* Header */}
          <View style={styles.header}>
            <TouchableOpacity onPress={onBack} style={styles.backBtn} activeOpacity={0.7}>
              <Ionicons name="arrow-back" size={24} color={Colors.gray800} />
            </TouchableOpacity>
            <View style={styles.headerTextWrap}>
              <Text style={styles.title}>Đăng nhập</Text>
              <Text style={styles.subtitle}>
                Chào mừng bạn quay trở lại Hirenix
              </Text>
            </View>
          </View>

          {/* Form */}
          <View style={styles.form}>
            <TextInput
              label="Email hoặc số điện thoại"
              placeholder="example@email.com"
              value={email}
              onChangeText={setEmail}
              error={errors.email}
              keyboardType="email-address"
              autoCapitalize="none"
              leftIcon="mail-outline"
            />
            <TextInput
              label="Mật khẩu"
              placeholder="Nhập mật khẩu"
              value={password}
              onChangeText={setPassword}
              error={errors.password}
              isPassword
              leftIcon="lock-closed-outline"
            />

            {/* Remember me + Forgot */}
            <View style={styles.optionRow}>
              <TouchableOpacity
                style={styles.rememberRow}
                onPress={() => setRememberMe(!rememberMe)}
                activeOpacity={0.7}
              >
                <View style={[styles.checkbox, rememberMe && styles.checkboxActive]}>
                  {rememberMe && <Ionicons name="checkmark" size={14} color={Colors.white} />}
                </View>
                <Text style={styles.rememberText}>Ghi nhớ đăng nhập</Text>
              </TouchableOpacity>
              <TouchableOpacity onPress={onForgotPassword} activeOpacity={0.7}>
                <Text style={styles.forgotText}>Quên mật khẩu?</Text>
              </TouchableOpacity>
            </View>

            {/* Login button */}
            <Button
              title="Đăng nhập"
              onPress={handleLogin}
              loading={loading}
              style={{ marginTop: Spacing.lg }}
            />
          </View>

          {/* Divider */}
          <View style={styles.dividerRow}>
            <View style={styles.dividerLine} />
            <Text style={styles.dividerText}>hoặc đăng nhập với</Text>
            <View style={styles.dividerLine} />
          </View>

          {/* Social login */}
          <View style={styles.socialRow}>
            <TouchableOpacity 
              style={[styles.socialBtn, googleLoading && styles.socialBtnDisabled]} 
              activeOpacity={0.85}
              onPress={signInWithGoogle}
              disabled={googleLoading || loading}
            >
              {googleLoading ? (
                <View style={styles.socialBtnContent}>
                  <Text style={styles.socialBtnText}>Đang xử lý...</Text>
                </View>
              ) : (
                <View style={styles.socialBtnContent}>
                  <Ionicons name="logo-google" size={22} color="#DB4437" />
                  <Text style={styles.socialBtnText}>Google</Text>
                </View>
              )}
            </TouchableOpacity>
            <TouchableOpacity 
              style={[styles.socialBtn, facebookLoading && styles.socialBtnDisabled]} 
              activeOpacity={0.85}
              onPress={signInWithFacebook}
              disabled={facebookLoading || loading}
            >
              {facebookLoading ? (
                <View style={styles.socialBtnContent}>
                  <Text style={styles.socialBtnText}>Đang xử lý...</Text>
                </View>
              ) : (
                <View style={styles.socialBtnContent}>
                  <Ionicons name="logo-facebook" size={22} color="#1877F2" />
                  <Text style={styles.socialBtnText}>Facebook</Text>
                </View>
              )}
            </TouchableOpacity>
          </View>

          {/* Register link */}
          <TouchableOpacity style={styles.registerLink} onPress={onRegister} activeOpacity={0.7}>
            <Text style={styles.registerText}>
              Chưa có tài khoản?{' '}
              <Text style={styles.registerTextBold}>Đăng ký ngay</Text>
            </Text>
          </TouchableOpacity>
        </ScrollView>
      </KeyboardAvoidingView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.white },
  scrollContent: { },
  header: { paddingTop: 56, paddingHorizontal: Spacing.base, paddingBottom: Spacing.lg },
  backBtn: {
    width: 44, height: 44, borderRadius: BorderRadius.md,
    borderWidth: 1, borderColor: Colors.gray200,
    alignItems: 'center', justifyContent: 'center', marginBottom: Spacing.xl,
  },
  headerTextWrap: {},
  title: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.displayMd,
    lineHeight: Typography.lineHeight.displayMd,
    color: Colors.gray900, marginBottom: Spacing.xs,
  },
  subtitle: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    lineHeight: Typography.lineHeight.bodyMd,
    color: Colors.gray600,
  },
  form: { paddingHorizontal: Spacing.base },
  optionRow: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    marginTop: -Spacing.sm,
  },
  rememberRow: { flexDirection: 'row', alignItems: 'center', gap: Spacing.sm },
  checkbox: {
    width: 20, height: 20, borderRadius: 4,
    borderWidth: 1.5, borderColor: Colors.gray200,
    alignItems: 'center', justifyContent: 'center',
  },
  checkboxActive: { backgroundColor: Colors.primaryBlue, borderColor: Colors.primaryBlue },
  rememberText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  forgotText: {
    fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.bodySm,
    color: Colors.primaryBlue,
  },
  dividerRow: {
    flexDirection: 'row', alignItems: 'center',
    paddingHorizontal: Spacing.base, marginVertical: Spacing.xl,
  },
  dividerLine: { flex: 1, height: 1, backgroundColor: Colors.gray200 },
  dividerText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodySm,
    color: Colors.gray400, marginHorizontal: Spacing.md,
  },
  socialRow: {
    flexDirection: 'row', paddingHorizontal: Spacing.base, gap: Spacing.md,
  },
  socialBtn: {
    flex: 1, height: 50, borderRadius: BorderRadius.md,
    borderWidth: 1, borderColor: Colors.gray200,
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
    backgroundColor: Colors.white,
  },
  socialBtnDisabled: {
    opacity: 0.5,
  },
  socialBtnContent: {
    flexDirection: 'row', alignItems: 'center', gap: Spacing.sm,
  },
  socialBtnText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd, color: Colors.gray800,
  },
  registerLink: { alignItems: 'center', marginTop: Spacing['2xl'], paddingVertical: Spacing.sm },
  registerText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  registerTextBold: { fontFamily: Typography.fontFamily.semibold, color: Colors.primaryBlue },
});
