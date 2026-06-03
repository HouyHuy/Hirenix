export type JobFormState = {
  title: string;
  description: string;
  requirements: string;
  responsibilities: string;
  benefits: string;
  salaryMin: string;
  salaryMax: string;
  currency: string;
  employmentType: string;
  experienceLevel: string;
  workMode: string;
  locationId: number | null;
  industryId: number | null;
  expiryDate: string;
  skillIds: number[];
};

export type JobFormErrors = Partial<Record<keyof JobFormState, string>>;

const isoDateRegex = /^\d{4}-\d{2}-\d{2}$/;

export const validateJobForm = (state: JobFormState): JobFormErrors => {
  const errors: JobFormErrors = {};

  if (!state.title.trim()) errors.title = 'Title is required';
  if (!state.description.trim()) errors.description = 'Description is required';
  if (!state.requirements.trim()) errors.requirements = 'Requirements are required';
  if (!state.responsibilities.trim()) errors.responsibilities = 'Responsibilities are required';
  if (!state.locationId) errors.locationId = 'Please choose a location';
  if (!state.industryId) errors.industryId = 'Please choose an industry';
  if (state.skillIds.length === 0) errors.skillIds = 'Please choose at least one skill';

  if (!state.expiryDate.trim()) {
    errors.expiryDate = 'Expiry date is required';
  } else if (!isoDateRegex.test(state.expiryDate.trim())) {
    errors.expiryDate = 'Use format YYYY-MM-DD';
  } else {
    const date = new Date(`${state.expiryDate.trim()}T00:00:00`);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (Number.isNaN(date.getTime())) {
      errors.expiryDate = 'Invalid date';
    } else if (date <= today) {
      errors.expiryDate = 'Expiry date must be in the future';
    }
  }

  const salaryMin = state.salaryMin.trim() ? Number(state.salaryMin) : null;
  const salaryMax = state.salaryMax.trim() ? Number(state.salaryMax) : null;

  if (salaryMin !== null && (Number.isNaN(salaryMin) || salaryMin < 0)) {
    errors.salaryMin = 'Minimum salary must be >= 0';
  }

  if (salaryMax !== null && (Number.isNaN(salaryMax) || salaryMax < 0)) {
    errors.salaryMax = 'Maximum salary must be >= 0';
  }

  if (salaryMin !== null && salaryMax !== null && salaryMin > salaryMax) {
    errors.salaryMax = 'Maximum salary must be >= minimum salary';
  }

  return errors;
};
