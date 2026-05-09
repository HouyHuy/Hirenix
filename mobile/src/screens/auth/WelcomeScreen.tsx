/**
 * WelcomeScreen — Hirenix
 * Chọn vai trò: Ứng viên / Nhà tuyển dụng
 */
import React from 'react';
import {
  View, Text, StyleSheet, TouchableOpacity, StatusBar, Dimensions,
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';

const { width } = Dimensions.get('window');

interface WelcomeScreenProps {
  onLogin: () => void;
  onRegister: () => void;
}

export const WelcomeScreen: React.FC<WelcomeScreenProps> = ({ onLogin, onRegister }) => {
  return (
    <View style={styles.container}>
      <StatusBar barStyle="dark-content" backgroundColor={Colors.gray50} />

      {/* Header */}
      <View style={styles.header}>
        <View style={styles.logoRow}>
          <View style={styles.miniLogo}>
            <Text style={styles.miniLogoText}>H</Text>
          </View>
          <Text style={styles.brandName}>Hirenix</Text>
        </View>
        <Text style={styles.headerTitle}>Chào mừng bạn!</Text>
        <Text style={styles.headerSubtitle}>
          Nền tảng kết nối ứng viên và nhà tuyển dụng hàng đầu Việt Nam
        </Text>
      </View>

      {/* Role Cards */}
      <View style={styles.cardsContainer}>
        {/* Candidate Card */}
        <TouchableOpacity style={styles.roleCard} onPress={onRegister} activeOpacity={0.9}>
          <LinearGradient
            colors={['#EFF6FF', '#DBEAFE']}
            style={styles.cardGradient}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
          >
            <View style={[styles.cardIconWrap, { backgroundColor: Colors.primaryBlue }]}>
              <Ionicons name="person-outline" size={28} color={Colors.white} />
            </View>
            <Text style={styles.cardTitle}>Tôi là Ứng viên</Text>
            <Text style={styles.cardDesc}>
              Tìm việc làm, nộp đơn ứng tuyển và kết nối với nhà tuyển dụng
            </Text>
            <View style={styles.cardArrow}>
              <Ionicons name="arrow-forward-circle" size={32} color={Colors.primaryBlue} />
            </View>
          </LinearGradient>
        </TouchableOpacity>

        {/* Employer Card */}
        <TouchableOpacity style={styles.roleCard} onPress={onRegister} activeOpacity={0.9}>
          <LinearGradient
            colors={['#ECFDF5', '#D1FAE5']}
            style={styles.cardGradient}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 1 }}
          >
            <View style={[styles.cardIconWrap, { backgroundColor: Colors.accentTeal }]}>
              <Ionicons name="business-outline" size={28} color={Colors.white} />
            </View>
            <Text style={styles.cardTitle}>Tôi là Nhà tuyển dụng</Text>
            <Text style={styles.cardDesc}>
              Đăng tin tuyển dụng, quản lý ứng viên và xây dựng đội ngũ
            </Text>
            <View style={styles.cardArrow}>
              <Ionicons name="arrow-forward-circle" size={32} color={Colors.accentTeal} />
            </View>
          </LinearGradient>
        </TouchableOpacity>
      </View>

      {/* Bottom */}
      <View style={styles.bottomArea}>
        <View style={styles.dividerRow}>
          <View style={styles.dividerLine} />
          <Text style={styles.dividerText}>hoặc</Text>
          <View style={styles.dividerLine} />
        </View>
        <TouchableOpacity style={styles.loginLink} onPress={onLogin} activeOpacity={0.7}>
          <Text style={styles.loginText}>
            Đã có tài khoản?{' '}
            <Text style={styles.loginTextBold}>Đăng nhập</Text>
          </Text>
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  header: { paddingTop: 72, paddingHorizontal: Spacing.xl, paddingBottom: Spacing.xl },
  logoRow: { flexDirection: 'row', alignItems: 'center', marginBottom: Spacing.xl, gap: Spacing.sm },
  miniLogo: {
    width: 36, height: 36, borderRadius: 10,
    backgroundColor: Colors.primaryBlue, alignItems: 'center', justifyContent: 'center',
  },
  miniLogoText: { fontFamily: Typography.fontFamily.bold, fontSize: 20, color: Colors.white },
  brandName: { fontFamily: Typography.fontFamily.bold, fontSize: Typography.size.headingLg, color: Colors.gray900 },
  headerTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.displayLg,
    lineHeight: Typography.lineHeight.displayLg,
    color: Colors.gray900, marginBottom: Spacing.sm,
  },
  headerSubtitle: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyLg,
    lineHeight: Typography.lineHeight.bodyLg,
    color: Colors.gray600,
  },
  cardsContainer: { paddingHorizontal: Spacing.base, gap: Spacing.base },
  roleCard: {
    borderRadius: BorderRadius.lg,
    overflow: 'hidden',
    ...Shadows.elevation2,
  },
  cardGradient: { padding: Spacing.xl, borderRadius: BorderRadius.lg },
  cardIconWrap: {
    width: 52, height: 52, borderRadius: BorderRadius.md,
    alignItems: 'center', justifyContent: 'center', marginBottom: Spacing.md,
  },
  cardTitle: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingMd,
    lineHeight: Typography.lineHeight.headingMd,
    color: Colors.gray900, marginBottom: Spacing.xs,
  },
  cardDesc: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    lineHeight: Typography.lineHeight.bodyMd,
    color: Colors.gray600, marginBottom: Spacing.sm,
  },
  cardArrow: { position: 'absolute', bottom: Spacing.xl, right: Spacing.xl },
  bottomArea: { flex: 1, justifyContent: 'flex-end', paddingHorizontal: Spacing.base, paddingBottom: Spacing['4xl'] },
  dividerRow: { flexDirection: 'row', alignItems: 'center', marginBottom: Spacing.lg },
  dividerLine: { flex: 1, height: 1, backgroundColor: Colors.gray200 },
  dividerText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodySm,
    color: Colors.gray400, marginHorizontal: Spacing.md,
  },
  loginLink: { alignItems: 'center', paddingVertical: Spacing.sm },
  loginText: {
    fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  loginTextBold: { fontFamily: Typography.fontFamily.semibold, color: Colors.primaryBlue },
});
