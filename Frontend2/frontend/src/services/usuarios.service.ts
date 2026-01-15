import axios from 'axios';

const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8080';

export interface ActualizarPerfilDto {
    nombre: string;
    telefono: string;
    direccion: string;
}

export interface UsuarioDto {
    id: string;
    username: string;
    nombre: string;
    correo: string;
    telefono: { valor: string };
    direccion: { valor: string };
    activo: boolean;
}

class UsuariosService {
    /**
     * Obtiene el perfil de un usuario por ID
     */
    async getUsuario(id: string): Promise<UsuarioDto> {
        const response = await axios.get<UsuarioDto>(
            `${GATEWAY_URL}/api/usuarios/${id}`
        );
        return response.data;
    }

    /**
     * Actualiza el perfil del usuario
     */
    async actualizarPerfil(id: string, data: ActualizarPerfilDto): Promise<void> {
        await axios.put(
            `${GATEWAY_URL}/api/usuarios/${id}/perfil`,
            data
        );
    }
}

export const usuariosService = new UsuariosService();
