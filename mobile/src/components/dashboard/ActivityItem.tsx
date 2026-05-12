import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { Colors, Typography, Spacing, BorderRadius } from '../../constants/theme';
import { RecentActivity } from '../../types/admin';

interface ActivityItemProps {
  activity: RecentActivity;
}

const activityConfig: Record<string, { icon: keyof typeof Ionicons.glyphMap; color: string }> = {
  user_registered: { icon: 'person-add-outline', color: Colors.primaryBlue },
  job_posted: { icon: 'briefcase-outline', color: Colors.accentTeal },
  application_submitted: { icon: 'document-text-outline', color: Colors.warningAmber },
  company_created: { icon: 'business-outline', color: Colors.successText },
};

const formatTimestamp = (value: string) => {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) return value;
  const day = parsed.getDate().toString().padStart(2, '0');
  const month = (parsed.getMonth() + 1).toString().padStart(2, '0');
  const hours = parsed.getHours().toString().padStart(2, '0');
  const minutes = parsed.getMinutes().toString().padStart(2, '0');
  return `${day}/${month} ${hours}:${minutes}`;
};

export const ActivityItem: React.FC<ActivityItemProps> = ({ activity }) => {
  const config = activityConfig[activity.type] ?? {
    icon: 'pulse-outline',
    color: Colors.primaryBlue,
  };

  const metaText =
    activity.userName || activity.companyName || activity.jobTitle || 'System activity';

  return (
    <View style={styles.container}>
      <View style={[styles.iconWrap, { backgroundColor: `${config.color}1A` }]}>
        <Ionicons name={config.icon} size={18} color={config.color} />
      </View>
      <View style={styles.content}>
        <Text style={styles.title}>{activity.description}</Text>
        <Text style={styles.meta}>{metaText}</Text>
      </View>
      <Text style={styles.time}>{formatTimestamp(activity.timestamp)}</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: Spacing.md,
    paddingHorizontal: Spacing.base,
    borderBottomWidth: 1,
    borderBottomColor: Colors.gray100,
  },
  iconWrap: {
    width: 40,
    height: 40,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
    marginRight: Spacing.md,
  },
  content: {
    flex: 1,
  },
  title: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray900,
  },
  meta: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  time: {
    marginLeft: Spacing.sm,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray400,
  },
});
