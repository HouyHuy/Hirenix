import React from 'react';
import {
  TouchableOpacity,
  Text,
  StyleSheet,
  ActivityIndicator,
  ViewStyle,
  TextStyle,
} from 'react-native';
import { Colors, Typography, BorderRadius, ButtonHeight, Spacing } from '../constants/theme';

type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger';

interface ButtonProps {
  title: string;
  onPress: () => void;
  variant?: ButtonVariant;
  disabled?: boolean;
  loading?: boolean;
  fullWidth?: boolean;
  style?: ViewStyle;
  textStyle?: TextStyle;
  icon?: React.ReactNode;
}

export const Button: React.FC<ButtonProps> = ({
  title,
  onPress,
  variant = 'primary',
  disabled = false,
  loading = false,
  fullWidth = true,
  style,
  textStyle,
  icon,
}) => {
  const isDisabled = disabled || loading;

  return (
    <TouchableOpacity
      onPress={onPress}
      disabled={isDisabled}
      activeOpacity={0.85}
      style={[
        styles.base,
        styles[variant],
        fullWidth && styles.fullWidth,
        isDisabled && styles.disabled,
        style,
      ]}
    >
      {loading ? (
        <ActivityIndicator
          color={variant === 'primary' || variant === 'danger' ? Colors.white : Colors.primaryBlue}
          size="small"
        />
      ) : (
        <>
          {icon}
          <Text
            style={[
              styles.text,
              styles[`${variant}Text` as keyof typeof styles] as TextStyle,
              isDisabled && styles.disabledText,
              textStyle,
            ]}
          >
            {title}
          </Text>
        </>
      )}
    </TouchableOpacity>
  );
};

const styles = StyleSheet.create({
  base: {
    height: ButtonHeight.primary,
    borderRadius: BorderRadius.md,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: Spacing.sm,
    paddingHorizontal: Spacing.xl,
  },
  fullWidth: {
    width: '100%',
  },
  primary: {
    backgroundColor: Colors.primaryBlue,
  },
  primaryText: {
    color: Colors.white,
  },
  secondary: {
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: Colors.primaryBlue,
  },
  secondaryText: {
    color: Colors.primaryBlue,
  },
  ghost: {
    backgroundColor: 'transparent',
    height: ButtonHeight.ghost,
  },
  ghostText: {
    color: Colors.primaryBlue,
  },
  danger: {
    backgroundColor: Colors.dangerBg,
    borderWidth: 1,
    borderColor: Colors.dangerBorder,
  },
  dangerText: {
    color: Colors.dangerText,
  },
  disabled: {
    backgroundColor: Colors.gray400,
    borderWidth: 0,
    opacity: 0.7,
  },
  disabledText: {
    color: Colors.white,
  },
  text: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.labelLg,
    lineHeight: Typography.lineHeight.labelLg,
  },
});
