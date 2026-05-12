import React, { useCallback, useEffect, useMemo, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  StatusBar,
  TouchableOpacity,
  RefreshControl,
  ActivityIndicator,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { LinearGradient } from 'expo-linear-gradient';
import { Colors, Typography, Spacing, BorderRadius, Shadows } from '../../constants/theme';
import { adminApi } from '../../api/adminApi';
import { ActivityItem } from '../../components/dashboard/ActivityItem';
import { LineChartCard } from '../../components/dashboard/LineChartCard';
import { PieChartCard } from '../../components/dashboard/PieChartCard';
import { StatCard } from '../../components/dashboard/StatCard';
import { useToast } from '../../contexts/ToastContext';
import { useAuth } from '../../contexts/AuthContext';
import { AnalyticsData, DashboardStats, RecentActivity } from '../../types/admin';

const PERIODS = [
  { label: '7D', value: '7d' },
  { label: '30D', value: '30d' },
  { label: '90D', value: '90d' },
  { label: '1Y', value: '1y' },
] as const;

type PeriodValue = (typeof PERIODS)[number]['value'];

export const AdminDashboardScreen: React.FC = () => {
  const insets = useSafeAreaInsets();
  const { showToast } = useToast();
  const { user } = useAuth();
  const [period, setPeriod] = useState<PeriodValue>('30d');
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [analytics, setAnalytics] = useState<AnalyticsData | null>(null);
  const [activities, setActivities] = useState<RecentActivity[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isAdmin = (user?.role || '').toLowerCase() === 'admin';

  const formatNumber = useCallback((value?: number) => {
    if (value === undefined || value === null) return '0';
    return new Intl.NumberFormat('en-US').format(value);
  }, []);

  const fetchDashboard = useCallback(
    async (isRefresh = false) => {
      if (isRefresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      setError(null);

      try {
        const [statsResponse, analyticsResponse, activityResponse] = await Promise.all([
          adminApi.getDashboardStats(),
          adminApi.getAnalytics(period),
          adminApi.getRecentActivities(8),
        ]);

        // 🔍 Debug logging
        console.log('📊 Dashboard API Responses:');
        console.log('  Stats:', JSON.stringify(statsResponse, null, 2));
        console.log('  Analytics:', JSON.stringify(analyticsResponse, null, 2));
        console.log('  Activities:', JSON.stringify(activityResponse, null, 2));
        console.log('  Period:', period);

        setStats(statsResponse ?? null);
        setAnalytics(analyticsResponse ?? null);
        setActivities(Array.isArray(activityResponse) ? activityResponse : []);
      } catch (err) {
        console.error('Dashboard fetch error:', err);
        setError('Unable to load dashboard data.');
        showToast('Failed to load dashboard data', 'error');
      } finally {
        setLoading(false);
        setRefreshing(false);
      }
    },
    [period, showToast]
  );

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  const roleDistribution = useMemo(() => {
    const candidate = stats?.usersByRole?.candidate ?? 0;
    const employer = stats?.usersByRole?.employer ?? 0;
    const total = candidate + employer;
    
    // 🔍 Debug logging
    console.log('👥 Role Distribution:', { candidate, employer, total, stats: stats?.usersByRole });
    
    if (total === 0) return [];
    return [
      { label: 'Candidate', value: candidate, color: Colors.primaryBlue },
      { label: 'Employer', value: employer, color: Colors.accentTeal },
    ];
  }, [stats]);

  if (!isAdmin) {
    return (
      <View style={styles.deniedContainer}>
        <Ionicons name="lock-closed-outline" size={40} color={Colors.gray400} />
        <Text style={styles.deniedTitle}>Access restricted</Text>
        <Text style={styles.deniedText}>This area is available for admin accounts only.</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor={Colors.primaryDark} />
      <ScrollView
        showsVerticalScrollIndicator={false}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => fetchDashboard(true)}
            tintColor={Colors.primaryBlue}
          />
        }
        contentContainerStyle={styles.scrollContent}
      >
        <LinearGradient
          colors={[Colors.primaryDark, Colors.primaryBlue]}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 1 }}
          style={[styles.hero, { paddingTop: insets.top + Spacing.lg }]}
        >
          <View style={styles.heroGlowTop} />
          <View style={styles.heroGlowBottom} />

          <View style={styles.heroRow}>
            <Text style={styles.heroEyebrow}>ADMIN CONTROL</Text>
            <View style={styles.heroBadge}>
              <Ionicons name="pulse" size={14} color={Colors.accentTeal} />
              <Text style={styles.heroBadgeText}>Live</Text>
            </View>
          </View>
          <Text style={styles.heroTitle}>Admin Dashboard</Text>
          <Text style={styles.heroSubtitle}>System overview and growth insights.</Text>

          <View style={styles.heroMetricsRow}>
            <View style={styles.heroMetricCard}>
              <Text style={styles.heroMetricValue}>{formatNumber(stats?.newUsersToday)}</Text>
              <Text style={styles.heroMetricLabel}>New users</Text>
            </View>
            <View style={styles.heroMetricCard}>
              <Text style={styles.heroMetricValue}>{formatNumber(stats?.newJobsToday)}</Text>
              <Text style={styles.heroMetricLabel}>New jobs</Text>
            </View>
            <View style={styles.heroMetricCard}>
              <Text style={styles.heroMetricValue}>{formatNumber(stats?.newApplicationsToday)}</Text>
              <Text style={styles.heroMetricLabel}>New applications</Text>
            </View>
          </View>
        </LinearGradient>

        {error && (
          <View style={styles.errorBanner}>
            <Ionicons name="alert-circle" size={18} color={Colors.dangerRed} />
            <Text style={styles.errorText}>{error}</Text>
            <TouchableOpacity onPress={() => fetchDashboard()}>
              <Text style={styles.retryText}>Retry</Text>
            </TouchableOpacity>
          </View>
        )}

        {loading && !refreshing && (
          <View style={styles.loadingRow}>
            <ActivityIndicator size="small" color={Colors.primaryBlue} />
            <Text style={styles.loadingText}>Loading dashboard...</Text>
          </View>
        )}

        <Text style={styles.sectionTitle}>Overview</Text>
        <View style={styles.statsGrid}>
          <View style={styles.statItem}>
            <StatCard
              title="Total users"
              value={formatNumber(stats?.totalUsers)}
              icon="people-outline"
              color={Colors.primaryBlue}
              trend={{ value: stats?.newUsersToday ?? 0, isPositive: true, label: 'today' }}
            />
          </View>
          <View style={styles.statItem}>
            <StatCard
              title="Total jobs"
              value={formatNumber(stats?.totalJobs)}
              icon="briefcase-outline"
              color={Colors.accentTeal}
              trend={{ value: stats?.newJobsToday ?? 0, isPositive: true, label: 'today' }}
              subtitle={`Pending: ${formatNumber(stats?.pendingJobsCount)}`}
            />
          </View>
          <View style={styles.statItem}>
            <StatCard
              title="Applications"
              value={formatNumber(stats?.totalApplications)}
              icon="document-text-outline"
              color={Colors.warningAmber}
              trend={{ value: stats?.newApplicationsToday ?? 0, isPositive: true, label: 'today' }}
            />
          </View>
          <View style={styles.statItem}>
            <StatCard
              title="Companies"
              value={formatNumber(stats?.totalCompanies)}
              icon="business-outline"
              color={Colors.successText}
              subtitle={`Active jobs: ${formatNumber(stats?.activeJobsCount)}`}
            />
          </View>
        </View>

        <Text style={styles.sectionTitle}>Quick actions</Text>
        <View style={styles.quickActionsGrid}>
          {[
            { label: 'Manage users', icon: 'person-outline', color: Colors.primaryBlue },
            { label: 'Review jobs', icon: 'briefcase-outline', color: Colors.accentTeal },
            { label: 'Companies', icon: 'business-outline', color: Colors.warningAmber },
            { label: 'Reports', icon: 'stats-chart-outline', color: Colors.successText },
          ].map((action) => (
            <TouchableOpacity key={action.label} style={styles.actionCard}>
              <View style={[styles.actionIcon, { backgroundColor: `${action.color}1A` }]}>
                <Ionicons name={action.icon as any} size={18} color={action.color} />
              </View>
              <Text style={styles.actionText}>{action.label}</Text>
            </TouchableOpacity>
          ))}
        </View>

        <View style={styles.sectionHeaderRow}>
          <Text style={styles.sectionTitle}>Analytics</Text>
          <View style={styles.periodRow}>
            {PERIODS.map((item) => {
              const isActive = period === item.value;
              return (
                <TouchableOpacity
                  key={item.value}
                  onPress={() => setPeriod(item.value)}
                  style={[styles.periodButton, isActive && styles.periodButtonActive]}
                >
                  <Text
                    style={[styles.periodButtonText, isActive && styles.periodButtonTextActive]}
                  >
                    {item.label}
                  </Text>
                </TouchableOpacity>
              );
            })}
          </View>
        </View>

        <LineChartCard
          title="User growth"
          subtitle={`Period: ${analytics?.period ?? period}`}
          data={analytics?.usersGrowth ?? []}
          color={Colors.primaryBlue}
        />
        <View style={styles.sectionSpacer} />
        <LineChartCard
          title="Jobs growth"
          subtitle={`Period: ${analytics?.period ?? period}`}
          data={analytics?.jobsGrowth ?? []}
          color={Colors.accentTeal}
        />
        <View style={styles.sectionSpacer} />
        <LineChartCard
          title="Applications growth"
          subtitle={`Period: ${analytics?.period ?? period}`}
          data={analytics?.applicationsGrowth ?? []}
          color={Colors.warningAmber}
        />
        <View style={styles.sectionSpacer} />

        <PieChartCard title="Users by role" data={roleDistribution} />

        <View style={styles.sectionHeaderRow}>
          <Text style={styles.sectionTitle}>Recent activity</Text>
          {loading && <ActivityIndicator size="small" color={Colors.primaryBlue} />}
        </View>
        <View style={styles.activitiesCard}>
          <ScrollView 
            style={styles.activitiesScroll}
            nestedScrollEnabled={true}
            showsVerticalScrollIndicator={true}
          >
            {(activities?.length ?? 0) > 0 ? (
              activities.map((activity) => (
                <ActivityItem key={activity.id} activity={activity} />
              ))
            ) : (
              <View style={styles.emptyActivity}>
                <Text style={styles.emptyActivityTitle}>No recent activity</Text>
                <Text style={styles.emptyActivityText}>New events will appear here.</Text>
              </View>
            )}
          </ScrollView>
        </View>
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.gray50,
  },
  scrollContent: {
    paddingBottom: Spacing['3xl'],
  },
  hero: {
    paddingHorizontal: Spacing.base,
    paddingBottom: Spacing.xl,
    borderBottomLeftRadius: BorderRadius.xl,
    borderBottomRightRadius: BorderRadius.xl,
    overflow: 'hidden',
  },
  heroGlowTop: {
    position: 'absolute',
    width: 180,
    height: 180,
    borderRadius: 90,
    backgroundColor: 'rgba(255,255,255,0.15)',
    top: -40,
    right: -20,
  },
  heroGlowBottom: {
    position: 'absolute',
    width: 220,
    height: 220,
    borderRadius: 110,
    backgroundColor: 'rgba(255,255,255,0.08)',
    bottom: -120,
    left: -40,
  },
  heroRow: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  heroEyebrow: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.overline,
    color: Colors.gray100,
    letterSpacing: 1.2,
  },
  heroBadge: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(255,255,255,0.16)',
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
    borderRadius: BorderRadius.full,
    gap: 6,
  },
  heroBadgeText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    color: Colors.gray100,
  },
  heroTitle: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.displayMd,
    color: Colors.white,
  },
  heroSubtitle: {
    marginTop: Spacing.xs,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray100,
  },
  heroMetricsRow: {
    flexDirection: 'row',
    gap: Spacing.md,
    marginTop: Spacing.lg,
  },
  heroMetricCard: {
    flex: 1,
    backgroundColor: 'rgba(255,255,255,0.18)',
    borderRadius: BorderRadius.md,
    paddingVertical: Spacing.sm,
    paddingHorizontal: Spacing.sm,
  },
  heroMetricValue: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
    color: Colors.white,
  },
  heroMetricLabel: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray100,
  },
  errorBanner: {
    marginTop: Spacing.md,
    marginHorizontal: Spacing.base,
    padding: Spacing.base,
    borderRadius: BorderRadius.lg,
    backgroundColor: Colors.dangerBg,
    borderWidth: 1,
    borderColor: Colors.dangerBorder,
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.sm,
  },
  errorText: {
    flex: 1,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.dangerText,
  },
  retryText: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodySm,
    color: Colors.dangerRed,
  },
  loadingRow: {
    marginTop: Spacing.md,
    marginHorizontal: Spacing.base,
    paddingVertical: Spacing.sm,
    flexDirection: 'row',
    alignItems: 'center',
    gap: Spacing.sm,
  },
  loadingText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  sectionTitle: {
    marginTop: Spacing.lg,
    marginBottom: Spacing.sm,
    paddingHorizontal: Spacing.base,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
    color: Colors.gray900,
  },
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.md,
    paddingHorizontal: Spacing.base,
  },
  statItem: {
    flexBasis: '48%',
  },
  quickActionsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.md,
    paddingHorizontal: Spacing.base,
  },
  actionCard: {
    flexBasis: '48%',
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    ...Shadows.elevation1,
  },
  actionIcon: {
    width: 36,
    height: 36,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
  },
  actionText: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
    color: Colors.gray800,
  },
  sectionHeaderRow: {
    marginTop: Spacing.lg,
    marginBottom: Spacing.sm,
    paddingHorizontal: Spacing.base,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  periodRow: {
    flexDirection: 'row',
    gap: Spacing.sm,
  },
  periodButton: {
    paddingVertical: 6,
    paddingHorizontal: Spacing.sm,
    borderRadius: BorderRadius.full,
    borderWidth: 1,
    borderColor: Colors.gray200,
    backgroundColor: Colors.white,
  },
  periodButtonActive: {
    backgroundColor: Colors.infoBg,
    borderColor: Colors.infoBorder,
  },
  periodButtonText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
  periodButtonTextActive: {
    color: Colors.infoText,
  },
  sectionSpacer: {
    height: Spacing.md,
  },
  activitiesCard: {
    marginHorizontal: Spacing.base,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    borderColor: Colors.gray100,
    overflow: 'hidden',
    ...Shadows.elevation1,
  },
  activitiesScroll: {
    maxHeight: 400,
  },
  emptyActivity: {
    padding: Spacing.xl,
    alignItems: 'center',
    justifyContent: 'center',
  },
  emptyActivityTitle: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray900,
  },
  emptyActivityText: {
    marginTop: 4,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  deniedContainer: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    padding: Spacing['2xl'],
    backgroundColor: Colors.gray50,
  },
  deniedTitle: {
    marginTop: Spacing.md,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
  },
  deniedText: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
    textAlign: 'center',
  },
});
