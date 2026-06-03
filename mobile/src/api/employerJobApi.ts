import { apiClient } from './apiClient';
import { EmployerJobDto, CreateJobDto, UpdateJobDto } from '../types/employer';

export const employerJobApi = {
  /**
   * Get list of jobs posted by the employer
   * @param status - Optional filter by job status (Active or Closed)
   * @returns Array of employer job DTOs
   */
  getMyJobs: async (status?: 'Active' | 'Closed'): Promise<EmployerJobDto[]> => {
    const params = status ? { status } : {};
    const response = await apiClient.get('/employer/jobs', { params });
    return response.data;
  },

  /**
   * Get single job details by ID
   * @param jobId - The job ID
   * @returns Employer job DTO with full details
   */
  getJobById: async (jobId: number): Promise<EmployerJobDto> => {
    const response = await apiClient.get(`/employer/jobs/${jobId}`);
    return response.data;
  },

  /**
   * Create a new job posting
   * @param data - Job creation data
   * @returns Created job DTO
   */
  createJob: async (data: CreateJobDto): Promise<EmployerJobDto> => {
    const response = await apiClient.post('/employer/jobs', data);
    return response.data;
  },

  /**
   * Update an existing job
   * @param jobId - The job ID to update
   * @param data - Partial job update data
   * @returns Updated job DTO
   */
  updateJob: async (jobId: number, data: UpdateJobDto): Promise<EmployerJobDto> => {
    const response = await apiClient.put(`/employer/jobs/${jobId}`, data);
    return response.data;
  },

  /**
   * Close an active job posting
   * @param jobId - The job ID to close
   * @returns Success boolean
   */
  closeJob: async (jobId: number): Promise<boolean> => {
    const response = await apiClient.post(`/employer/jobs/${jobId}/close`);
    return response.data;
  },

  /**
   * Delete a job posting (only if no applications)
   * @param jobId - The job ID to delete
   */
  deleteJob: async (jobId: number): Promise<void> => {
    await apiClient.delete(`/employer/jobs/${jobId}`);
  },
};
