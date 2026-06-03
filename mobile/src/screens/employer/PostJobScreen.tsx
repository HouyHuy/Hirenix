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
  CreateJobDto,
  Currencies,
  EmploymentTypes,
  ExperienceLevels,
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

const toCreateJobPayload = (state: JobFormState): CreateJobDto => {
  return {
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
    locationId: state.locationId ?? 0,
    industryId: state.industryId ?? 0,
    skillIds: state.skillIds,
    expiryDate: state.expiryDate.trim(),
  };
};


export default function PostJobScreen({ navigation }: any) {
  const { showToast } = useToast();
  const [form, setForm] = useState<JobFormState>(initialState);
  const [errors, setErrors] = useState<JobFormErrors>({});
  const [skills, setSkills] = useState<Skill[]>([]);
  const [industries, setIndustries] = useState<Industry[]>([]);
  const [locations, setLocations] = useState<Location[]>([]);
  const [loadingTaxonomy, setLoadingTaxonomy] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const loadTaxonomy = async () => {
      try {
        const [skillsRes, industriesRes, locationsRes] = await Promise.all([
          candidateApi.getAllSkills(),
          candidateApi.getAllIndustries(),
          candidateApi.getAllLocations(),
        ]);

        setSkills(skillsRes.data.data ?? []);
        setIndustries(industriesRes.data.data ?? []);
        setLocations(locationsRes.data.data ?? []);
      } catch (error: any) {
        showToast(error?.response?.data?.message || 'Failed to load form data', 'error');
      } finally {
        setLoadingTaxonomy(false);
      }
    };

    loadTaxonomy();
  }, [showToast]);

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
      const nextSkillIds = exists
        ? prev.skillIds.filter((id) => id !== skillId)
        : [...prev.skillIds, skillId];

      return { ...prev, skillIds: nextSkillIds };
    });

    if (errors.skillIds) {
      setErrors((prev) => ({ ...prev, skillIds: undefined }));
    }
  };

  const handleSubmit = async () => {
    const formErrors = validateJobForm(form);
    setErrors(formErrors);

    if (Object.keys(formErrors).length > 0) {
      showToast('Please fix form errors before submitting', 'warning');
      return;
    }

    setSubmitting(true);
    try {
      const payload = toCreateJobPayload(form);
      await employerJobApi.createJob(payload);
      showToast('Job posted successfully', 'success');
      navigation.goBack();
    } catch (error: any) {
      showToast(error?.response?.data?.message || 'Failed to post job', 'error');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingTaxonomy) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color={Colors.primaryBlue} />
        <Text style={styles.loadingText}>Loading form data...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <ScrollView contentContainerStyle={styles.scrollContent} showsVerticalScrollIndicator={false}>
        <Text style={styles.sectionTitle}>Basic information</Text>
        <TextInput
          label="Job title *"
          placeholder="e.g. Senior Frontend Developer"
          value={form.title}
          onChangeText={(text) => setField('title', text)}
          error={errors.title}
        />

        <TextInput
          label="Description *"
          placeholder="Describe the role"
          value={form.description}
          onChangeText={(text) => setField('description', text)}
          error={errors.description}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />

        <TextInput
          label="Requirements *"
          placeholder="Required skills and experience"
          value={form.requirements}
          onChangeText={(text) => setField('requirements', text)}
          error={errors.requirements}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />

        <TextInput
          label="Responsibilities *"
          placeholder="Main responsibilities"
          value={form.responsibilities}
          onChangeText={(text) => setField('responsibilities', text)}
          error={errors.responsibilities}
          multiline
          numberOfLines={4}
          style={styles.multilineInput}
        />

        <TextInput
          label="Benefits"
          placeholder="Benefits and perks"
          value={form.benefits}
          onChangeText={(text) => setField('benefits', text)}
          multiline
          numberOfLines={3}
          style={styles.multilineInput}
        />

        <Text style={styles.sectionTitle}>Classification</Text>
        <View style={styles.optionGrid}>
          {EmploymentTypes.map((item) => {
            const active = form.employmentType === item.value;
            return (
              <TouchableOpacity
                key={item.value}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('employmentType', item.value)}
              >
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.optionGrid}>
          {ExperienceLevels.map((item) => {
            const active = form.experienceLevel === item.value;
            return (
              <TouchableOpacity
                key={item.value}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('experienceLevel', item.value)}
              >
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.optionGrid}>
          {WorkModes.map((item) => {
            const active = form.workMode === item.value;
            return (
              <TouchableOpacity
                key={item.value}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('workMode', item.value)}
              >
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
              <TouchableOpacity
                key={item.value}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('currency', item.value)}
              >
                <Text style={[styles.optionChipText, active && styles.optionChipTextActive]}>{item.label}</Text>
              </TouchableOpacity>
            );
          })}
        </View>

        <View style={styles.inlineRow}>
          <View style={styles.inlineCol}>
            <TextInput
              label="Min salary"
              placeholder="0"
              value={form.salaryMin}
              onChangeText={(text) => setField('salaryMin', text)}
              keyboardType="numeric"
              error={errors.salaryMin}
            />
          </View>
          <View style={styles.inlineCol}>
            <TextInput
              label="Max salary"
              placeholder="0"
              value={form.salaryMax}
              onChangeText={(text) => setField('salaryMax', text)}
              keyboardType="numeric"
              error={errors.salaryMax}
            />
          </View>
        </View>

        <Text style={styles.sectionTitle}>Industry & Location</Text>

        <Text style={styles.fieldLabel}>Industry *</Text>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.optionGridScroll}>
          {industries.map((item) => {
            const active = form.industryId === item.id;
            return (
              <TouchableOpacity
                key={item.id}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('industryId', item.id)}
              >
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
              <TouchableOpacity
                key={item.id}
                style={[styles.optionChip, active && styles.optionChipActive]}
                onPress={() => setField('locationId', item.id)}
              >
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
              <TouchableOpacity
                key={skill.id}
                style={[styles.skillChip, selected && styles.skillChipSelected]}
                onPress={() => toggleSkill(skill.id)}
              >
                <Text style={[styles.skillChipText, selected && styles.skillChipTextSelected]}>{skill.name}</Text>
                {selected ? <Ionicons name="checkmark-circle" size={14} color={Colors.white} /> : null}
              </TouchableOpacity>
            );
          })}
        </View>
        {errors.skillIds ? <Text style={styles.errorText}>{errors.skillIds}</Text> : null}

        <Text style={styles.sectionTitle}>Expiry Date</Text>
        <TextInput
          label="Expiry date (YYYY-MM-DD) *"
          placeholder="e.g. 2026-12-31"
          value={form.expiryDate}
          onChangeText={(text) => setField('expiryDate', text)}
          error={errors.expiryDate}
        />
      </ScrollView>

      <View style={styles.footer}>
        <Button title="Post Job" onPress={handleSubmit} loading={submitting} />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: Colors.gray50,
  },
  center: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: Colors.gray50,
    gap: Spacing.sm,
  },
  loadingText: {
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.bodyMd,
    color: Colors.gray600,
  },
  scrollContent: {
    paddingHorizontal: Spacing.base,
    paddingTop: Spacing.base,
    paddingBottom: 110,
  },
  sectionTitle: {
    fontFamily: Typography.fontFamily.bold,
    fontSize: Typography.size.headingMd,
    color: Colors.gray900,
    marginBottom: Spacing.sm,
    marginTop: Spacing.sm,
  },
  fieldLabel: {
    fontFamily: Typography.fontFamily.semibold,
    fontSize: Typography.size.bodySm,
    color: Colors.gray800,
    marginBottom: Spacing.xs,
  },
  optionGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
    marginBottom: Spacing.sm,
  },
  optionGridScroll: {
    gap: Spacing.sm,
    paddingBottom: Spacing.xs,
  },
  optionChip: {
    borderWidth: 1,
    borderColor: Colors.gray200,
    backgroundColor: Colors.white,
    borderRadius: BorderRadius.full,
    paddingHorizontal: Spacing.sm,
    paddingVertical: 6,
  },
  optionChipActive: {
    borderColor: Colors.primaryBlue,
    backgroundColor: Colors.primaryBlue,
  },
  optionChipText: {
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  optionChipTextActive: {
    color: Colors.white,
  },
  selectedText: {
    marginTop: 4,
    marginBottom: Spacing.sm,
    color: Colors.gray600,
    fontFamily: Typography.fontFamily.regular,
    fontSize: Typography.size.caption,
  },
  inlineRow: {
    flexDirection: 'row',
    gap: Spacing.sm,
  },
  inlineCol: {
    flex: 1,
  },
  multilineInput: {
    textAlignVertical: 'top',
    minHeight: 96,
  },
  skillWrap: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: Spacing.sm,
    marginBottom: Spacing.sm,
  },
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
  skillChipSelected: {
    borderColor: Colors.primaryBlue,
    backgroundColor: Colors.primaryBlue,
  },
  skillChipText: {
    color: Colors.gray800,
    fontFamily: Typography.fontFamily.medium,
    fontSize: Typography.size.caption,
  },
  skillChipTextSelected: {
    color: Colors.white,
  },
  errorText: {
    fontFamily: Typography.fontFamily.regular,
    color: Colors.dangerRed,
    fontSize: Typography.size.caption,
    marginTop: 2,
    marginBottom: Spacing.xs,
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
    ...Shadows.elevation1,
  },
});
