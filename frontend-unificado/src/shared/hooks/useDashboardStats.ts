/**
 * useDashboardStats Hook
 * Custom hook for fetching dashboard statistics using React Query
 */

import { useQuery } from '@tanstack/react-query';
import { fetchDashboardStats } from '../services/dashboardService';
import type { DashboardStats } from '../types/dashboard';

export function useDashboardStats() {
  return useQuery<DashboardStats, Error>({
    queryKey: ['dashboard', 'stats'],
    queryFn: fetchDashboardStats,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: 3,
  });
}
