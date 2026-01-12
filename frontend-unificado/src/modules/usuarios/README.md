# Módulo de Usuarios

Este módulo maneja la gestión de usuarios del sistema. **Solo accesible para usuarios con rol Admin**.

## Estructura

```
usuarios/
├── components/       # Componentes UI específicos de usuarios
│   ├── UsuariosList.tsx    # Tabla de usuarios
│   ├── UsuarioForm.tsx     # Formulario crear/editar
│   └── index.ts
├── hooks/           # Custom hooks para usuarios
├── pages/           # Páginas del módulo
│   └── UsuariosPage.tsx    # Página principal de gestión
├── services/        # Servicios API
├── types/           # Tipos TypeScript
└── index.ts         # Barrel export
```

## Componentes UI

### `UsuariosList`

Tabla profesional que muestra todos los usuarios con:
- Columnas: Username, Nombre, Correo, Teléfono, Rol, Estado
- Chips de colores para roles (Admin: rojo, Organizator: naranja, Asistente: azul)
- Botones de acción: Editar y Desactivar
- Estados vacío y de carga

### `UsuarioForm`

Formulario en diálogo para crear/editar usuarios con:
- Validación en tiempo real con Zod + react-hook-form
- Campos: Username, Nombre, Correo, Teléfono, Rol, Password
- Mensajes de error específicos por campo
- Botón de envío deshabilitado hasta que el formulario sea válido

### `UsuariosPage`

Página principal que integra todos los componentes:
- Botón "Crear Usuario"
- Lista de usuarios con acciones
- Diálogo de confirmación para desactivación
- Manejo de errores con retry
- Notificaciones toast para éxito/error

## Servicios API

### `usuariosService.ts`

Funciones para comunicación con el Gateway:

- `fetchUsuarios()` - Obtener lista de todos los usuarios
- `fetchUsuario(id)` - Obtener detalle de un usuario
- `createUsuario(data)` - Crear nuevo usuario
- `updateUsuario(id, data)` - Actualizar usuario existente
- `deactivateUsuario(id)` - Desactivar usuario (soft delete)

## Hooks

### `useUsuarios()`

Hook para obtener lista de usuarios con React Query.

```typescript
const { data: usuarios, isLoading, error, refetch } = useUsuarios();
```

### `useUsuario(id)`

Hook para obtener detalle de un usuario específico.

```typescript
const { data: usuario, isLoading, error } = useUsuario('123');
```

### `useCreateUsuario()`

Hook para crear un nuevo usuario.

```typescript
const { mutate: createUsuario, isPending, error } = useCreateUsuario();

const handleSubmit = (data: CreateUsuarioDto) => {
  createUsuario(data, {
    onSuccess: () => {
      toast.success('Usuario creado exitosamente');
      navigate('/usuarios');
    },
    onError: (error) => {
      toast.error('Error al crear usuario');
    }
  });
};
```

### `useUpdateUsuario()`

Hook para actualizar un usuario existente.

```typescript
const { mutate: updateUsuario, isPending, error } = useUpdateUsuario();

const handleSubmit = (data: UpdateUsuarioDto) => {
  updateUsuario({ id: '123', data }, {
    onSuccess: () => {
      toast.success('Usuario actualizado exitosamente');
    }
  });
};
```

### `useDeactivateUsuario()`

Hook para desactivar un usuario.

```typescript
const { mutate: deactivateUsuario, isPending, error } = useDeactivateUsuario();

const handleDeactivate = (usuarioId: string) => {
  if (confirm('¿Está seguro de desactivar este usuario?')) {
    deactivateUsuario(usuarioId, {
      onSuccess: () => {
        toast.success('Usuario desactivado exitosamente');
      }
    });
  }
};
```

## Tipos

### `Usuario`

```typescript
interface Usuario {
  id: string;
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: 'Admin' | 'Organizator' | 'Asistente';
  activo: boolean;
  fechaCreacion?: string;
  fechaActualizacion?: string;
}
```

### `CreateUsuarioDto`

```typescript
interface CreateUsuarioDto {
  username: string;
  nombre: string;
  correo: string;
  telefono: string;
  rol: RolUsuario;
  password: string;
}
```

### `UpdateUsuarioDto`

```typescript
interface UpdateUsuarioDto {
  nombre?: string;
  correo?: string;
  telefono?: string;
  rol?: RolUsuario;
}
```

## Validaciones

El módulo implementa las siguientes validaciones:

- **Username**: Único, mínimo 3 caracteres, máximo 50
- **Correo**: Formato de email válido, único
- **Teléfono**: Formato válido (regex: `^\+?[0-9]{10,15}$`)
- **Rol**: Debe ser uno de: Admin, Organizator, Asistente
- **Password**: Mínimo 8 caracteres (solo en creación)

## Control de Acceso

**IMPORTANTE**: Todas las funcionalidades de este módulo están restringidas a usuarios con rol **Admin**.

El control de acceso se implementa en:
1. **Frontend**: Rutas protegidas con `RoleBasedRoute`
2. **Backend**: Gateway valida roles en cada petición

## Ejemplo de Uso Completo

```typescript
import { useUsuarios, useCreateUsuario, useDeactivateUsuario } from '@modules/usuarios';

function UsuariosPage() {
  const { data: usuarios, isLoading } = useUsuarios();
  const { mutate: createUsuario } = useCreateUsuario();
  const { mutate: deactivateUsuario } = useDeactivateUsuario();

  if (isLoading) return <LoadingSpinner />;

  return (
    <div>
      <h1>Gestión de Usuarios</h1>
      <Button onClick={() => setShowCreateModal(true)}>
        Crear Usuario
      </Button>
      
      <UsuariosList 
        usuarios={usuarios}
        onDeactivate={deactivateUsuario}
      />
    </div>
  );
}
```

## Integración con Gateway

Todas las peticiones se realizan a través del Gateway:

- Base URL: `${VITE_GATEWAY_URL}/api/usuarios`
- Autenticación: Bearer token en header Authorization
- Formato de respuesta: `{ data: T, success: boolean, message?: string }`

## Caché y Sincronización

React Query maneja automáticamente:

- **Caché**: 5 minutos de staleTime
- **Invalidación**: Al crear, actualizar o desactivar usuarios
- **Retry**: 3 intentos con backoff exponencial
- **Refetch**: Manual con `refetch()` o automático al invalidar

## Requisitos Relacionados

Este módulo implementa los siguientes requisitos:

- **Requirement 10.1**: Mostrar opción "Usuarios" solo para Admin
- **Requirement 10.2**: Ruta `/usuarios` accesible solo para Admin
- **Requirement 10.6**: Operaciones CRUD de usuarios a través del Gateway
