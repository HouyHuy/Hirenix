import { apiClient } from './apiClient';
import {
  ApplicationStatisticsDto,
  ApplicationStatus,
  EmployerApplicationDto,
  UpdateApplicationStatusDto,
} from '../types/employerApplication';

export const employerApplicationApi = {
  getApplications: async (jobId?: number, status?: ApplicationStatus): Promise<EmployerApplicationDto[]> => {
    const params: { jobId?: number; status?: ApplicationStatus } = {};
    if (jobId) params.jobId = jobId;
    if (status) params.status = status;

    const response = await apiClient.get('/employer/applications', { params });
    return response.data;
  },

  getApplicationById: async (id: number): Promise<EmployerApplicationDto> => {
    const response = await apiClient.get(`/employer/applications/${id}`);
    return response.data;
  },

  updateStatus: async (id: number, payload: UpdateApplicationStatusDto): Promise<void> => {
    await apiClient.put(`/employer/applications/${id}/status`, payload);
  },

  getStatistics: async (): Promise<ApplicationStatisticsDto> => {
    const response = await apiClient.get('/employer/applications/statistics');
    return response.data;
  },
};
