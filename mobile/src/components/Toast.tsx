/**
 * Toast Component — Hirenix
 * Thông báo dạng banner với animation, hỗ trợ 4 loại: success, error, warning, info
 */
import React, { useEffect, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  Animated,
  TouchableOpacity,
  Dimensions,
  Modal,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../constants/theme';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

interface ToastProps {
  message: string;
  type: ToastType;
  visible: boolean;
  onDismiss: () => void;
  duration?: number;
}

const TOAST_CONFIG = {
  success: {
    icon: 'checkmark-circle' as const,
    iconColor: Colors.accentTeal,
    bgColor: '#ECFDF5',
    borderColor: '#A7F3D0',
    textColor: '#065F46',
  },
  error: {
    icon: 'close-circle' as const,
    iconColor: Colors.dangerRed,
    bgColor: '#FEF2F2',
    borderColor: '#FECACA',
    textColor: '#991B1B',
  },
  warning: {
    icon: 'warning' as const,
    iconColor: Colors.warningAmber,
    bgColor: '#FFFBEB',
    borderColor: '#FDE68A',
    textColor: '#92400E',
  },
  info: {
    icon: 'information-circle' as const,
    iconColor: Colors.primaryBlue,
    bgColor: '#EFF6FF',
    borderColor: '#BFDBFE',
    textColor: '#1D4ED8',
  },
};

export const Toast: React.FC<ToastProps> = ({
  message,
  type,
  visible,
  onDismiss,
  duration = 3500,
}) => {
  const insets = useSafeAreaInsets();
  const translateY = useRef(new Animated.Value(100)).current;
  const opacity = useRef(new Animated.Value(0)).current;

  const config = TOAST_CONFIG[type];

  useEffect(() => {
    if (visible) {
      // Slide up + fade in
      Animated.parallel([
        Animated.spring(translateY, {
          toValue: 0,
          useNativeDriver: true,
          tension: 65,
          friction: 8,
        }),
        Animated.timing(opacity, {
          toValue: 1,
          duration: 200,
          useNativeDriver: true,
        }),
      ]).start();

      // Auto dismiss
      const timer = setTimeout(() => {
        handleDismiss();
      }, duration);

      return () => clearTimeout(timer);
    } else {
      // Slide down + fade out
      Animated.parallel([
        Animated.timing(translateY, {
          toValue: 100,
          duration: 250,
          useNativeDriver: true,
        }),
        Animated.timing(opacity, {
          toValue: 0,
          duration: 200,
          useNativeDriver: true,
        }),
      ]).start();
    }
  }, [visible]);

  const handleDismiss = () => {
    Animated.parallel([
      Animated.timing(translateY, {
        toValue: 100,
        duration: 250,
        useNativeDriver: true,
      }),
      Animated.timing(opacity, {
        toValue: 0,
        duration: 200,
        useNativeDriver: true,
      }),
    ]).start(() => {
      onDismiss();
    });
  };

  if (!visible) return null;

  console.log('🎨 Toast rendering with config:', config);
  console.log('🎨 Toast position - bottom:', Math.max(insets.bottom, Spacing.base));

  return (
    <Animated.View
      style={[
        styles.container,
        {
          bottom: Math.max(insets.bottom, Spacing.base),
          transform: [{ translateY }],
          opacity,
        },
      ]}
      pointerEvents="box-none"
    >
      <TouchableOpacity
        activeOpacity={0.9}
        onPress={handleDismiss}
        style={[
          styles.toast,
          {
            backgroundColor: config.bgColor,
            borderColor: config.borderColor,
          },
        ]}
      >
        <View style={styles.iconWrap}>
          <Ionicons name={config.icon} size={24} color={config.iconColor} />
        </View>
        <Text
          style={[styles.message, { color: config.textColor }]}
          numberOfLines={3}
        >
          {message}
        </Text>
        <TouchableOpacity
          onPress={handleDismiss}
          style={styles.closeBtn}
          hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}
        >
          <Ionicons name="close" size={20} color={config.textColor} />
        </TouchableOpacity>
      </TouchableOpacity>
    </Animated.View>
  );
};

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: 0,
    paddingHorizontal: Spacing.base,
    zIndex: 999999,
    elevation: 999999,
  },
  toast: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: Spacing.md,
    paddingHorizontal: Spacing.base,
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    ...Shadows.elevation2,
  },
  iconWrap: {
    marginRight: Spacing.sm,
  },
  message: {
    flex: 1,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
    lineHeight: Typography.lineHeight.bodyMd,
  },
  closeBtn: {
    marginLeft: Spacing.sm,
    padding: Spacing.xs,
  },
});
