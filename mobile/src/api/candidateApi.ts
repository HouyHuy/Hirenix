import { apiClient } from './apiClient';
import {
  CandidateProfile,
  CreateProfileDto,
  UpdateProfileDto,
  Education,
  CreateEducationDto,
  UpdateEducationDto,
  Experience,
  CreateExperienceDto,
  UpdateExperienceDto,
  CandidateSkill,
  AddSkillsDto,
  Skill,
} from '../types/candidate';

/**
 * Candidate Profile API Client
 */
export const candidateApi = {
  // ═══════════════════════════════════════════════════════════════════
  //  PROFILE OPERATIONS
  // ═══════════════════════════════════════════════════════════════════

  /**
   * Get my candidate profile
   */
  getMyProfile: () =>
    apiClient.get<{ success: boolean; data: CandidateProfile }>('/candidate/profile'),

  /**
   * Create candidate profile
   */
  createProfile: (data: CreateProfileDto) =>
    apiClient.post<{ success: boolean; data: CandidateProfile; message: string }>(
      '/candidate/profile',
      data
    ),

  /**
   * Update candidate profile
   */
  updateProfile: (data: UpdateProfileDto) =>
    apiClient.put<{ success: boolean; data: CandidateProfile; message: string }>(
      '/candidate/profile',
      data
    ),

  /**
   * Delete candidate profile
   */
  deleteProfile: () =>
    apiClient.delete<{ success: boolean; data: boolean; message: string }>(
      '/candidate/profile'
    ),

  // ═══════════════════════════════════════════════════════════════════
  //  EDUCATION OPERATIONS
  // ═══════════════════════════════════════════════════════════════════

  /**
   * Add education to profile
   */
  addEducation: (data: CreateEducationDto) =>
    apiClient.post<{ success: boolean; data: Education; message: string }>(
      '/candidate/education',
      data
    ),

  /**
   * Update education
   */
  updateEducation: (id: number, data: UpdateEducationDto) =>
    apiClient.put<{ success: boolean; data: Education; message: string }>(
      `/candidate/education/${id}`,
      data
    ),

  /**
   * Delete education
   */
  deleteEducation: (id: number) =>
    apiClient.delete<{ success: boolean; data: boolean; message: string }>(
      `/candidate/education/${id}`
    ),

  // ═══════════════════════════════════════════════════════════════════
  //  EXPERIENCE OPERATIONS
  // ═══════════════════════════════════════════════════════════════════

  /**
   * Add experience to profile
   */
  addExperience: (data: CreateExperienceDto) =>
    apiClient.post<{ success: boolean; data: Experience; message: string }>(
      '/candidate/experience',
      data
    ),

  /**
   * Update experience
   */
  updateExperience: (id: number, data: UpdateExperienceDto) =>
    apiClient.put<{ success: boolean; data: Experience; message: string }>(
      `/candidate/experience/${id}`,
      data
    ),

  /**
   * Delete experience
   */
  deleteExperience: (id: number) =>
    apiClient.delete<{ success: boolean; data: boolean; message: string }>(
      `/candidate/experience/${id}`
    ),

  // ═══════════════════════════════════════════════════════════════════
  //  SKILLS OPERATIONS
  // ═══════════════════════════════════════════════════════════════════

  /**
   * Add skills to profile
   */
  addSkills: (data: AddSkillsDto) =>
    apiClient.post<{ success: boolean; data: CandidateSkill[]; message: string }>(
      '/candidate/skills',
      data
    ),

  /**
   * Remove skill from profile
   */
  removeSkill: (skillId: number) =>
    apiClient.delete<{ success: boolean; data: boolean; message: string }>(
      `/candidate/skills/${skillId}`
    ),

  // ═══════════════════════════════════════════════════════════════════
  //  TAXONOMY (Skills, Industries, Locations)
  // ═══════════════════════════════════════════════════════════════════

  /**
   * Get all skills
   */
  getAllSkills: () =>
    apiClient.get<Skill[]>('/taxonomy/skills'),

  /**
   * Get skills by category
   */
  getSkillsByCategory: (category: string) =>
    apiClient.get<Skill[]>(`/taxonomy/skills?category=${category}`),
};

export default candidateApi;
