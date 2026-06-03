export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface PaginationMetadata {
  currentPage: number;
  pageSize: number;
  totalPages: number;
  totalItems: number;
}

export interface PaginatedResult<T> {
  data: T[];
  pagination: PaginationMetadata;
}

export interface JobListItemDto {
  id: number;
  title: string;
  companyName: string;
  companyLogo?: string;
  cityName?: string;
  workType: string;
  level: string;
  salaryMin?: number;
  salaryMax?: number;
  isSalaryVisible: boolean;
  deadline: string;
  status: string;
  isFeatured: boolean;
  viewsCount: number;
  applicationsCount: number;
  skills: string[];
  createdAt: string;
}

export interface FilterLocationOptionDto {
  id: number;
  name: string;
}

export interface FilterIndustryOptionDto {
  id: number;
  name: string;
}

export interface FilterWorkTypeOptionDto {
  value: number;
  name: string;
}

export interface FilterLevelOptionDto {
  value: number;
  name: string;
}

export interface FilterOptionsDto {
  cities: FilterLocationOptionDto[];
  industries: FilterIndustryOptionDto[];
  workTypes: FilterWorkTypeOptionDto[];
  levels: FilterLevelOptionDto[];
}

export interface JobDetailCompanyDto {
  id: number;
  name: string;
  logoUrl?: string;
  website?: string;
  description?: string;
}

export interface JobDetailSkillDto {
  id: number;
  name: string;
  isRequired: boolean;
}

export interface JobDetailDto {
  id: number;
  title: string;
  description: string;
  requirements?: string;
  benefits?: string;
  workType: string;
  level: string;
  salaryMin?: number;
  salaryMax?: number;
  isSalaryVisible: boolean;
  deadline: string;
  status: string;
  isFeatured: boolean;
  viewsCount: number;
  applicationsCount: number;
  createdAt: string;
  updatedAt: string;
  company: JobDetailCompanyDto;
  city?: string;
  industry?: string;
  skills: JobDetailSkillDto[];
  hasApplied: boolean;
}

export interface JobFilterParams {
  search?: string;
  cityId?: number;
  industryId?: number;
  sortBy?: 'CreatedAt' | 'salary' | 'title' | 'deadline';
  page?: number;
  pageSize?: number;
}

export interface SubmitApplicationDto {
  jobId: number;
  cvFileUri: string;
  cvFileName: string;
  coverLetter?: string;
}
