// Type definitions for the event management system

// Event types
export interface Evento {
  id: string;
  titulo: string;
  descripcion: string;
  fechaInicio: string;
  fechaFin: string;
  ubicacion: {
    nombreLugar: string;
    direccion: string;
    ciudad: string;
    pais: string;
  };
  maximoAsistentes: number;
  organizadorId?: string;
  publicado: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface EventoCreateDto {
  titulo: string;
  descripcion: string;
  fechaInicio: string;
  fechaFin: string;
  ubicacion: {
    nombreLugar: string;
    direccion: string;
    ciudad: string;
    pais: string;
  };
  maximoAsistentes: number;
  organizadorId?: string;
}

// Category types
export interface Categoria {
  id?: string; // Optional for backward compatibility
  categoriaId?: string; // Backend returns this field
  nombre: string;
  precioBase: number;
  tienePrioridad: boolean;
  mapaId?: string;
}

export interface CategoryCreateDto {
  nombre: string;
  precioBase: number;
  tienePrioridad: boolean;
  mapaId: string;
}

// Seat types
export interface Asiento {
  id: string;
  fila: string;
  numero: number;
  estado: 'Disponible' | 'Reservado' | 'Ocupado';
  categoriaId?: string;
  categoria?: Categoria;
  mapaId: string;
}

export interface SeatCreateDto {
  fila: string;
  numero: number;
  categoriaId: string;
  categoriaNombre: string; // Category name for backend
  mapaId: string;
  estado?: 'Disponible' | 'Reservado' | 'Ocupado';
}
