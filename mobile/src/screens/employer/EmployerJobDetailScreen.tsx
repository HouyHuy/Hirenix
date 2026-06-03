import React, { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Alert,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { useFocusEffect } from '@react-navigation/native';
import { BorderRadius, Colors, Shadows, Spacing, Typography } from '../../constants/theme';
import { useToast } from '../../contexts/ToastContext';
import { employerJobApi } from '../../api/employerJobApi';
import { EmployerJobDto } from '../../types/employer';

export const EmployerJobDetailScreen: React.FC<any> = ({ route, navigation }) => {
  const { showToast } = useToast();
  const jobId = route?.params?.jobId as number;

  const [job, setJob] = useState<EmployerJobDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [actionLoading, setActionLoading] = useState<'close' | 'delete' | null>(null);

  const loadJob = useCallback(async (isRefresh = false) => {
    try {
      if (isRefresh) {
        setRefreshing(true);
      } else {
        setLoading(true);
      }
      const data = await employerJobApi.getJobById(jobId);
      setJob(data);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to load job detail', 'error');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [jobId, showToast]);

  useFocusEffect(
    useCallback(() => {
      loadJob();
    }, [loadJob])
  );

  const salaryText = useMemo(() => {
    if (!job) return '-';
    if (job.salaryMin == null && job.salaryMax == null) return 'Negotiable';
    const min = job.salaryMin != null ? `${job.salaryMin.toLocaleString()} ${job.currency}` : '';
    const max = job.salaryMax != null ? `${job.salaryMax.toLocaleString()} ${job.currency}` : '';
    if (min && max) return `${min} - ${max}`;
    return min || max;
  }, [job]);

  const handleEdit = () => {
    if (!job) return;
    navigation.navigate('EditJob', { jobId: job.id });
  };

  const handleViewApplications = () => {
    if (!job) return;
    navigation.navigate('EmployerApplications', { jobId: job.id });
  };

  const handleClose = () => {
    if (!job) return;
    Alert.alert('Close Job', 'Are you sure you want to close this job posting?', [
      { text: 'Cancel', style: 'cancel' },
      {
        text: 'Close Job',
        style: 'destructive',
        onPress: async () => {
          try {
            setActionLoading('close');
            await employerJobApi.closeJob(job.id);
            showToast('Job closed successfully', 'success');
            await loadJob(true);
          } catch (error: any) {
            showToast(error?.response?.data?.message || 'Failed to close job', 'error');
          } finally {
            setActionLoading(null);
          }
        },
      },
    ]);
  };

  const handleDelete = () => {
    if (!job) return;
    Alert.alert(
      'Delete Job',
      'This action cannot be undone. The job can only be deleted if it has no applications.',
      [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Delete',
          style: 'destructive',
          onPress: async () => {
            try {
              setActionLoading('delete');
              await employerJobApi.deleteJob(job.id);
              showToast('Job deleted successfully', 'success');
              navigation.goBack();
            } catch (error: any) {
              showToast(error?.response?.data?.message || 'Failed to delete job', 'error');
            } finally {
              setActionLoading(null);
            }
          },
        },
      ]
    );
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading job detail...</Text>
      </View>
    );
  }

  if (!job) {
    return (
      <View style={styles.center}>
        <Text style={styles.emptyTitle}>Job not found</Text>
        <Text style={styles.emptyText}>This job may have been deleted or you no longer have access.</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView
        contentContainerStyle={styles.content}
        refreshControl={
          <RefreshControl
            refreshing={refreshing}
            onRefresh={() => loadJob(true)}
            colors={[Colors.primaryBlue]}
            tintColor={Colors.primaryBlue}
          />
        }
      >
        <View style={styles.headerCard}>
          <View style={styles.rowBetween}>
            <Text style={styles.title}>{job.title}</Text>
            <View style={[styles.badge, job.status === 'Active' ? styles.activeBadge : styles.closedBadge]}>
              <Text style={[styles.badgeText, job.status === 'Active' ? styles.activeText : styles.closedText]}>
                {job.status}
              </Text>
            </View>
          </View>
          <Text style={styles.subtitle}>{job.companyName}</Text>
          <Text style={styles.metaText}>{job.employmentType} • {job.workMode} • {job.locationName}</Text>
        </View>

        <View style={styles.statsRow}>
          <View style={styles.statCard}>
            <Ionicons name="eye-outline" size={18} color={Colors.primaryBlue} />
            <Text style={styles.statValue}>{job.viewCount}</Text>
            <Text style={styles.statLabel}>Views</Text>
          </View>
          <View style={styles.statCard}>
            <Ionicons name="document-text-outline" size={18} color={Colors.accentTeal} />
            <Text style={styles.statValue}>{job.applicationCount}</Text>
            <Text style={styles.statLabel}>Applications</Text>
          </View>
        </View>

        <TouchableOpacity style={styles.atsButton} onPress={handleViewApplications} activeOpacity={0.8}>
          <Text style={styles.atsButtonText}>View Applications for This Job</Text>
        </TouchableOpacity>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Overview</Text>
          <Text style={styles.bodyText}>{job.description}</Text>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Requirements</Text>
          <Text style={styles.bodyText}>{job.requirements}</Text>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Responsibilities</Text>
          <Text style={styles.bodyText}>{job.responsibilities}</Text>
        </View>

        {job.benefits ? (
          <View style={styles.sectionCard}>
            <Text style={styles.sectionTitle}>Benefits</Text>
            <Text style={styles.bodyText}>{job.benefits}</Text>
          </View>
        ) : null}

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Details</Text>
          <Text style={styles.detailRow}><Text style={styles.detailLabel}>Industry: </Text>{job.industryName}</Text>
          <Text style={styles.detailRow}><Text style={styles.detailLabel}>Experience: </Text>{job.experienceLevel}</Text>
          <Text style={styles.detailRow}><Text style={styles.detailLabel}>Salary: </Text>{salaryText}</Text>
          <Text style={styles.detailRow}><Text style={styles.detailLabel}>Expiry: </Text>{new Date(job.expiryDate).toLocaleDateString()}</Text>
          <Text style={styles.detailRow}><Text style={styles.detailLabel}>Created: </Text>{new Date(job.createdAt).toLocaleDateString()}</Text>
        </View>

        <View style={styles.sectionCard}>
          <Text style={styles.sectionTitle}>Skills</Text>
          <View style={styles.skillWrap}>
            {job.skillNames.map((name) => (
              <View key={name} style={styles.skillChip}>
                <Text style={styles.skillText}>{name}</Text>
              </View>
            ))}
          </View>
        </View>
      </ScrollView>

      <View style={styles.footer}>
        <TouchableOpacity
          style={[styles.actionBtn, styles.editBtn, !job.canEdit && styles.disabledBtn]}
          disabled={!job.canEdit}
          onPress={handleEdit}
        >
          <Text style={styles.editBtnText}>Edit</Text>
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.actionBtn, styles.closeBtn, (!job.canClose || actionLoading === 'close') && styles.disabledBtn]}
          disabled={!job.canClose || actionLoading === 'close'}
          onPress={handleClose}
        >
          {actionLoading === 'close' ? (
            <ActivityIndicator size="small" color={Colors.white} />
          ) : (
            <Text style={styles.closeBtnText}>Close</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.actionBtn, styles.deleteBtn, (actionLoading === 'delete') && styles.disabledBtn]}
          disabled={actionLoading === 'delete'}
          onPress={handleDelete}
        >
          {actionLoading === 'delete' ? (
            <ActivityIndicator size="small" color={Colors.dangerText} />
          ) : (
            <Text style={styles.deleteBtnText}>Delete</Text>
          )}
        </TouchableOpacity>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.gray50,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: Spacing.xl,
  },
  loadingText: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  emptyTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    color: Colors.gray900,
  },
  emptyText: {
    marginTop: Spacing.sm,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
    textAlign: 'center',
  },
  content: {
    padding: Spacing.base,
    paddingBottom: 110,
    gap: Spacing.md,
  },
  headerCard: {
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    ...Shadows.elevation1,
  },
  rowBetween: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    justifyContent: 'space-between',
  },
  title: {
    flex: 1,
    marginRight: Spacing.md,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    color: Colors.gray900,
  },
  subtitle: {
    marginTop: 6,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray800,
  },
  metaText: {
    marginTop: 6,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray600,
  },
  badge: {
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
  },
  badgeText: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.caption,
  },
  activeBadge: { backgroundColor: '#E8F5E9' },
  activeText: { color: '#2E7D32' },
  closedBadge: { backgroundColor: '#FFEBEE' },
  closedText: { color: '#C62828' },
  statsRow: {
    flexDirection: 'row',
    gap: Spacing.md,
  },
  statCard: {
    flex: 1,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    ...Shadows.elevation1,
    alignItems: 'center',
  },
  statValue: {
    marginTop: 6,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
  },
  statLabel: {
    marginTop: 2,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
    color: Colors.gray600,
  },
  atsButton: {
    backgroundColor: Colors.primaryBlue,
    borderRadius: BorderRadius.md,
    paddingVertical: 12,
    alignItems: 'center',
    justifyContent: 'center',
  },
  atsButtonText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.labelLg,
  },
  sectionCard: {
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    borderWidth: 1,
    borderColor: Colors.gray100,
    ...Shadows.elevation1,
  },
  sectionTitle: {
    marginBottom: Spacing.sm,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingSm,
    color: Colors.gray900,
  },
  bodyText: {
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray800,
    lineHeight: Typography.lineHeight.bodyMd,
  },
  detailRow: {
    marginBottom: 6,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
    color: Colors.gray800,
  },
  detailLabel: {
    fontFamily: Typography.fontFamily.semibold,
    color: Colors.gray900,
  },
  skillWrap: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
  },
  skillChip: {
    backgroundColor: Colors.infoBg,
    borderWidth: 1,
    borderColor: Colors.infoBorder,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 6,
  },
  skillText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
    color: Colors.infoText,
  },
  footer: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: 0,
    padding: Spacing.base,
    backgroundColor: Colors.white,
    borderTopWidth: 1,
    borderTopColor: Colors.gray100,
    flexDirection: 'row',
    gap: Spacing.sm,
    ...Shadows.elevation1,
  },
  actionBtn: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: BorderRadius.md,
    height: 44,
  },
  editBtn: {
    backgroundColor: Colors.primaryBlue,
  },
  editBtnText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
  },
  closeBtn: {
    backgroundColor: Colors.gray800,
  },
  closeBtnText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
  },
  deleteBtn: {
    backgroundColor: Colors.dangerBg,
    borderWidth: 1,
    borderColor: Colors.dangerBorder,
  },
  deleteBtnText: {
    color: Colors.dangerText,
    fontFamily: Typography.fontFamily.semibold,
  },
  disabledBtn: {
    opacity: 0.6,
  },
});
