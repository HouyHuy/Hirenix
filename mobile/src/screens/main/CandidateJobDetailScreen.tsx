import React, { useCallback, useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { Ionicons } from '@expo/vector-icons';
import { BorderRadius, Colors, Spacing, Typography } from '../../constants/theme';
import { useToast } from '../../contexts/ToastContext';
import { jobApi } from '../../api/jobApi';
import { JobDetailDto } from '../../types/job';

export const CandidateJobDetailScreen: React.FC<any> = ({ route }) => {
  const { showToast } = useToast();
  const jobId = Number(route?.params?.jobId);

  const [job, setJob] = useState<JobDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [coverLetter, setCoverLetter] = useState('');
  const [cvFileUri, setCvFileUri] = useState('');

  const loadJob = useCallback(async () => {
    if (!jobId) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const response = await jobApi.getJobDetail(jobId);
      if (!response.success || !response.data) {
        throw new Error(response.message || 'Failed to load job details');
      }
      setJob(response.data);
    } catch (error: any) {
      showToast(error?.response?.data?.message || error?.message || 'Failed to load job details', 'error');
    } finally {
      setLoading(false);
    }
  }, [jobId, showToast]);

  useFocusEffect(
    useCallback(() => {
      loadJob();
    }, [loadJob]),
  );

  const formatSalary = () => {
    if (!job) {
      return '';
    }
    if (!job.isSalaryVisible) {
      return 'Thương lượng';
    }
    if (job.salaryMin == null && job.salaryMax == null) {
      return 'Cạnh tranh';
    }
    if (job.salaryMin != null && job.salaryMax != null) {
      return `${job.salaryMin.toLocaleString()} - ${job.salaryMax.toLocaleString()}`;
    }
    return `${(job.salaryMin ?? job.salaryMax ?? 0).toLocaleString()}`;
  };

  const submitApplication = async () => {
    if (!job || job.hasApplied) {
      return;
    }

    const trimmedUri = cvFileUri.trim();
    if (!trimmedUri) {
      showToast('Vui lòng nhập CV file URI trước khi ứng tuyển', 'error');
      return;
    }

    const inferredName = trimmedUri.split('/').pop() || `cv-${Date.now()}.pdf`;

    try {
      setSubmitting(true);
      await jobApi.submitApplication({
        jobId: job.id,
        cvFileUri: trimmedUri,
        cvFileName: inferredName,
        coverLetter,
      });
      showToast('Ứng tuyển thành công!', 'success');
      await loadJob();
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Ứng tuyển thất bại', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Đang tải chi tiết việc làm...</Text>
      </View>
    );
  }

  if (!job) {
    return (
      <View style={styles.center}>
        <Text style={styles.emptyText}>Không tìm thấy việc làm.</Text>
      </View>
    );
  }

  return (
    <ScrollView style={styles.container} contentContainerStyle={styles.content}>
      <View style={styles.card}>
        <Text style={styles.title}>{job.title}</Text>
        <Text style={styles.company}>{job.company?.name || 'Unknown company'}</Text>

        <View style={styles.metaWrap}>
          <View style={styles.metaItem}>
            <Ionicons name="location-outline" size={15} color={Colors.gray600} />
            <Text style={styles.metaText}>{job.city || 'Đang cập nhật'}</Text>
          </View>
          <View style={styles.metaItem}>
            <Ionicons name="briefcase-outline" size={15} color={Colors.gray600} />
            <Text style={styles.metaText}>{job.workType}</Text>
          </View>
          <View style={styles.metaItem}>
            <Ionicons name="layers-outline" size={15} color={Colors.gray600} />
            <Text style={styles.metaText}>{job.level}</Text>
          </View>
        </View>

        <View style={styles.salaryRow}>
          <Text style={styles.salaryLabel}>Mức lương</Text>
          <Text style={styles.salaryValue}>{formatSalary()}</Text>
        </View>

        <Text style={styles.sectionTitle}>Mô tả công việc</Text>
        <Text style={styles.bodyText}>{job.description}</Text>

        {job.requirements ? (
          <>
            <Text style={styles.sectionTitle}>Yêu cầu</Text>
            <Text style={styles.bodyText}>{job.requirements}</Text>
          </>
        ) : null}

        {job.benefits ? (
          <>
            <Text style={styles.sectionTitle}>Quyền lợi</Text>
            <Text style={styles.bodyText}>{job.benefits}</Text>
          </>
        ) : null}

        {!!job.skills?.length && (
          <>
            <Text style={styles.sectionTitle}>Kỹ năng</Text>
            <View style={styles.skillWrap}>
              {job.skills.map((skill) => (
                <View key={skill.id} style={styles.skillChip}>
                  <Text style={styles.skillText}>{skill.name}</Text>
                </View>
              ))}
            </View>
          </>
        )}
      </View>

      <View style={styles.card}>
        <Text style={styles.sectionTitle}>Ứng tuyển</Text>
        <Text style={styles.helperText}>Nhập đường dẫn file CV trên thiết bị (URI) để gửi hồ sơ.</Text>

        <TextInput
          style={styles.input}
          value={cvFileUri}
          onChangeText={setCvFileUri}
          placeholder="file:///path/to/your-cv.pdf"
          placeholderTextColor={Colors.gray400}
          autoCapitalize="none"
        />

        <TextInput
          style={[styles.input, styles.textArea]}
          value={coverLetter}
          onChangeText={setCoverLetter}
          placeholder="Cover letter (không bắt buộc)"
          placeholderTextColor={Colors.gray400}
          multiline
          numberOfLines={4}
        />

        <TouchableOpacity
          style={[styles.applyBtn, (submitting || job.hasApplied) && styles.applyBtnDisabled]}
          onPress={submitApplication}
          activeOpacity={0.85}
          disabled={submitting || job.hasApplied}
        >
          {submitting ? (
            <ActivityIndicator color={Colors.white} />
          ) : (
            <Text style={styles.applyBtnText}>{job.hasApplied ? 'Bạn đã ứng tuyển' : 'Ứng tuyển ngay'}</Text>
          )}
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.gray50,
  },
  content: {
    padding: Spacing.base,
    gap: Spacing.sm,
    paddingBottom: Spacing['2xl'],
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.gray50,
  },
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
    borderRadius: BorderRadius.lg,
    borderWidth: 1,
    borderColor: Colors.gray100,
    padding: Spacing.base,
  },
  title: {
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingLg,
    lineHeight: Typography.lineHeight.headingLg,
  },
  company: {
    marginTop: 2,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
  },
  metaWrap: {
    marginTop: Spacing.md,
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.md,
  },
  metaItem: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
  },
  metaText: {
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodySm,
  },
  salaryRow: {
    marginTop: Spacing.md,
    paddingVertical: Spacing.sm,
    borderTopWidth: 1,
    borderBottomWidth: 1,
    borderColor: Colors.gray100,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  salaryLabel: {
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodySm,
  },
  salaryValue: {
    color: Colors.primaryBlue,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.bodyMd,
  },
  sectionTitle: {
    marginTop: Spacing.md,
    marginBottom: 6,
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingSm,
  },
  bodyText: {
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    lineHeight: Typography.lineHeight.bodyMd,
  },
  skillWrap: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
  },
  skillChip: {
    paddingHorizontal: Spacing.sm,
    paddingVertical: 6,
    borderRadius: BorderRadius.full,
    backgroundColor: Colors.infoBg,
    borderWidth: 1,
    borderColor: Colors.infoBorder,
  },
  skillText: {
    color: Colors.infoText,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  helperText: {
    marginBottom: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
  },
  input: {
    minHeight: 46,
    borderWidth: 1,
    borderColor: Colors.gray200,
    borderRadius: BorderRadius.md,
    paddingHorizontal: Spacing.md,
    color: Colors.gray900,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.bodyMd,
    backgroundColor: Colors.white,
    marginBottom: Spacing.sm,
  },
  textArea: {
    minHeight: 96,
    textAlignVertical: 'top',
    paddingTop: Spacing.sm,
  },
  applyBtn: {
    height: 46,
    marginTop: Spacing.sm,
    borderRadius: BorderRadius.md,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.primaryBlue,
  },
  applyBtnDisabled: {
    opacity: 0.6,
  },
  applyBtnText: {
    color: Colors.white,
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.bodyMd,
  },
});
