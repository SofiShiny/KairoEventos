/**
 * Dashboard Types
 * Types for dashboard statistics and data
 */

export interface DashboardStats {
  totalEventos: number;
  misEntradas: number;
  proximosEventos: number;
  totalUsuarios?: number; // Solo para Admin
  eventosCreados?: number; // Solo para Organizator
}

export interface EventoDestacado {
  id: string;
  nombre: string;
  descripcion: string;
  fecha: string;
  ubicacion: string;
  imagenUrl?: string;
  asientosDisponibles: number;
  capacidadTotal: number;
}

export interface QuickAction {
  label: string;
  path: string;
  icon: string;
  description: string;
  requiredRoles?: string[];
}
