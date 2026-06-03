import React, { useEffect, useMemo, useState } from 'react';
import {
  ActivityIndicator,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { BorderRadius, Colors, Shadows, Spacing, Typography } from '../../constants/theme';
import { TextInput } from '../../components/TextInput';
import { Button } from '../../components/Button';
import { candidateApi } from '../../api/candidateApi';
import { employerJobApi } from '../../api/employerJobApi';
import {
  Currencies,
  EmploymentTypes,
  ExperienceLevels,
  UpdateJobDto,
  WorkModes,
} from '../../types/employer';
import { Industry, Location, Skill } from '../../types/candidate';
import { useToast } from '../../contexts/ToastContext';
import { JobFormErrors, JobFormState, validateJobForm } from './jobFormUtils';

const initialState: JobFormState = {
  title: '',
  description: '',
  requirements: '',
  responsibilities: '',
  benefits: '',
  salaryMin: '',
  salaryMax: '',
  currency: Currencies[0].value,
  employmentType: EmploymentTypes[0].value,
  experienceLevel: ExperienceLevels[0].value,
  workMode: WorkModes[0].value,
  locationId: null,
  industryId: null,
  expiryDate: '',
  skillIds: [],
};

const toUpdatePayload = (state: JobFormState): UpdateJobDto => ({
  title: state.title.trim(),
  description: state.description.trim(),
  requirements: state.requirements.trim(),
  responsibilities: state.responsibilities.trim(),
  benefits: state.benefits.trim() || undefined,
  salaryMin: state.salaryMin.trim() ? Number(state.salaryMin) : undefined,
  salaryMax: state.salaryMax.trim() ? Number(state.salaryMax) : undefined,
  currency: state.currency,
  employmentType: state.employmentType,
  experienceLevel: state.experienceLevel,
  workMode: state.workMode,
  locationId: state.locationId ?? undefined,
  industryId: state.industryId ?? undefined,
  skillIds: state.skillIds,
  expiryDate: state.expiryDate.trim(),
});


export default function EditJobScreen({ route, navigation }: any) {
  const { showToast } = useToast();
  const jobId = route?.params?.jobId as number;

  const [form, setForm] = useState<JobFormState>(initialState);
  const [errors, setErrors] = useState<JobFormErrors>({});
  const [skills, setSkills] = useState<Skill[]>([]);
  const [industries, setIndustries] = useState<Industry[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [job, skillsRes, industriesRes, locationsRes] = await Promise.all([
          employerJobApi.getJobById(jobId),
          candidateApi.getAllSkills(),
          candidateApi.getAllIndustries(),
          candidateApi.getAllLocations(),
        ]);

        setSkills(skillsRes.data.data ?? []);
        setIndustries(industriesRes.data.data ?? []);
        setLocations(locationsRes.data.data ?? []);

        setForm({
          title: job.title ?? '',
          description: job.description ?? '',
          requirements: job.requirements ?? '',
          responsibilities: job.responsibilities ?? '',
          benefits: job.benefits ?? '',
          salaryMin: job.salaryMin?.toString() ?? '',
          salaryMax: job.salaryMax?.toString() ?? '',
          currency: job.currency ?? Currencies[0].value,
          employmentType: job.employmentType ?? EmploymentTypes[0].value,
          experienceLevel: job.experienceLevel ?? ExperienceLevels[0].value,
          workMode: job.workMode ?? WorkModes[0].value,
          locationId: job.locationId ?? null,
          industryId: job.industryId ?? null,
          expiryDate: job.expiryDate ? String(job.expiryDate).split('T')[0] : '',
          skillIds: Array.isArray(job.skillIds) ? job.skillIds : [],
        });
      } catch (error: any) {
        showToast(error?.response?.data?.message || 'Failed to load job data', 'error');
        navigation.goBack();
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [jobId, navigation, showToast]);

  const selectedIndustryName = useMemo(
    () => industries.find((item) => item.id === form.industryId)?.name ?? 'Select industry',
    [industries, form.industryId]
  );

  const selectedLocationName = useMemo(
    () => locations.find((item) => item.id === form.locationId)?.name ?? 'Select location',
    [locations, form.locationId]
  );

  const setField = <K extends keyof JobFormState>(key: K, value: JobFormState[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }));
    if (errors[key]) {
      setErrors((prev) => ({ ...prev, [key]: undefined }));
    }
  };

  const toggleSkill = (skillId: number) => {
    setForm((prev) => {
      const exists = prev.skillIds.includes(skillId);
      return {
        ...prev,
        skillIds: exists ? prev.skillIds.filter((id) => id !== skillId) : [...prev.skillIds, skillId],
      };
    });

    if (errors.skillIds) {
      setErrors((prev) => ({ ...prev, skillIds: undefined }));
    }
  };

  const handleSave = async () => {
    const nextErrors = validateJobForm(form);
    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      showToast('Please fix form errors before saving', 'warning');
      return;
    }

    setSaving(true);
    try {
      const payload = toUpdatePayload(form);
      await employerJobApi.updateJob(jobId, payload);
      showToast('Job updated successfully', 'success');
      navigation.goBack();
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to update job', 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading job...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <Text style={styles.sectionTitle}>Basic information</Text>
        <TextInput label="Job title *" value={form.title} onChangeText={(v) => setField('title', v)} error={errors.title} />
        <TextInput
          label="Description *"
          value={form.description}
          onChangeText={(v) => setField('description', v)}
          error={errors.description}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />
        <TextInput
          label="Requirements *"
          value={form.requirements}
          onChangeText={(v) => setField('requirements', v)}
          error={errors.requirements}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />
        <TextInput
          label="Responsibilities *"
          value={form.responsibilities}
          onChangeText={(v) => setField('responsibilities', v)}
          error={errors.responsibilities}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />
        <TextInput
          label="Benefits"
          value={form.benefits}
          onChangeText={(v) => setField('benefits', v)}
          multiline
          numberOfLines={3}
          style={styles.multilineInput}
        />

        <Text style={styles.sectionTitle}>Classification</Text>
        <View style={styles.optionGrid}>
          {EmploymentTypes.map((item) => {
            const active = form.employmentType === item.value;
            return (
              <TouchableOpacity key={item.value} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('employmentType', item.value)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.optionGrid}>
          {ExperienceLevels.map((item) => {
            const active = form.experienceLevel === item.value;
            return (
              <TouchableOpacity key={item.value} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('experienceLevel', item.value)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.optionGrid}>
          {WorkModes.map((item) => {
            const active = form.workMode === item.value;
            return (
              <TouchableOpacity key={item.value} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('workMode', item.value)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <Text style={styles.sectionTitle}>Compensation</Text>
        <View style={styles.optionGrid}>
          {Currencies.map((item) => {
            const active = form.currency === item.value;
            return (
              <TouchableOpacity key={item.value} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('currency', item.value)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.inlineRow}>
          <View style={styles.inlineCol}>
            <TextInput label="Min salary" value={form.salaryMin} onChangeText={(v) => setField('salaryMin', v)} keyboardType="numeric" error={errors.salaryMin} />
          </View>
          <View style={styles.inlineCol}>
            <TextInput label="Max salary" value={form.salaryMax} onChangeText={(v) => setField('salaryMax', v)} keyboardType="numeric" error={errors.salaryMax} />
          </View>
        </View>

        <Text style={styles.sectionTitle}>Industry & Location</Text>

        <Text style={styles.fieldLabel}>Industry *</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.optionGridScroll}>
          {industries.map((item) => {
            const active = form.industryId === item.id;
            return (
              <TouchableOpacity key={item.id} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('industryId', item.id)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.name}</Text>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
        <Text style={styles.selectedText}>Selected: {selectedIndustryName}</Text>
        {errors.industryId ? <Text style={styles.errorText}>{errors.industryId}</Text> : null}

        <Text style={styles.fieldLabel}>Location *</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.optionGridScroll}>
          {locations.map((item) => {
            const active = form.locationId === item.id;
            return (
              <TouchableOpacity key={item.id} style={[styles.optionChip, active && styles.optionChipActive]} onPress={() => setField('locationId', item.id)}>
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.name}</Text>
              </TouchableOpacity>
            );
          })}
        </ScrollView>
        <Text style={styles.selectedText}>Selected: {selectedLocationName}</Text>
        {errors.locationId ? <Text style={styles.errorText}>{errors.locationId}</Text> : null}

        <Text style={styles.sectionTitle}>Skills *</Text>
        <View style={styles.skillWrap}>
          {skills.map((skill) => {
            const selected = form.skillIds.includes(skill.id);
            return (
              <TouchableOpacity key={skill.id} style={[styles.skillChip, selected && styles.skillChipSelected]} onPress={() => toggleSkill(skill.id)}>
                <Text style={[styles.skillChipText, selected && styles.skillChipTextSelected]}>{skill.name}</Text>
                {selected ? <Ionicons name="checkmark-circle" size={14} color={Colors.white} /> : null}
              </TouchableOpacity>
            );
          })}
        </View>
        {errors.skillIds ? <Text style={styles.errorText}>{errors.skillIds}</Text> : null}

        <TextInput
          label="Expiry date (YYYY-MM-DD) *"
          value={form.expiryDate}
          onChangeText={(v) => setField('expiryDate', v)}
          error={errors.expiryDate}
        />
      </ScrollView>

      <View style={styles.footer}>
        <Button title="Save Changes" onPress={handleSave} loading={saving} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.gray50 },
  center: { flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: Colors.gray50, gap: Spacing.sm },
  loadingText: { fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.bodyMd, color: Colors.gray600 },
  scrollContent: { paddingHorizontal: Spacing.base, paddingTop: Spacing.base, paddingBottom: 110 },
  sectionTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
    marginBottom: Spacing.sm,
    marginTop: Spacing.sm,
  },
  fieldLabel: { fontFamily: Typography.fontFamily.semibold, fontSize: Typography.size.bodySm, color: Colors.gray800, marginBottom: Spacing.xs },
  optionGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.sm, marginBottom: Spacing.sm },
  optionGridScroll: { gap: Spacing.sm, paddingBottom: Spacing.xs },
  optionChip: {
    borderWidth: 1,
    borderColor: Colors.gray200,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 6,
  },
  optionChipActive: { borderColor: Colors.primaryBlue, backgroundColor: Colors.primaryBlue },
  optionChipText: { color: Colors.gray800, fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.caption },
  optionChipTextActive: { color: Colors.white },
  selectedText: { marginTop: 4, marginBottom: Spacing.sm, color: Colors.gray600, fontFamily: Typography.fontFamily.regular, fontSize: Typography.size.caption },
  inlineRow: { flexDirection: 'row', gap: Spacing.sm },
  inlineCol: { flex: 1 },
  multilineInput: { textAlignVertical: 'top', minHeight: 96 },
  skillWrap: { flexDirection: 'row', flexWrap: 'wrap', gap: Spacing.sm, marginBottom: Spacing.sm },
  skillChip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    borderWidth: 1,
    borderColor: Colors.gray200,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.full,
    paddingVertical: 6,
    paddingHorizontal: Spacing.sm,
  },
  skillChipSelected: { borderColor: Colors.primaryBlue, backgroundColor: Colors.primaryBlue },
  skillChipText: { color: Colors.gray800, fontFamily: Typography.fontFamily.medium, fontSize: Typography.size.caption },
  skillChipTextSelected: { color: Colors.white },
  errorText: { fontFamily: Typography.fontFamily.regular, color: Colors.dangerRed, fontSize: Typography.size.caption, marginTop: 2, marginBottom: Spacing.xs },
  footer: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: 0,
    padding: Spacing.base,
    backgroundColor: Colors.white,
    borderTopWidth: 1,
    borderTopColor: Colors.gray100,
    ...Shadows.elevation1,
  },
});
