/**
 * Candidate Profile Types
 */

export interface CandidateProfile {
  id: number;
  fullName: string;
  avatarUrl?: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: string;
  address?: string;
  cityId?: number;
  cityName?: string;
  expectedSalaryMin?: number;
  expectedSalaryMax?: number;
  desiredPosition?: string;
  workType?: 'Fulltime' | 'Parttime' | 'Intern' | 'Remote';
  level?: 'Junior' | 'Mid' | 'Senior' | 'Manager';
  industryId?: number;
  industryName?: string;
  isOpenToWork: boolean;
  isProfileHidden: boolean;
  bio?: string;
  createdAt: string;
  updatedAt: string;
  educations: Education[];
  experiences: Experience[];
  skills: CandidateSkill[];
}

export interface Education {
  id: number;
  schoolName: string;
  degree?: string;
  major?: string;
  startYear: number;
  endYear?: number;
  description?: string;
}

export interface Experience {
  id: number;
  companyName: string;
  position: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
}

export interface CandidateSkill {
  id: number;
  skillId: number;
  skillName: string;
  category?: string;
  level?: 'Beginner' | 'Intermediate' | 'Advanced';
}

export interface Skill {
  id: number;
  name: string;
  slug: string;
  category?: string;
}

// Request DTOs
export interface CreateProfileDto {
  fullName: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: string;
  address?: string;
  cityId?: number;
  expectedSalaryMin?: number;
  expectedSalaryMax?: number;
  desiredPosition?: string;
  workType?: 'Fulltime' | 'Parttime' | 'Intern' | 'Remote';
  level?: 'Junior' | 'Mid' | 'Senior' | 'Manager';
  industryId?: number;
  isOpenToWork?: boolean;
  bio?: string;
  skillIds?: number[];
}

export interface UpdateProfileDto {
  fullName: string;
  gender?: 'Male' | 'Female' | 'Other';
  dateOfBirth?: string;
  address?: string;
  cityId?: number;
  expectedSalaryMin?: number;
  expectedSalaryMax?: number;
  desiredPosition?: string;
  workType?: 'Fulltime' | 'Parttime' | 'Intern' | 'Remote';
  level?: 'Junior' | 'Mid' | 'Senior' | 'Manager';
  industryId?: number;
  isOpenToWork: boolean;
  isProfileHidden: boolean;
  bio?: string;
}

export interface CreateEducationDto {
  schoolName: string;
  degree?: string;
  major?: string;
  startYear: number;
  endYear?: number;
  description?: string;
}

export interface UpdateEducationDto {
  schoolName: string;
  degree?: string;
  major?: string;
  startYear: number;
  endYear?: number;
  description?: string;
}

export interface CreateExperienceDto {
  companyName: string;
  position: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
}

export interface UpdateExperienceDto {
  companyName: string;
  position: string;
  startDate: string;
  endDate?: string;
  isCurrent: boolean;
  description?: string;
}

export interface AddSkillsDto {
  skills: SkillWithLevel[];
}

export interface SkillWithLevel {
  skillId: number;
  level?: 'Beginner' | 'Intermediate' | 'Advanced';
}
