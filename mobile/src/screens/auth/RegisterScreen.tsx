/**
 * RegisterScreen — Hirenix
 * Đăng ký 3 bước wizard:
 *  Step 1: Email + Mật khẩu
 *  Step 2: Thông tin cá nhân
 *  Step 3: Kỹ năng / Hoàn tất
 */
import React, { useState, useRef, useCallback, useEffect } from 'react';
import {
  View, Text, StyleSheet, ScrollView, TouchableOpacity,
  StatusBar, KeyboardAvoidingView, Platform,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, Typography, Spacing, BorderRadius } from '../../constants/theme';
import { Button } from '../../components/Button';
import { TextInput } from '../../components/TextInput';
import { authApi } from '../../api/authApi';
import { useToast } from '../../contexts/ToastContext';

interface RegisterScreenProps {
  onBack: () => void;
  onRegister: (email: string) => void;
  onLogin: () => void;
}

const TOTAL_STEPS = 3;

const SKILL_OPTIONS = [
  'JavaScript', 'TypeScript', 'React', 'React Native', 'Node.js',
  'Python', 'Java', 'C#', '.NET', 'SQL', 'Git', 'Docker',
  'AWS', 'Firebase', 'Figma', 'UI/UX', 'Marketing', 'Sales',
];

export const RegisterScreen: React.FC<RegisterScreenProps> = ({
  onBack, onRegister, onLogin,
}) => {
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const insets = useSafeAreaInsets();
  const { showToast } = useToast();

  // Step 1
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const [emailCheckMessage, setEmailCheckMessage] = useState<{ text: string; type: 'success' | 'error' | 'warning' } | null>(null);
  const [isCheckingEmail, setIsCheckingEmail] = useState(false);
  const [showUnverifiedOptions, setShowUnverifiedOptions] = useState(false);
  
  // Cache to prevent duplicate API calls
  const lastCheckedEmailRef = useRef<string>('');
  const emailCheckTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const isCheckingRef = useRef<boolean>(false);

  // Use useEffect to handle email checking with proper debouncing
  useEffect(() => {
    // Clear any pending timeout
    if (emailCheckTimeoutRef.current) {
      clearTimeout(emailCheckTimeoutRef.current);
    }

    // Reset message when email changes
    setEmailCheckMessage(null);

    // Only check if email looks valid
    if (email.trim() && /\S+@\S+\.\S+/.test(email)) {
      // Skip if already checked this email
      if (lastCheckedEmailRef.current === email) {
        return;
      }

      // Debounce the API call by 1000ms
      emailCheckTimeoutRef.current = setTimeout(async () => {
        // Double-check we're not already checking
        if (isCheckingRef.current) {
          return;
        }

        isCheckingRef.current = true;
        setIsCheckingEmail(true);
        
        try {
          const res = await authApi.checkEmail(email);
          
          // Cache this email as checked
          lastCheckedEmailRef.current = email;
          
          if (res.success && res.data) {
            // Check if data is an object with exists/isVerified properties
            if (typeof res.data === 'object' && 'exists' in res.data) {
              if (res.data.exists && res.data.isVerified) {
                // Email đã tồn tại và đã verify
                setEmailCheckMessage({ text: 'Email này đã có người sử dụng', type: 'error' });
                setShowUnverifiedOptions(false);
              } else if (res.data.exists && !res.data.isVerified) {
                // Email đã tồn tại nhưng chưa verify
                setEmailCheckMessage({ text: 'Email này đã đăng ký nhưng chưa xác thực', type: 'warning' });
                setShowUnverifiedOptions(true);
              } else {
                // Email available
                setEmailCheckMessage({ text: 'Email khả dụng ✓', type: 'success' });
                setShowUnverifiedOptions(false);
              }
            } else if (res.data === true) {
              // Old API format: data is boolean true = email exists and verified
              setEmailCheckMessage({ text: 'Email này đã có người sử dụng', type: 'error' });
              setShowUnverifiedOptions(false);
            } else {
              // data is false or null = email available
              setEmailCheckMessage({ text: 'Email khả dụng ✓', type: 'success' });
              setShowUnverifiedOptions(false);
            }
          } else {
            setEmailCheckMessage({ text: 'Email khả dụng ✓', type: 'success' });
            setShowUnverifiedOptions(false);
          }
        } catch (error: any) {
          console.error('Error checking email:', error.message);
          setEmailCheckMessage({ text: 'Không thể kiểm tra email', type: 'error' });
          setShowUnverifiedOptions(false);
        } finally {
          isCheckingRef.current = false;
          setIsCheckingEmail(false);
        }
      }, 1000);
    }

    // Cleanup function
    return () => {
      if (emailCheckTimeoutRef.current) {
        clearTimeout(emailCheckTimeoutRef.current);
      }
    };
  }, [email]); // Only re-run when email changes

  const handleEmailChange = (text: string) => {
    setEmail(text);
    // Only reset cache if the new email is different from what we last checked
    // This prevents unnecessary API calls
    if (lastCheckedEmailRef.current !== text) {
      lastCheckedEmailRef.current = '';
    }
    setErrors(prev => {
      const newErr = { ...prev };
      delete newErr.email;
      return newErr;
    });
  };

  // Step 2
  const [fullName, setFullName] = useState('');
  const [phone, setPhone] = useState('');

  // Step 3
  const [selectedSkills, setSelectedSkills] = useState<string[]>([]);

  const [errors, setErrors] = useState<Record<string, string>>({});

  // ─── Password strength ───
  const getPasswordStrength = (pwd: string) => {
    if (!pwd) return { level: 0, label: '', color: Colors.gray200 };
    let score = 0;
    if (pwd.length >= 6) score++;
    if (pwd.length >= 8) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;

    if (score <= 2) return { level: score, label: 'Yếu', color: Colors.dangerRed };
    if (score <= 3) return { level: score, label: 'Trung bình', color: Colors.warningAmber };
    return { level: score, label: 'Mạnh', color: Colors.accentTeal };
  };

  const pwdStrength = getPasswordStrength(password);

  // ─── Validation ───
  const validateStep = () => {
    const e: Record<string, string> = {};
    if (step === 1) {
      if (!email.trim()) e.email = 'Vui lòng nhập email';
      else if (!/\S+@\S+\.\S+/.test(email)) e.email = 'Email không hợp lệ';
      else if (emailCheckMessage?.type === 'error') {
        e.email = emailCheckMessage.text;
      }
      
      if (!password) e.password = 'Vui lòng nhập mật khẩu';
      else if (password.length < 6) e.password = 'Tối thiểu 6 ký tự';
      else if (!/[A-Z]/.test(password)) e.password = 'Cần ít nhất 1 chữ hoa';
      else if (!/[0-9]/.test(password)) e.password = 'Cần ít nhất 1 chữ số';
      else if (!/[^A-Za-z0-9]/.test(password)) e.password = 'Cần ít nhất 1 ký tự đặc biệt';
      if (password !== confirmPassword) e.confirmPassword = 'Mật khẩu không khớp';
    }
    if (step === 2) {
      if (!fullName.trim()) e.fullName = 'Vui lòng nhập họ tên';
      if (!phone.trim()) e.phone = 'Vui lòng nhập số điện thoại';
      else if (!/^(0|\+84)\d{9,10}$/.test(phone.replace(/\s/g, '')))
        e.phone = 'Số điện thoại không hợp lệ';
    }
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleNext = async () => {
    if (!validateStep()) return;
    if (step < TOTAL_STEPS) {
      setStep(step + 1);
    } else {
      setLoading(true);
      try {
        const response = await authApi.register({
          email: email,
          phone: phone,
          password: password,
          role: 'candidate',
        });
        if (response.success) {
          showToast('Đăng ký thành công! Vui lòng kiểm tra email để nhận mã OTP.', 'success', 4000);
          onRegister(email);
        } else {
          showToast(response.message || 'Không thể tạo tài khoản.', 'error');
        }
      } catch (error: any) {
        const msg = error.response?.data?.message || 'Đã có lỗi xảy ra. Vui lòng thử lại sau.';
        showToast(msg, 'error');
      } finally {
        setLoading(false);
      }
    }
  };

  const handleBack = () => {
    if (step > 1) setStep(step - 1);
    else onBack();
  };

  const toggleSkill = (skill: string) => {
    setSelectedSkills((prev) =>
      prev.includes(skill) ? prev.filter((s) => s !== skill) : [...prev, skill]
    );
  };

  // ─── Handle Resend OTP ───
  const handleResendOtp = async () => {
    setLoading(true);
    try {
      const response = await authApi.resendOtp({ email });
      if (response.success) {
        showToast('Mã OTP đã được gửi lại. Vui lòng kiểm tra email.', 'success');
        onRegister(email); // Navigate to OTP screen
      } else {
        showToast(response.message || 'Không thể gửi lại mã OTP.', 'error');
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || 'Đã có lỗi xảy ra.';
      showToast(msg, 'error');
    } finally {
      setLoading(false);
    }
  };

  // ─── Handle Register Again (will delete old unverified account) ───
  const handleRegisterAgain = () => {
    // Clear the warning and allow user to continue
    setShowUnverifiedOptions(false);
    setEmailCheckMessage({ text: 'Bạn có thể tiếp tục đăng ký', type: 'success' });
    // Backend will automatically delete the old unverified account when registering
  };

  // ─── Step Indicator ───
  const renderStepIndicator = () => (
    <View style={styles.stepIndicator}>
      {Array.from({ length: TOTAL_STEPS }, (_, i) => {
        const stepNum = i + 1;
        const isActive = stepNum === step;
        const isDone = stepNum < step;
        return (
          <React.Fragment key={stepNum}>
            <View style={[
              styles.stepCircle,
              isActive && styles.stepCircleActive,
              isDone && styles.stepCircleDone,
            ]}>
              {isDone ? (
                <Ionicons name="checkmark" size={14} color={Colors.white} />
              ) : (
                <Text style={[
                  styles.stepNum,
                  (isActive || isDone) && styles.stepNumActive,
                ]}>{stepNum}</Text>
              )}
            </View>
            {stepNum < TOTAL_STEPS && (
              <View style={[styles.stepLine, isDone && styles.stepLineDone]} />
            )}
          </React.Fragment>
        );
      })}
    </View>
  );

  // ─── Step Labels ───
  const stepLabels = ['Tài khoản', 'Thông tin', 'Kỹ năng'];

  // ─── Render Steps ───
  const renderStep1 = () => (
    <View style={styles.stepContent}>
      <Text style={styles.stepTitle}>Tạo tài khoản</Text>
      <Text style={styles.stepDesc}>Nhập email và mật khẩu để bắt đầu</Text>

      <View>
        <TextInput
          label="Email" placeholder="example@email.com"
          value={email} 
          onChangeText={handleEmailChange}
          error={errors.email}
          keyboardType="email-address" autoCapitalize="none"
          leftIcon="mail-outline"
        />
        {emailCheckMessage && !errors.email && (
          <View>
            <Text style={[
              styles.emailCheckMessage,
              { 
                color: emailCheckMessage.type === 'success' 
                  ? Colors.accentTeal 
                  : emailCheckMessage.type === 'warning'
                  ? Colors.warningAmber
                  : Colors.dangerRed 
              }
            ]}>
              {emailCheckMessage.text}
            </Text>
            
            {/* Show options for unverified email */}
            {showUnverifiedOptions && (
              <View style={styles.unverifiedOptionsContainer}>
                <Text style={styles.unverifiedOptionsTitle}>Bạn muốn:</Text>
                <View style={styles.unverifiedButtonsRow}>
                  <TouchableOpacity 
                    style={styles.unverifiedButton}
                    onPress={handleResendOtp}
                    activeOpacity={0.7}
                  >
                    <Ionicons name="mail-outline" size={18} color={Colors.primaryBlue} />
                    <Text style={styles.unverifiedButtonText}>Gửi lại mã OTP</Text>
                  </TouchableOpacity>
                  
                  <TouchableOpacity 
                    style={[styles.unverifiedButton, styles.unverifiedButtonSecondary]}
                    onPress={handleRegisterAgain}
                    activeOpacity={0.7}
                  >
                    <Ionicons name="refresh-outline" size={18} color={Colors.gray600} />
                    <Text style={[styles.unverifiedButtonText, styles.unverifiedButtonTextSecondary]}>
                      Đăng ký lại
                    </Text>
                  </TouchableOpacity>
                </View>
              </View>
            )}
          </View>
        )}
      </View>
      <TextInput
        label="Mật khẩu" placeholder="Tối thiểu 6 ký tự"
        value={password} onChangeText={setPassword} error={errors.password}
        isPassword leftIcon="lock-closed-outline"
      />

      {/* Password strength bar */}
      {password.length > 0 && (
        <View style={styles.strengthWrap}>
          <View style={styles.strengthBar}>
            {[1, 2, 3, 4, 5].map((seg) => (
              <View
                key={seg}
                style={[
                  styles.strengthSeg,
                  {
                    backgroundColor:
                      seg <= pwdStrength.level ? pwdStrength.color : Colors.gray200,
                  },
                ]}
              />
            ))}
          </View>
          <Text style={[styles.strengthLabel, { color: pwdStrength.color }]}>
            {pwdStrength.label}
          </Text>
        </View>
      )}

      <TextInput
        label="Xác nhận mật khẩu" placeholder="Nhập lại mật khẩu"
        value={confirmPassword} onChangeText={setConfirmPassword}
        error={errors.confirmPassword}
        isPassword leftIcon="lock-closed-outline"
      />
    </View>
  );

  const renderStep2 = () => (
    <View style={styles.stepContent}>
      <Text style={styles.stepTitle}>Thông tin cá nhân</Text>
      <Text style={styles.stepDesc}>Cho chúng tôi biết thêm về bạn</Text>

      <TextInput
        label="Họ và tên" placeholder="Nguyễn Văn A"
        value={fullName} onChangeText={setFullName} error={errors.fullName}
        leftIcon="person-outline"
      />
      <TextInput
        label="Số điện thoại" placeholder="0912 345 678"
        value={phone} onChangeText={setPhone} error={errors.phone}
        keyboardType="phone-pad" leftIcon="call-outline"
      />
    </View>
  );

  const renderStep3 = () => (
    <View style={styles.stepContent}>
      <Text style={styles.stepTitle}>Kỹ năng của bạn</Text>
      <Text style={styles.stepDesc}>
        Chọn các kỹ năng để nhận gợi ý phù hợp (tùy chọn)
      </Text>

      <View style={styles.skillsGrid}>
        {SKILL_OPTIONS.map((skill) => {
          const isSelected = selectedSkills.includes(skill);
          return (
            <TouchableOpacity
              key={skill}
              style={[styles.skillChip, isSelected && styles.skillChipActive]}
              onPress={() => toggleSkill(skill)}
              activeOpacity={0.7}
            >
              <Text style={[styles.skillChipText, isSelected && styles.skillChipTextActive]}>
                {skill}
              </Text>
              {isSelected && (
                <Ionicons name="checkmark-circle" size={16} color={Colors.primaryBlue} />
              )}
            </TouchableOpacity>
          );
        })}
      </View>

      {selectedSkills.length > 0 && (
        <Text style={styles.selectedCount}>
          Đã chọn {selectedSkills.length} kỹ năng
        </Text>
      )}
    </View>
  );

  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.white} />
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        style={{ flex: 1 }}
      >
        {/* Top bar */}
        <View style={styles.topBar}>
          <TouchableOpacity onPress={handleBack} style={styles.backBtn} activeOpacity={0.7}>
            <Ionicons name="arrow-back" size={24} color={Colors.gray800} />
          </TouchableOpacity>
          <Text style={styles.topBarTitle}>Đăng ký</Text>
          <View style={{ width: 44 }} />
        </View>

        {/* Step indicator */}
        <View style={styles.stepIndicatorWrap}>
          {renderStepIndicator()}
          <View style={styles.stepLabelsRow}>
            {stepLabels.map((label, i) => (
              <Text
                key={label}
                style={[
                  styles.stepLabelText,
                  i + 1 === step && styles.stepLabelActive,
                ]}
              >
                {label}
              </Text>
            ))}
          </View>
        </View>

        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          {step === 1 && renderStep1()}
          {step === 2 && renderStep2()}
          {step === 3 && renderStep3()}
        </ScrollView>

        {/* Bottom */}
        <View style={[styles.bottomArea, { paddingBottom: Math.max(insets.bottom, Spacing.xs) }]}>
          <Button
            title={step === TOTAL_STEPS ? 'Hoàn tất đăng ký' : 'Tiếp theo'}
            onPress={handleNext}
            loading={loading}
          />
          {step === 1 && (
            <TouchableOpacity style={styles.loginLink} onPress={onLogin} activeOpacity={0.7}>
              <Text style={styles.loginLinkText}>
                Đã có tài khoản?{' '}
                <Text style={styles.loginLinkBold}>Đăng nhập</Text>
              </Text>
            </TouchableOpacity>
          )}
        </View>
      </KeyboardAvoidingView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.white },
  topBar: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between',
    paddingTop: 56, paddingHorizontal: Spacing.base, paddingBottom: Spacing.md,
  },
  backBtn: {
    width: 44, height: 44, borderRadius: BorderRadius.md,
    borderWidth: 1, borderColor: Colors.gray200,
    alignItems: 'center', justifyContent: 'center',
  },
  topBarTitle: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
  },
  stepIndicatorWrap: { paddingHorizontal: Spacing['2xl'], paddingVertical: Spacing.lg },
  stepIndicator: {
    flexDirection: 'row', alignItems: 'center', justifyContent: 'center',
  },
  stepCircle: {
    width: 32, height: 32, borderRadius: 16,
    borderWidth: 2, borderColor: Colors.gray200,
    alignItems: 'center', justifyContent: 'center',
    backgroundColor: Colors.white,
  },
  stepCircleActive: { borderColor: Colors.primaryBlue, backgroundColor: Colors.primaryBlue },
  stepCircleDone: { borderColor: Colors.accentTeal, backgroundColor: Colors.accentTeal },
  stepNum: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.caption, color: Colors.gray400,
  },
  stepNumActive: { color: Colors.white },
  stepLine: { width: 48, height: 2, backgroundColor: Colors.gray200, marginHorizontal: Spacing.sm },
  stepLineDone: { backgroundColor: Colors.accentTeal },
  stepLabelsRow: {
    flexDirection: 'row', justifyContent: 'space-between',
    paddingHorizontal: Spacing.sm, marginTop: Spacing.sm,
  },
  stepLabelText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.caption,
    color: Colors.gray400, flex: 1, textAlign: 'center',
  },
  stepLabelActive: { fontFamily: Typography.fontFamily.medium, color: Colors.primaryBlue },
  scrollContent: { paddingBottom: Spacing.xl },
  stepContent: { paddingHorizontal: Spacing.base },
  stepTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    lineHeight: Typography.lineHeight.headingLg,
    color: Colors.gray900, marginBottom: Spacing.xs,
  },
  stepDesc: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd, color: Colors.gray600,
    marginBottom: Spacing.xl,
  },
  strengthWrap: {
    flexDirection: 'row', alignItems: 'center', gap: Spacing.sm,
    marginTop: -Spacing.sm, marginBottom: Spacing.base,
  },
  strengthBar: { flex: 1, flexDirection: 'row', gap: 3 },
  strengthSeg: { flex: 1, height: 4, borderRadius: 2 },
  strengthLabel: {
    fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.caption,
  },
  skillsGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.sm },
  skillChip: {
    paddingVertical: 8, paddingHorizontal: 14, borderRadius: BorderRadius.xl,
    borderWidth: 1, borderColor: Colors.gray200, backgroundColor: Colors.white,
    flexDirection: 'row', alignItems: 'center', gap: 6,
  },
  skillChipActive: {
    borderColor: Colors.infoBorder, backgroundColor: Colors.infoBg,
  },
  skillChipText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  skillChipTextActive: {
    fontFamily: Typography.fontFamily.medium, color: Colors.infoText,
  },
  selectedCount: {
    fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.bodySm,
    color: Colors.primaryBlue, marginTop: Spacing.base, textAlign: 'center',
  },
  bottomArea: {
    paddingHorizontal: Spacing.base,
    paddingTop: Spacing.md,
    borderTopWidth: 1, borderTopColor: Colors.gray100,
    backgroundColor: Colors.white,
  },
  loginLink: { alignItems: 'center', marginTop: Spacing.md, paddingVertical: Spacing.xs },
  loginLinkText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  loginLinkBold: { fontFamily: Typography.fontFamily.semibold, color: Colors.primaryBlue },
  emailCheckMessage: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    marginTop: -Spacing.sm,
    marginBottom: Spacing.base,
    marginLeft: Spacing.xs,
  },
  unverifiedOptionsContainer: {
    marginTop: Spacing.sm,
    marginBottom: Spacing.base,
    padding: Spacing.md,
    backgroundColor: Colors.warningBg,
    borderRadius: BorderRadius.md,
    borderWidth: 1,
    borderColor: Colors.warningBorder,
  },
  unverifiedOptionsTitle: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
    marginBottom: Spacing.sm,
  },
  unverifiedButtonsRow: {
    flexDirection: 'row',
    gap: Spacing.sm,
  },
  unverifiedButton: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.xs,
    paddingVertical: Spacing.sm,
    paddingHorizontal: Spacing.md,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.md,
    borderWidth: 1.5,
    borderColor: Colors.primaryBlue,
  },
  unverifiedButtonSecondary: {
    borderColor: Colors.gray200,
  },
  unverifiedButtonText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.primaryBlue,
  },
  unverifiedButtonTextSecondary: {
    color: Colors.gray600,
  },
});
