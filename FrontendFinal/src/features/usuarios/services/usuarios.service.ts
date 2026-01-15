import api from '@/lib/axios';

export interface Usuario {
    id: string;
    username: string;
    nombre: string;
    correo: string;
    telefono: string;
    direccion: string;
    rol: string;
    activo: boolean;
    fechaCreacion: string;
}

export interface ActualizarPerfilDto {
    nombre: string;
    telefono: string;
    direccion: string;
}

export interface CambiarPasswordDto {
    passwordActual: string;
    nuevoPassword: string;
}

export const usuariosService = {
    getUsuario: async (id: string): Promise<Usuario> => {
        const response = await api.get(`/usuarios/${id}`);
        return response.data;
    },

    actualizarPerfil: async (id: string, dto: ActualizarPerfilDto): Promise<void> => {
        await api.put(`/usuarios/${id}/perfil`, dto);
    },

    cambiarPassword: async (id: string, dto: CambiarPasswordDto): Promise<void> => {
        await api.post(`/usuarios/${id}/password`, dto);
    }
};
