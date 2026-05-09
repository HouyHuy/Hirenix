import React, { useState } from 'react';
import {
  View,
  TextInput as RNTextInput,
  Text,
  StyleSheet,
  TouchableOpacity,
  TextInputProps as RNTextInputProps,
  ViewStyle,
  Platform,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Colors, Typography, BorderRadius, InputHeight, Spacing } from '../constants/theme';

interface TextInputProps extends RNTextInputProps {
  label?: string;
  error?: string;
  helperText?: string;
  containerStyle?: ViewStyle;
  isPassword?: boolean;
  leftIcon?: keyof typeof Ionicons.glyphMap;
}

export const TextInput: React.FC<TextInputProps> = ({
  label,
  error,
  helperText,
  containerStyle,
  isPassword = false,
  leftIcon,
  style,
  ...rest
}) => {
  const [isFocused, setIsFocused] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const borderColor = error
    ? Colors.dangerRed
    : isFocused
    ? Colors.primaryBlue
    : Colors.gray200;

  const borderWidth = isFocused ? 2 : 1;

  return (
    <View style={[styles.container, containerStyle]}>
      {label && <Text style={styles.label}>{label}</Text>}
      <View
        style={[
          styles.inputWrapper,
          {
            borderColor,
            borderWidth,
          },
        ]}
      >
        {leftIcon && (
          <Ionicons
            name={leftIcon}
            size={20}
            color={isFocused ? Colors.primaryBlue : Colors.gray400}
            style={styles.leftIcon}
          />
        )}
        <RNTextInput
          style={[styles.input, leftIcon && styles.inputWithIcon, style]}
          placeholderTextColor={Colors.gray400}
          {...rest}
          onFocus={(e) => {
            setIsFocused(true);
            if (rest.onFocus) rest.onFocus(e);
          }}
          onBlur={(e) => {
            setIsFocused(false);
            if (rest.onBlur) rest.onBlur(e);
          }}
          secureTextEntry={isPassword && !showPassword}
        />
        {isPassword && (
          <TouchableOpacity
            onPress={() => setShowPassword(!showPassword)}
            style={styles.eyeButton}
            hitSlop={{ top: 10, bottom: 10, left: 10, right: 10 }}
          >
            <Ionicons
              name={showPassword ? 'eye-off-outline' : 'eye-outline'}
              size={22}
              color={Colors.gray400}
            />
          </TouchableOpacity>
        )}
      </View>
      {error && <Text style={styles.errorText}>{error}</Text>}
      {helperText && !error && <Text style={styles.helperText}>{helperText}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    marginBottom: Spacing.base,
  },
  label: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    lineHeight: Typography.lineHeight.bodySm,
    color: Colors.gray800,
    marginBottom: 6,
  },
  inputWrapper: {
    height: InputHeight.text,
    borderRadius: BorderRadius.md,
    backgroundColor: Colors.white,
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: Spacing.md,
  },
  leftIcon: {
    marginRight: Spacing.sm,
  },
  input: {
    flex: 1,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyLg,
    color: Colors.gray900,
    paddingVertical: Platform.select({ web: 0, default: Spacing.sm }),
    ...(Platform.select({ web: { outlineStyle: 'none' }, default: {} }) as any),
  },
  inputWithIcon: {},
  eyeButton: {
    paddingLeft: Spacing.sm,
  },
  errorText: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    lineHeight: Typography.lineHeight.caption,
    color: Colors.dangerRed,
    marginTop: 4,
  },
  helperText: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    lineHeight: Typography.lineHeight.caption,
    color: Colors.gray400,
    marginTop: 4,
  },
});
