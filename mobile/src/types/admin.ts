export interface UsersByRole {
  candidate: number;
  employer: number;
}

export interface DashboardStats {
  totalUsers: number;
  totalJobs: number;
  totalApplications: number;
  totalCompanies: number;
  newUsersToday: number;
  newJobsToday: number;
  newApplicationsToday: number;
  activeJobsCount: number;
  pendingJobsCount: number;
  usersByRole: UsersByRole;
}

export interface AnalyticsPoint {
  date: string;
  count: number;
}

export interface AnalyticsData {
  period: string;
  usersGrowth: AnalyticsPoint[];
  jobsGrowth: AnalyticsPoint[];
  applicationsGrowth: AnalyticsPoint[];
}

export interface RecentActivity {
  id: number;
  type: string;
  description: string;
  timestamp: string;
  userId?: number;
  userName?: string;
  jobId?: number;
  jobTitle?: string;
  companyName?: string;
  metadata?: Record<string, unknown>;
}
