import React from 'react';
import { View, Text, StyleSheet, useWindowDimensions } from 'react-native';
import { PieChart } from 'react-native-chart-kit';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';

interface PieChartCardProps {
  title: string;
  data: Array<{ label: string; value: number; color: string }>;
}

export const PieChartCard: React.FC<PieChartCardProps> = ({ title, data }) => {
  const { width } = useWindowDimensions();
  const chartWidth = Math.max(280, width - Spacing.base * 4);
  const hasData = data.length > 0 && data.some((item) => item.value > 0);

  const chartData = data.map((item) => ({
    name: item.label,
    population: item.value,
    color: item.color,
    legendFontColor: Colors.gray600,
    legendFontSize: Typography.size.caption,
  }));

  return (
    <View style={styles.card}>
      <Text style={styles.title}>{title}</Text>
      {hasData ? (
        <PieChart
          data={chartData}
          width={chartWidth}
          height={220}
          chartConfig={{
            color: () => Colors.gray600,
          }}
          accessor="population"
          backgroundColor="transparent"
          paddingLeft="0"
          center={[0, 0]}
          absolute
        />
      ) : (
        <View style={styles.emptyState}>
          <Text style={styles.emptyTitle}>No data</Text>
          <Text style={styles.emptyText}>Chart will appear when data is available.</Text>
        </View>
      )}
    </View>
  );
};

const styles = StyleSheet.create({
  card: {
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    marginHorizontal: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    ...Shadows.elevation1,
  },
  title: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingSm,
    color: Colors.gray900,
    marginBottom: Spacing.sm,
  },
  emptyState: {
    height: 200,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.gray50,
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    borderColor: Colors.gray100,
  },
  emptyTitle: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray900,
  },
  emptyText: {
    marginTop: 4,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
});
