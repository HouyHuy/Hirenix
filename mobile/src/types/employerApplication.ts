export type ApplicationStatus = 'Applied' | 'Reviewing' | 'Shortlisted' | 'Rejected' | 'Accepted' | 'Withdrawn';

export interface EmployerApplicationDto {
  id: number;
  jobId: number;
  jobTitle: string;
  candidateId: number;
  candidateName: string;
  candidateEmail: string;
  candidatePhone?: string;
  candidatePhotoUrl?: string;
  cvUrl: string;
  coverLetter?: string;
  status: ApplicationStatus;
  appliedDate: string;
  reviewedDate?: string;
  reviewNotes?: string;
  yearsOfExperience: number;
  skills: string[];
  currentPosition?: string;
}

export interface ApplicationStatisticsDto {
  total: number;
  byStatus: Record<string, number>;
  byJob: Record<string, number>;
}

export interface UpdateApplicationStatusDto {
  status: ApplicationStatus;
  reviewNotes?: string;
}

export const ApplicationStatusOptions: Array<{ label: string; value: ApplicationStatus }> = [
  { label: 'Applied', value: 'Applied' },
  { label: 'Reviewing', value: 'Reviewing' },
  { label: 'Shortlisted', value: 'Shortlisted' },
  { label: 'Rejected', value: 'Rejected' },
  { label: 'Accepted', value: 'Accepted' },
  { label: 'Withdrawn', value: 'Withdrawn' },
];
