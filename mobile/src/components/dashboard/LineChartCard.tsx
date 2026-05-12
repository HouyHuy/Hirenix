import React, { useMemo } from 'react';
import { View, Text, StyleSheet, useWindowDimensions } from 'react-native';
import { LineChart } from 'react-native-chart-kit';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';

interface LineChartCardProps {
  title: string;
  subtitle?: string;
  data: Array<{ date: string; count: number }>;
  color: string;
}

const buildLabels = (data: Array<{ date: string; count: number }>) => {
  if (data.length === 0) return [];
  const step = Math.max(1, Math.ceil(data.length / 6));
  return data.map((item, index) => {
    if (index % step !== 0 && index !== data.length - 1) return '';
    const parsed = new Date(item.date);
    if (Number.isNaN(parsed.getTime())) return item.date.slice(5);
    const day = parsed.getDate().toString().padStart(2, '0');
    const month = (parsed.getMonth() + 1).toString().padStart(2, '0');
    return `${day}/${month}`;
  });
};

export const LineChartCard: React.FC<LineChartCardProps> = ({
  title,
  subtitle,
  data,
  color,
}) => {
  const { width } = useWindowDimensions();
  const chartWidth = Math.max(280, width - Spacing.base * 4);
  const values = useMemo(() => data.map((item) => item.count), [data]);
  const labels = useMemo(() => buildLabels(data), [data]);
  
  // Show chart if we have data points (even if all zeros)
  // This provides better UX than empty state for zero-value periods
  const hasData = values.length > 0;
  const hasPositiveValues = values.some((value) => value > 0);

  // Prevent react-native-chart-kit crash when all values are 0
  // Use tiny values (0.01) instead to show flat line
  const safeValues = useMemo(() => {
    if (values.length === 0) return [];
    const allZeros = values.every((v) => v === 0);
    return allZeros ? values.map(() => 0.01) : values;
  }, [values]);

  // 🔍 Debug logging
  console.log(`📈 ${title}:`, {
    dataLength: data.length,
    valuesLength: values.length,
    values: values.slice(0, 5), // First 5 values
    safeValues: safeValues.slice(0, 5),
    hasData,
    hasPositiveValues,
    allZeros: values.every((v) => v === 0),
    rawData: data.slice(0, 3), // First 3 raw data points
  });

  return (
    <View style={styles.card}>
      <View style={styles.header}>
        <Text style={styles.title}>{title}</Text>
        {subtitle && <Text style={styles.subtitle}>{subtitle}</Text>}
      </View>
      {hasData ? (
        <LineChart
          data={{
            labels,
            datasets: [{ data: safeValues }],
          }}
          width={chartWidth}
          height={220}
          withShadow={false}
          bezier
          fromZero
          yAxisInterval={1}
          chartConfig={{
            backgroundGradientFrom: Colors.white,
            backgroundGradientTo: Colors.gray50,
            decimalPlaces: 0,
            color: () => color,
            labelColor: () => Colors.gray400,
            propsForDots: {
              r: '4',
              strokeWidth: '2',
              stroke: Colors.white,
            },
            propsForBackgroundLines: {
              stroke: Colors.gray100,
            },
          }}
          style={styles.chart}
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
  header: {
    marginBottom: Spacing.sm,
  },
  title: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingSm,
    color: Colors.gray900,
  },
  subtitle: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray400,
  },
  chart: {
    borderRadius: BorderRadius.lg,
    marginTop: Spacing.sm,
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
