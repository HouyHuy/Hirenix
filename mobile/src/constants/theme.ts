/**
 * Hirenix Design System — Theme Constants
 * Based on UIUXDesign.md specification
 */

// ─── Brand Colors ───
export const Colors = {
  // Brand
  primaryBlue: '#1A6FBF',
  primaryDark: '#134E8A',
  accentTeal: '#1D9E75',
  warningAmber: '#D97706',
  dangerRed: '#DC2626',

  // Neutral Palette
  gray50: '#F9FAFB',
  gray100: '#F3F4F6',
  gray200: '#E5E7EB',
  gray400: '#9CA3AF',
  gray600: '#4B5563',
  gray800: '#1F2937',
  gray900: '#111827',

  // Semantic
  infoBg: '#EFF6FF',
  infoText: '#1D4ED8',
  infoBorder: '#BFDBFE',
  successBg: '#ECFDF5',
  successText: '#065F46',
  successBorder: '#A7F3D0',
  warningBg: '#FFFBEB',
  warningText: '#92400E',
  warningBorder: '#FDE68A',
  dangerBg: '#FEF2F2',
  dangerText: '#991B1B',
  dangerBorder: '#FECACA',

  // Base
  white: '#FFFFFF',
  black: '#000000',
  backdrop: 'rgba(0,0,0,0.45)',

  // Dark Mode
  darkSurface: '#1C1C1E',
  darkSecondary: '#2C2C2E',
  darkBorder: '#3A3A3C',
  darkPrimary: '#3B82F6',
  darkSuccess: '#34D399',
} as const;

// ─── Typography ───
export const Typography = {
  fontFamily: {
    regular: 'Inter-Regular',
    medium: 'Inter-Medium',
    semibold: 'Inter-SemiBold',
    bold: 'Inter-Bold',
  },
  size: {
    displayLg: 28,
    displayMd: 24,
    headingLg: 20,
    headingMd: 17,
    headingSm: 15,
    bodyLg: 16,
    bodyMd: 14,
    bodySm: 13,
    caption: 12,
    labelLg: 14,
    labelSm: 12,
    overline: 11,
  },
  lineHeight: {
    displayLg: 36,
    displayMd: 32,
    headingLg: 28,
    headingMd: 24,
    headingSm: 22,
    bodyLg: 24,
    bodyMd: 22,
    bodySm: 20,
    caption: 18,
    labelLg: 20,
    labelSm: 16,
    overline: 16,
  },
} as const;

// ─── Spacing (Base 4px) ───
export const Spacing = {
  xs: 4,
  sm: 8,
  md: 12,
  base: 16,
  lg: 20,
  xl: 24,
  '2xl': 32,
  '3xl': 40,
  '4xl': 48,
} as const;

// ─── Border Radius ───
export const BorderRadius = {
  sm: 6,
  md: 10,
  lg: 14,
  xl: 20,
  full: 9999,
} as const;

// ─── Shadows ───
export const Shadows = {
  elevation1: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 1 },
    shadowOpacity: 0.08,
    shadowRadius: 4,
    elevation: 2,
  },
  elevation2: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.12,
    shadowRadius: 8,
    elevation: 4,
  },
  elevation3: {
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.16,
    shadowRadius: 16,
    elevation: 8,
  },
} as const;

// ─── Button Heights ───
export const ButtonHeight = {
  primary: 50,
  secondary: 50,
  ghost: 44,
  icon: 44,
} as const;

// ─── Input ───
export const InputHeight = {
  text: 50,
  search: 48,
} as const;
