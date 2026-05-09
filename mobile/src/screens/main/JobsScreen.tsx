/**
 * JobsScreen — Hirenix
 * Danh sách công việc đã ứng tuyển
 */
import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { Colors, Typography } from '../../constants/theme';

export const JobsScreen: React.FC = () => {
  return (
    <View style={styles.container}>
      <Text style={styles.text}>Jobs Screen</Text>
      <Text style={styles.subtext}>Danh sách công việc đã ứng tuyển</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.white,
    alignItems: 'center',
    justifyContent: 'center',
  },
  text: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    color: Colors.gray900,
  },
  subtext: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
    marginTop: 8,
  },
});
