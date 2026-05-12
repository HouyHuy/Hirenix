import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';

interface TrendInfo {
  value: number;
  isPositive: boolean;
  label?: string;
}

interface StatCardProps {
  title: string;
  value: number | string;
  icon: keyof typeof Ionicons.glyphMap;
  color: string;
  subtitle?: string;
  trend?: TrendInfo;
}

export const StatCard: React.FC<StatCardProps> = ({
  title,
  value,
  icon,
  color,
  subtitle,
  trend,
}) => {
  return (
    <View style={styles.card}>
      <View style={[styles.accentBar, { backgroundColor: color }]} />
      <View style={styles.headerRow}>
        <View style={[styles.iconWrap, { backgroundColor: `${color}1A` }]}>
          <Ionicons name={icon} size={18} color={color} />
        </View>
        {trend && (
          <View
            style={[
              styles.trendPill,
              { backgroundColor: trend.isPositive ? Colors.successBg : Colors.dangerBg },
            ]}
          >
            <Ionicons
              name={trend.isPositive ? 'arrow-up' : 'arrow-down'}
              size={12}
              color={trend.isPositive ? Colors.successText : Colors.dangerText}
            />
            <Text
              style={[
                styles.trendText,
                { color: trend.isPositive ? Colors.successText : Colors.dangerText },
              ]}
            >
              {trend.value}
            </Text>
            {trend.label && <Text style={styles.trendLabel}> {trend.label}</Text>}
          </View>
        )}
      </View>
      <Text style={styles.valueText}>{value}</Text>
      <Text style={styles.titleText}>{title}</Text>
      {subtitle && <Text style={styles.subtitleText}>{subtitle}</Text>}
    </View>
  );
};

const styles = StyleSheet.create({
  card: {
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    overflow: 'hidden',
    ...Shadows.elevation1,
  },
  accentBar: {
    position: 'absolute',
    left: 0,
    top: 0,
    bottom: 0,
    width: 4,
  },
  headerRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  iconWrap: {
    width: 34,
    height: 34,
    borderRadius: 10,
    alignItems: 'center',
    justifyContent: 'center',
  },
  valueText: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    color: Colors.gray900,
  },
  titleText: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  subtitleText: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray400,
  },
  trendPill: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingHorizontal: Spacing.sm,
    paddingVertical: 2,
    borderRadius: BorderRadius.full,
  },
  trendText: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.caption,
  },
  trendLabel: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
});
