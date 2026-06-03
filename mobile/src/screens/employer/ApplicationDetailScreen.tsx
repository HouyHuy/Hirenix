import React, { useCallback, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  Linking,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { BorderRadius, Colors, Shadows, Spacing, Typography } from '../../constants/theme';
import { employerApplicationApi } from '../../api/employerApplicationApi';
import { ApplicationStatus, ApplicationStatusOptions, EmployerApplicationDto } from '../../types/employerApplication';
import { useToast } from '../../contexts/ToastContext';

const getAllowedStatusTransitions = (status: ApplicationStatus): ApplicationStatus[] => {
  if (status === 'Applied') return ['Reviewing', 'Shortlisted', 'Rejected', 'Accepted'];
  if (status === 'Reviewing') return ['Shortlisted', 'Rejected', 'Accepted'];
  if (status === 'Shortlisted') return ['Rejected', 'Accepted'];
  return [];
};

export default function ApplicationDetailScreen({ route, navigation }: any) {
  const { showToast } = useToast();
  const applicationId = route?.params?.applicationId as number;

  const [application, setApplication] = useState<EmployerApplicationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [updatingStatus, setUpdatingStatus] = useState<ApplicationStatus | null>(null);

  const loadDetail = useCallback(async (isRefresh = false) => {
    try {
      if (isRefresh) setRefreshing(true);
      else setLoading(true);

      const data = await employerApplicationApi.getApplicationById(applicationId);
      setApplication(data);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to load application detail', 'error');
      if (!isRefresh) {
        navigation.goBack();
      }
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [applicationId, navigation, showToast]);

  useFocusEffect(
    useCallback(() => {
      loadDetail();
    }, [loadDetail])
  );

  const allowedTransitions = useMemo(() => {
    if (!application) return [];
    return getAllowedStatusTransitions(application.status);
  }, [application]);

  const handleOpenCv = async () => {
    if (!application?.cvUrl) return;

    const supported = await Linking.canOpenURL(application.cvUrl);
    if (!supported) {
      showToast('Cannot open CV URL on this device', 'warning');
      return;
    }

    await Linking.openURL(application.cvUrl);
  };

  const handleStatusUpdate = async (nextStatus: ApplicationStatus) => {
    if (!application) return;

    try {
      setUpdatingStatus(nextStatus);
      await employerApplicationApi.updateStatus(application.id, { status: nextStatus });
      showToast(`Application moved to ${nextStatus}`, 'success');
      await loadDetail(true);
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to update status', 'error');
    } finally {
      setUpdatingStatus(null);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading application...</Text>
      </View>
    );
  }

  if (!application) {
    return (
      <View style={styles.center}>
        <Text style={styles.emptyText}>Application not found.</Text>
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
            onRefresh={() => loadDetail(true)}
            colors={[Colors.primaryBlue]}
            tintColor={Colors.primaryBlue}
          />
        }
      >
        <View style={styles.card}>
          <Text style={styles.title}>{application.candidateName}</Text>
          <Text style={styles.subtitle}>{application.candidateEmail}</Text>
          <Text style={styles.subtitle}>{application.candidatePhone || 'No phone provided'}</Text>
          <View style={styles.statusPill}>
            <Text style={styles.statusText}>Current: {application.status}</Text>
          </View>
        </View>

        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Application</Text>
          <Text style={styles.label}>Job</Text>
          <Text style={styles.value}>{application.jobTitle}</Text>
          <Text style={styles.label}>Applied Date</Text>
          <Text style={styles.value}>{new Date(application.appliedDate).toLocaleString()}</Text>
          <Text style={styles.label}>Review Date</Text>
          <Text style={styles.value}>{application.reviewedDate ? new Date(application.reviewedDate).toLocaleString() : 'Not reviewed yet'}</Text>

          <TouchableOpacity style={styles.cvButton} onPress={handleOpenCv} activeOpacity={0.8}>
            <Text style={styles.cvButtonText}>Open CV</Text>
          </TouchableOpacity>
        </View>

        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Candidate Summary</Text>
          <Text style={styles.value}>Current Position: {application.currentPosition || 'N/A'}</Text>
          <Text style={styles.value}>Experience: {application.yearsOfExperience} year(s)</Text>
          <Text style={[styles.label, styles.skillsLabel]}>Skills</Text>
          <View style={styles.skillWrap}>
            {application.skills.length === 0 ? (
              <Text style={styles.value}>No skills listed</Text>
            ) : (
              application.skills.map((skill) => (
                <View key={skill} style={styles.skillChip}>
                  <Text style={styles.skillText}>{skill}</Text>
                </View>
              ))
            )}
          </View>
        </View>

        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Cover Letter</Text>
          <Text style={styles.value}>{application.coverLetter || 'No cover letter provided.'}</Text>
        </View>

        {application.reviewNotes ? (
          <View style={styles.card}>
            <Text style={styles.sectionTitle}>Review Notes</Text>
            <Text style={styles.value}>{application.reviewNotes}</Text>
          </View>
        ) : null}

        <View style={styles.card}>
          <Text style={styles.sectionTitle}>Update Status</Text>
          {allowedTransitions.length === 0 ? (
            <Text style={styles.value}>No available status transition for this application.</Text>
          ) : (
            <View style={styles.actionsWrap}>
              {allowedTransitions.map((status) => {
                const loadingAction = updatingStatus === status;
                return (
                  <TouchableOpacity
                    key={status}
                    style={[styles.actionButton, loadingAction && styles.actionButtonDisabled]}
                    disabled={!!updatingStatus}
                    onPress={() => handleStatusUpdate(status)}
                  >
                    {loadingAction ? (
                      <ActivityIndicator size="small" color={Colors.white} />
                    ) : (
                      <Text style={styles.actionText}>{status}</Text>
                    )}
                  </TouchableOpacity>
                );
              })}
            </View>
          )}
        </View>
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  content: { padding: Spacing.base, gap: Spacing.md, paddingBottom: Spacing.xl },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  loadingText: {
    marginTop: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
  emptyText: {
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
  card: {
    backgroundColor: Colors.white,
    borderWidth: 1,
    borderColor: Colors.gray100,
    borderRadius: BorderRadius.lg,
    padding: Spacing.base,
    ...Shadows.elevation1,
  },
  title: {
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
  },
  subtitle: {
    marginTop: 4,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
  },
  statusPill: {
    marginTop: Spacing.sm,
    alignSelf: 'flex-start',
    backgroundColor: Colors.infoBg,
    borderColor: Colors.infoBorder,
    borderWidth: 1,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 4,
  },
  statusText: {
    color: Colors.infoText,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.caption,
  },
  sectionTitle: {
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.headingSm,
    marginBottom: Spacing.sm,
  },
  label: {
    marginTop: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  value: {
    marginTop: 4,
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
  },
  cvButton: {
    marginTop: Spacing.base,
    backgroundColor: Colors.primaryBlue,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
    height: 44,
  },
  cvButtonText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.labelLg,
  },
  skillsLabel: { marginBottom: Spacing.sm },
  skillWrap: { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.sm },
  skillChip: {
    backgroundColor: Colors.infoBg,
    borderWidth: 1,
    borderColor: Colors.infoBorder,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 6,
  },
  skillText: {
    color: Colors.infoText,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  actionsWrap: { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.sm },
  actionButton: {
    backgroundColor: Colors.primaryBlue,
    borderRadius: BorderRadius.md,
    paddingHorizontal: Spacing.md,
    minWidth: 110,
    height: 40,
    justifyContent: 'center',
    alignItems: 'center',
  },
  actionButtonDisabled: { opacity: 0.65 },
  actionText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodySm,
  },
});
