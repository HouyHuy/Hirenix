/**
 * OtpVerificationScreen — Hirenix
 * Màn hình nhập mã OTP để verify email sau khi đăng ký
 */
import React, { useState, useRef, useEffect } from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, TextInput as RNTextInput,
  StatusBar, KeyboardAvoidingView, Platform, ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, Typography, Spacing, BorderRadius } from '../../constants/theme';
import { Button } from '../../components/Button';
import { authApi } from '../../api/authApi';
import { useAuth } from '../../contexts/AuthContext';
import { useToast } from '../../contexts/ToastContext';

interface OtpVerificationScreenProps {
  email: string;
  onBack: () => void;
  onVerified: () => void;
}

const OTP_LENGTH = 6;

export const OtpVerificationScreen: React.FC<OtpVerificationScreenProps> = ({
  email,
  onBack,
  onVerified,
}) => {
  const [otp, setOtp] = useState<string[]>(Array(OTP_LENGTH).fill(''));
  const [loading, setLoading] = useState(false);
  const [resending, setResending] = useState(false);
  const [countdown, setCountdown] = useState(60);
  const [canResend, setCanResend] = useState(false);
  const insets = useSafeAreaInsets();
  const { login } = useAuth();
  const { showToast } = useToast();

  const inputRefs = useRef<RNTextInput[]>([]);

  // Countdown timer for resend
  useEffect(() => {
    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    } else {
      setCanResend(true);
    }
  }, [countdown]);

  const handleOtpChange = (value: string, index: number) => {
    // Chỉ cho phép số
    if (value && !/^\d$/.test(value)) return;

    const newOtp = [...otp];
    newOtp[index] = value;
    setOtp(newOtp);

    // Auto focus next input
    if (value && index < OTP_LENGTH - 1) {
      inputRefs.current[index + 1]?.focus();
    }

    // Auto submit when all filled
    if (value && index === OTP_LENGTH - 1 && newOtp.every(digit => digit)) {
      handleVerify(newOtp.join(''));
    }
  };

  const handleKeyPress = (e: any, index: number) => {
    if (e.nativeEvent.key === 'Backspace' && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handleVerify = async (otpCode?: string) => {
    const code = otpCode || otp.join('');
    
    if (code.length !== OTP_LENGTH) {
      showToast('Vui lòng nhập đủ 6 chữ số', 'error');
      return;
    }

    setLoading(true);
    try {
      const response = await authApi.verifyOtp({ email, otpCode: code });
      if (response.success && response.data) {
        // Save auth tokens
        await login(
          response.data.accessToken,
          response.data.refreshToken,
          {
            userId: response.data.userId,
            email: response.data.email,
            phone: response.data.phone,
          }
        );
        showToast('Xác thực thành công!', 'success');
        onVerified();
      } else {
        showToast(response.message || 'Mã OTP không đúng', 'error');
        // Clear OTP
        setOtp(Array(OTP_LENGTH).fill(''));
        inputRefs.current[0]?.focus();
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Đã có lỗi xảy ra';
      showToast(msg, 'error');
      setOtp(Array(OTP_LENGTH).fill(''));
      inputRefs.current[0]?.focus();
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (!canResend) return;

    setResending(true);
    try {
      const response = await authApi.resendOtp({ email });
      if (response.success) {
        showToast('Đã gửi lại mã OTP', 'success');
        setCountdown(60);
        setCanResend(false);
        setOtp(Array(OTP_LENGTH).fill(''));
        inputRefs.current[0]?.focus();
      } else {
        showToast(response.message || 'Không thể gửi lại OTP', 'error');
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Đã có lỗi xảy ra';
      showToast(msg, 'error');
    } finally {
      setResending(false);
    }
  };

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.white} />
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        {/* Top bar */}
        <View style={[styles.topBar, { paddingTop: insets.top + 16 }]}>
          <TouchableOpacity onPress={onBack} style={styles.backBtn} activeOpacity={0.7}>
            <Ionicons name="arrow-back" size={24} color={Colors.gray800} />
          </TouchableOpacity>
        </View>

        {/* Content */}
        <View style={styles.content}>
          {/* Icon */}
          <View style={styles.iconContainer}>
            <Ionicons name="mail-outline" size={64} color={Colors.primaryBlue} />
          </View>

          {/* Title */}
          <Text style={styles.title}>Xác thực email</Text>
          <Text style={styles.subtitle}>
            Chúng tôi đã gửi mã OTP gồm 6 chữ số đến{'\n'}
            <Text style={styles.email}>{email}</Text>
          </Text>

          {/* OTP Input */}
          <View style={styles.otpContainer}>
            {otp.map((digit, index) => (
              <RNTextInput
                key={index}
                ref={(ref) => { if (ref) inputRefs.current[index] = ref; }}
                style={[
                  styles.otpInput,
                  digit && styles.otpInputFilled,
                ]}
                value={digit}
                onChangeText={(value) => handleOtpChange(value, index)}
                onKeyPress={(e) => handleKeyPress(e, index)}
                keyboardType="number-pad"
                maxLength={1}
                selectTextOnFocus
                autoFocus={index === 0}
              />
            ))}
          </View>

          {/* Resend */}
          <View style={styles.resendContainer}>
            {canResend ? (
              <TouchableOpacity onPress={handleResend} disabled={resending} activeOpacity={0.7}>
                <Text style={styles.resendText}>
                  {resending ? 'Đang gửi...' : 'Gửi lại mã OTP'}
                </Text>
              </TouchableOpacity>
            ) : (
              <Text style={styles.countdownText}>
                Gửi lại sau {countdown}s
              </Text>
            )}
          </View>
        </View>

        {/* Bottom Button */}
        <View style={[styles.bottomArea, { paddingBottom: Math.max(insets.bottom, Spacing.xs) }]}>
          <Button
            title="Xác nhận"
            onPress={() => handleVerify()}
            loading={loading}
            disabled={otp.some(digit => !digit)}
          />
        </View>
      </KeyboardAvoidingView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.white },
  topBar: {
    paddingHorizontal: Spacing.base,
    paddingBottom: Spacing.md,
  },
  backBtn: {
    width: 44,
    height: 44,
    borderRadius: BorderRadius.md,
    borderWidth: 1,
    borderColor: Colors.gray200,
    alignItems: 'center',
    justifyContent: 'center',
  },
  content: {
    flex: 1,
    paddingHorizontal: Spacing.base,
    alignItems: 'center',
    paddingTop: Spacing['2xl'],
  },
  iconContainer: {
    width: 120,
    height: 120,
    borderRadius: 60,
    backgroundColor: Colors.infoBg,
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: Spacing.xl,
  },
  title: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    color: Colors.gray900,
    marginBottom: Spacing.sm,
  },
  subtitle: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
    textAlign: 'center',
    marginBottom: Spacing['2xl'],
  },
  email: {
    fontFamily: Typography.fontFamily.semibold,
    color: Colors.primaryBlue,
  },
  otpContainer: {
    flexDirection: 'row',
    gap: Spacing.sm,
    marginBottom: Spacing.xl,
  },
  otpInput: {
    width: 48,
    height: 56,
    borderWidth: 2,
    borderColor: Colors.gray200,
    borderRadius: BorderRadius.md,
    textAlign: 'center',
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
  },
  otpInputFilled: {
    borderColor: Colors.primaryBlue,
    backgroundColor: Colors.infoBg,
  },
  resendContainer: {
    alignItems: 'center',
  },
  resendText: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodyMd,
    color: Colors.primaryBlue,
  },
  countdownText: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray400,
  },
  bottomArea: {
    paddingHorizontal: Spacing.base,
    paddingTop: Spacing.md,
    borderTopWidth: 1,
    borderTopColor: Colors.gray100,
    backgroundColor: Colors.white,
  },
});
