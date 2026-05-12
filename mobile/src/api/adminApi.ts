import { apiClient } from './apiClient';
import { AnalyticsData, DashboardStats, RecentActivity } from '../types/admin';

// Helper to safely unwrap API responses with fallback values
const unwrapData = <T>(data: any, fallback: T): T => {
  if (data === null || data === undefined) {
    return fallback;
  }
  return data;
};

export const adminApi = {
  getDashboardStats: async (): Promise<DashboardStats> => {
    try {
      const response = await apiClient.get<{ success: boolean; message: string; data: DashboardStats }>(
        '/api/admin/dashboard/stats'
      );
      // Backend returns { success, message, data } - unwrap to get actual data
      const actualData = response.data?.data ?? response.data;
      return unwrapData(actualData, {} as DashboardStats);
    } catch (error) {
      console.error('getDashboardStats error:', error);
      return {} as DashboardStats;
    }
  },

  getAnalytics: async (period: string): Promise<AnalyticsData> => {
    try {
      const response = await apiClient.get<{ success: boolean; message: string; data: AnalyticsData }>(
        `/api/admin/dashboard/analytics?period=${encodeURIComponent(period)}`
      );
      // Backend returns { success, message, data } - unwrap to get actual data
      const actualData = response.data?.data ?? response.data;
      return unwrapData(actualData, {} as AnalyticsData);
    } catch (error) {
      console.error('getAnalytics error:', error);
      return {} as AnalyticsData;
    }
  },

  getRecentActivities: async (limit = 10): Promise<RecentActivity[]> => {
    try {
      const response = await apiClient.get<{ success: boolean; message: string; data: RecentActivity[] }>(
        `/api/admin/dashboard/recent-activities?limit=${limit}`
      );
      // Backend returns { success, message, data } - unwrap to get actual data
      const wrappedData = response.data;
      
      // Handle multiple response shapes:
      // 1. { success, message, data: [...] }
      // 2. { success, message, data: { activities: [...] } }
      // 3. Direct array [...]
      
      if (wrappedData?.data) {
        // Case 1 or 2: wrapped response
        const innerData = wrappedData.data;
        if (Array.isArray(innerData)) {
          return innerData; // Case 1
        }
        if (innerData && Array.isArray((innerData as any).activities)) {
          return (innerData as any).activities; // Case 2
        }
      }
      
      // Case 3: direct array
      if (Array.isArray(wrappedData)) {
        return wrappedData;
      }
      
      return [];
    } catch (error) {
      console.error('getRecentActivities error:', error);
      return [];
    }
  },
};
