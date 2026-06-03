import { apiClient } from './apiClient';
import {
  ApiResponse,
  FilterOptionsDto,
  JobDetailDto,
  JobFilterParams,
  JobListItemDto,
  PaginatedResult,
  SubmitApplicationDto,
} from '../types/job';

export const jobApi = {
  getJobs: async (params: JobFilterParams = {}) => {
    const response = await apiClient.get<ApiResponse<PaginatedResult<JobListItemDto>>>('/api/jobs', {
      params,
    });
    return response.data;
  },

  getJobDetail: async (jobId: number) => {
    const response = await apiClient.get<ApiResponse<JobDetailDto>>(`/api/jobs/${jobId}/detail`);
    return response.data;
  },

  getFilterOptions: async () => {
    const response = await apiClient.get<ApiResponse<FilterOptionsDto>>('/api/jobs/filters');
    return response.data;
  },

  submitApplication: async ({ jobId, cvFileUri, cvFileName, coverLetter }: SubmitApplicationDto) => {
    const form = new FormData();
    form.append('jobId', String(jobId));

    if (coverLetter?.trim()) {
      form.append('coverLetter', coverLetter.trim());
    }

    form.append('cvFile', {
      uri: cvFileUri,
      name: cvFileName,
      type: 'application/pdf',
    } as any);

    const response = await apiClient.post('/api/applications', form, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return response.data;
  },
};
