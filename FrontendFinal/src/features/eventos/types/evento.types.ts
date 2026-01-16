export interface Evento {
    id: string;
    titulo: string;
    descripcion: string;
    fechaInicio: string; // ISO String from backend
    fechaFin: string;    // ISO String from backend
    lugar: string;
    imagenUrl: string | null;
    categoria: string;
    estado: string;
    maximoAsistentes: number;
    organizadorId: string;
    esVirtual?: boolean;
    precioBase?: number;
}
