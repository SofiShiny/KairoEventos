/**
 * Dashboard Service
 * API calls for dashboard statistics and featured events
 */

import axiosClient from '../api/axiosClient';
import type { DashboardStats, EventoDestacado } from '../types/dashboard';

/**
 * Fetch dashboard statistics
 */
export async function fetchDashboardStats(): Promise<DashboardStats> {
  const response = await axiosClient.get<{ data: DashboardStats }>('/api/dashboard/stats');
  return response.data.data;
}

/**
 * Fetch featured events for dashboard
 */
export async function fetchEventosDestacados(): Promise<EventoDestacado[]> {
  const response = await axiosClient.get<{ data: EventoDestacado[] }>('/api/dashboard/eventos-destacados');
  return response.data.data;
}
