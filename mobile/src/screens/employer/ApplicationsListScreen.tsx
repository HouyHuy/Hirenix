import React, { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { BorderRadius, Colors, Shadows, Spacing, Typography } from '../../constants/theme';
import { employerApplicationApi } from '../../api/employerApplicationApi';
import { employerJobApi } from '../../api/employerJobApi';
import { EmployerApplicationDto, ApplicationStatus, ApplicationStatusOptions } from '../../types/employerApplication';
import { EmployerJobDto } from '../../types/employer';
import { useToast } from '../../contexts/ToastContext';

const statusTabs: Array<{ label: string; value: ApplicationStatus | 'All' }> = [
  { label: 'All', value: 'All' },
  ...ApplicationStatusOptions,
];

export default function ApplicationsListScreen({ navigation, route }: any) {
  const { showToast } = useToast();
  const initialJobId = route?.params?.jobId as number | undefined;

  const [applications, setApplications] = useState<EmployerApplicationDto[]>([]);
  const [jobs, setJobs] = useState<EmployerJobDto[]>([]);
  const [statsTotal, setStatsTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [selectedStatus, setSelectedStatus] = useState<ApplicationStatus | 'All'>('All');
  const [selectedJobId, setSelectedJobId] = useState<number | undefined>(initialJobId);

  const selectedJobLabel = useMemo(() => {
    if (!selectedJobId) return 'All jobs';
    return jobs.find((j) => j.id === selectedJobId)?.title ?? 'Selected job';
  }, [jobs, selectedJobId]);

  const loadData = useCallback(async (isRefresh = false) => {
    try {
      if (isRefresh) setRefreshing(true);
      else setLoading(true);

      const status = selectedStatus === 'All' ? undefined : selectedStatus;
      const [apps, jobsRes, stats] = await Promise.all([
        employerApplicationApi.getApplications(selectedJobId, status),
        employerJobApi.getMyJobs(),
        employerApplicationApi.getStatistics(),
      ]);

      setApplications(apps);
      setJobs(jobsRes);
      setStatsTotal(stats.total ?? apps.length);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to load applications', 'error');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [selectedJobId, selectedStatus, showToast]);

  useFocusEffect(
    useCallback(() => {
      loadData();
    }, [loadData])
  );

  const openDetail = (applicationId: number) => {
    navigation.navigate('EmployerApplicationDetail', { applicationId });
  };

  const renderApplicationCard = ({ item }: { item: EmployerApplicationDto }) => (
    <TouchableOpacity style={styles.card} onPress={() => openDetail(item.id)} activeOpacity={0.8}>
      <View style={styles.cardHeader}>
        <Text style={styles.candidateName}>{item.candidateName}</Text>
        <View style={styles.statusPill}>
          <Text style={styles.statusText}>{item.status}</Text>
        </View>
      </View>
      <Text style={styles.jobTitle} numberOfLines={1}>{item.jobTitle}</Text>
      <Text style={styles.metaText}>{item.candidateEmail}</Text>
      <Text style={styles.metaText}>
        {item.currentPosition || 'Position N/A'} • {item.yearsOfExperience} years
      </Text>
      <Text style={styles.dateText}>Applied: {new Date(item.appliedDate).toLocaleDateString()}</Text>
    </TouchableOpacity>
  );

  return (
    <View style={styles.container}>
      <View style={styles.statsCard}>
        <Text style={styles.statsTitle}>Applications</Text>
        <Text style={styles.statsValue}>{statsTotal}</Text>
      </View>

      <View style={styles.filtersWrap}>
        <FlatList
          horizontal
          data={statusTabs}
          keyExtractor={(item) => item.value}
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.statusTabs}
          renderItem={({ item }) => {
            const active = selectedStatus === item.value;
            return (
              <TouchableOpacity
                style={[styles.tab, active && styles.tabActive]}
                onPress={() => setSelectedStatus(item.value)}
              >
                <Text style={[styles.tabText, active && styles.tabTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          }}
        />

        <FlatList
          horizontal
          data={[{ id: 0, title: 'All jobs' }, ...jobs]}
          keyExtractor={(item) => item.id.toString()}
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.jobTabs}
          renderItem={({ item }) => {
            const value = item.id === 0 ? undefined : item.id;
            const active = selectedJobId === value;
            return (
              <TouchableOpacity
                style={[styles.tab, active && styles.tabActive]}
                onPress={() => setSelectedJobId(value)}
              >
                <Text style={[styles.tabText, active && styles.tabTextActive]} numberOfLines={1}>
                  {item.title}
                </Text>
              </TouchableOpacity>
            );
          }}
        />
        <Text style={styles.selectedJobText}>Filter: {selectedJobLabel}</Text>
      </View>

      {loading ? (
        <View style={styles.center}>
          <ActivityIndicator size="large" color={Colors.primaryBlue} />
          <Text style={styles.loadingText}>Loading applications...</Text>
        </View>
      ) : (
        <FlatList
          data={applications}
          keyExtractor={(item) => item.id.toString()}
          renderItem={renderApplicationCard}
          contentContainerStyle={styles.listContent}
          ListEmptyComponent={<Text style={styles.emptyText}>No applications found.</Text>}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={() => loadData(true)}
              colors={[Colors.primaryBlue]}
              tintColor={Colors.primaryBlue}
            />
          }
          showsVerticalScrollIndicator={false}
        />
      )}
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  statsCard: {
    margin: Spacing.base,
    marginBottom: Spacing.sm,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    borderColor: Colors.gray100,
    padding: Spacing.base,
    ...Shadows.elevation1,
  },
  statsTitle: {
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
  },
  statsValue: {
    marginTop: 4,
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.displayMd,
  },
  filtersWrap: { paddingBottom: Spacing.sm },
  statusTabs: { paddingHorizontal: Spacing.base, gap: Spacing.sm },
  jobTabs: { paddingHorizontal: Spacing.base, gap: Spacing.sm, marginTop: Spacing.sm },
  tab: {
    paddingHorizontal: Spacing.md,
    paddingVertical: 8,
    borderWidth: 1,
    borderColor: Colors.gray200,
    borderRadius: BorderRadius.full,
    backgroundColor: Colors.white,
    maxWidth: 220,
  },
  tabActive: {
    backgroundColor: Colors.primaryBlue,
    borderColor: Colors.primaryBlue,
  },
  tabText: {
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  tabTextActive: { color: Colors.white },
  selectedJobText: {
    marginTop: Spacing.sm,
    paddingHorizontal: Spacing.base,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
  },
  center: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
  listContent: { padding: Spacing.base, paddingBottom: Spacing['2xl'] },
  card: {
    marginBottom: Spacing.sm,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    borderColor: Colors.gray100,
    padding: Spacing.base,
    ...Shadows.elevation1,
  },
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  candidateName: {
    flex: 1,
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
    marginRight: Spacing.sm,
  },
  statusPill: {
    backgroundColor: Colors.infoBg,
    borderWidth: 1,
    borderColor: Colors.infoBorder,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
  },
  statusText: {
    color: Colors.infoText,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.caption,
  },
  jobTitle: {
    marginTop: Spacing.sm,
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodyMd,
  },
  metaText: {
    marginTop: 4,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
  },
  dateText: {
    marginTop: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
  },
  emptyText: {
    textAlign: 'center',
    marginTop: Spacing['2xl'],
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
});
