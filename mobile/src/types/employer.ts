/**
 * Employer Job Management Types
 * Matches backend DTOs from Hirenix.Application/DTOs/Job/
 */

export interface EmployerJobDto {
  id: number;
  title: string;
  description: string;
  requirements: string;
  responsibilities: string;
  benefits?: string;
  salaryMin?: number;
  salaryMax?: number;
  currency: string;
  employmentType: 'FullTime' | 'PartTime' | 'Contract' | 'Internship';
  experienceLevel: 'Entry' | 'Mid' | 'Senior' | 'Lead';
  workMode: 'Onsite' | 'Remote' | 'Hybrid';
  locationId: number;
  locationName: string;
  industryId: number;
  industryName: string;
  skillIds: number[];
  skillNames: string[];
  expiryDate: string; // ISO date string
  status: 'Active' | 'Closed';
  companyId: number;
  companyName: string;
  createdAt: string; // ISO date string
  updatedAt: string; // ISO date string
  applicationCount: number;
  viewCount: number;
  canEdit: boolean;
  canClose: boolean;
}

export interface CreateJobDto {
  title: string;
  description: string;
  requirements: string;
  responsibilities: string;
  benefits?: string;
  salaryMin?: number;
  salaryMax?: number;
  currency: string;
  employmentType: string;
  experienceLevel: string;
  workMode: string;
  locationId: number;
  industryId: number;
  skillIds: number[];
  expiryDate: string; // ISO date string (YYYY-MM-DD)
}

export interface UpdateJobDto {
  title?: string;
  description?: string;
  requirements?: string;
  responsibilities?: string;
  benefits?: string;
  salaryMin?: number;
  salaryMax?: number;
  currency?: string;
  employmentType?: string;
  experienceLevel?: string;
  workMode?: string;
  locationId?: number;
  industryId?: number;
  skillIds?: number[];
  expiryDate?: string; // ISO date string (YYYY-MM-DD)
}

// Enums for form dropdowns
export const EmploymentTypes = [
  { label: 'Full Time', value: 'FullTime' },
  { label: 'Part Time', value: 'PartTime' },
  { label: 'Contract', value: 'Contract' },
  { label: 'Internship', value: 'Internship' },
] as const;

export const ExperienceLevels = [
  { label: 'Entry Level', value: 'Entry' },
  { label: 'Mid Level', value: 'Mid' },
  { label: 'Senior Level', value: 'Senior' },
  { label: 'Lead/Principal', value: 'Lead' },
] as const;

export const WorkModes = [
  { label: 'Onsite', value: 'Onsite' },
  { label: 'Remote', value: 'Remote' },
  { label: 'Hybrid', value: 'Hybrid' },
] as const;

export const Currencies = [
  { label: 'USD ($)', value: 'USD' },
  { label: 'VND (₫)', value: 'VND' },
  { label: 'EUR (€)', value: 'EUR' },
  { label: 'GBP (£)', value: 'GBP' },
] as const;
