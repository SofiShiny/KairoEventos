# 👥 Gestión de Usuarios - Guía de Uso

## 📋 Descripción

La funcionalidad de **Gestión de Usuarios** permite a los administradores crear y gestionar cuentas de organizadores y otros administradores sin que estos tengan que registrarse manualmente.

---

## 🎯 Acceso

### Requisitos
- ✅ Estar autenticado como **Administrador**
- ✅ Tener el rol `admin` en Keycloak

### Cómo Acceder
1. Inicia sesión en el sistema
2. Ve al **Panel de Administración** (`/admin`)
3. Haz clic en **"Usuarios"** en el menú lateral

---

## 🚀 Funcionalidades

### 1. **Ver Lista de Usuarios**

Al entrar a la página verás:
- **Tabla completa** de todos los usuarios del sistema
- **Estadísticas** en la parte superior:
  - Total de usuarios
  - Cantidad de organizadores
  - Cantidad de administradores

### 2. **Crear Nuevo Usuario/Organizador**

#### Pasos:
1. Haz clic en el botón **"Crear Usuario"** (esquina superior derecha)
2. Completa el formulario:
   - **Nombre de Usuario**: Mínimo 3 caracteres (ej: `organizador1`)
   - **Email**: Email válido (ej: `organizador@kairo.com`)
   - **Nombre**: Nombre real del usuario
   - **Apellido**: Apellido del usuario
   - **Contraseña**: Mínimo 8 caracteres
   - **Rol**: Selecciona `Organizador` o `Administrador`
3. Haz clic en **"Crear Usuario"**

#### Validaciones:
- ✅ Username mínimo 3 caracteres
- ✅ Email válido
- ✅ Contraseña mínimo 8 caracteres
- ✅ Todos los campos son obligatorios

### 3. **Habilitar/Deshabilitar Usuario**

- Haz clic en el ícono de **Ban** (🚫) para deshabilitar un usuario activo
- Haz clic en el ícono de **CheckCircle** (✓) para habilitar un usuario inactivo
- Los usuarios deshabilitados no pueden iniciar sesión

### 4. **Eliminar Usuario**

- Haz clic en el ícono de **Trash** (🗑️)
- Confirma la acción en el diálogo
- **⚠️ Acción irreversible**

---

## 🎨 Interfaz

### Tabla de Usuarios

| Columna | Descripción |
|---------|-------------|
| **Usuario** | Username y nombre completo |
| **Email** | Correo electrónico |
| **Rol** | Badge con el rol (Organizador/Administrador/Usuario) |
| **Estado** | Activo/Inactivo |
| **Fecha Creación** | Cuándo se creó la cuenta |
| **Acciones** | Botones para habilitar/deshabilitar y eliminar |

### Badges de Roles

- 🔴 **Administrador**: Rojo
- 🟣 **Organizador**: Púrpura
- ⚪ **Usuario**: Gris

---

## 🔐 Seguridad

### Protección de Ruta
- Solo usuarios con rol `admin` pueden acceder
- Si un organizador intenta acceder, será redirigido

### Validación de Datos
- Email único (no duplicados)
- Username único
- Contraseña segura (mínimo 8 caracteres)

---

## 🛠️ Backend Requerido

### Endpoints Necesarios

El servicio espera que el backend exponga estos endpoints:

```
GET    /api/usuarios              - Listar todos los usuarios
POST   /api/usuarios              - Crear nuevo usuario
PUT    /api/usuarios/:id          - Actualizar usuario
DELETE /api/usuarios/:id          - Eliminar usuario
PATCH  /api/usuarios/:id/estado   - Habilitar/deshabilitar
POST   /api/usuarios/:id/roles    - Asignar rol
```

### Estructura de Request (Crear Usuario)

```json
{
  "username": "organizador1",
  "email": "organizador@kairo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "password": "Password123!",
  "role": "organizador"
}
```

### Estructura de Response (Usuario)

```json
{
  "id": "uuid-here",
  "username": "organizador1",
  "email": "organizador@kairo.com",
  "firstName": "Juan",
  "lastName": "Pérez",
  "enabled": true,
  "roles": ["organizador"],
  "createdTimestamp": 1704844800000
}
```

---

## 📝 Casos de Uso

### Caso 1: Crear un Organizador

**Escenario**: Necesitas dar acceso a un nuevo organizador de eventos

**Pasos**:
1. Accede a `/admin/usuarios`
2. Clic en "Crear Usuario"
3. Completa:
   - Username: `organizador_teatro`
   - Email: `teatro@kairo.com`
   - Nombre: `María`
   - Apellido: `González`
   - Contraseña: `Teatro2026!`
   - Rol: `Organizador`
4. Clic en "Crear Usuario"
5. ✅ El organizador ya puede iniciar sesión y crear eventos

### Caso 2: Deshabilitar Temporalmente un Usuario

**Escenario**: Un organizador está de vacaciones

**Pasos**:
1. Busca al usuario en la tabla
2. Haz clic en el ícono 🚫 (Ban)
3. ✅ El usuario no podrá iniciar sesión hasta que lo habilites nuevamente

### Caso 3: Eliminar un Usuario Inactivo

**Escenario**: Un organizador ya no trabaja con la empresa

**Pasos**:
1. Busca al usuario en la tabla
2. Haz clic en el ícono 🗑️ (Trash)
3. Confirma la eliminación
4. ✅ El usuario es eliminado permanentemente

---

## 🎯 Flujo Completo

```
Admin accede a /admin/usuarios
        ↓
Ve lista de usuarios existentes
        ↓
Clic en "Crear Usuario"
        ↓
Completa formulario con datos del organizador
        ↓
Clic en "Crear Usuario"
        ↓
Backend crea usuario en Keycloak
        ↓
Backend asigna rol "organizador"
        ↓
✅ Usuario creado exitosamente
        ↓
Organizador puede iniciar sesión
        ↓
Organizador accede a /admin/eventos
        ↓
Organizador puede crear y gestionar eventos
```

---

## 🐛 Troubleshooting

### Problema 1: "Error al cargar usuarios"

**Causa**: Backend no responde o no está corriendo

**Solución**:
```bash
# Verificar que el servicio de usuarios esté corriendo
docker ps | grep usuarios

# Ver logs
docker logs kairo-usuarios
```

### Problema 2: "Error al crear usuario"

**Causas posibles**:
- Email ya existe
- Username ya existe
- Contraseña muy débil

**Solución**:
- Verifica que el email y username sean únicos
- Usa una contraseña de al menos 8 caracteres

### Problema 3: No puedo acceder a /admin/usuarios

**Causa**: No tienes rol de administrador

**Solución**:
- Verifica tu rol en Keycloak
- Solo usuarios con rol `admin` pueden acceder

---

## 📊 Estadísticas

La página muestra 3 métricas principales:

1. **Total Usuarios**: Todos los usuarios del sistema
2. **Organizadores**: Usuarios con rol `organizador`
3. **Administradores**: Usuarios con rol `admin`

---

## 🎨 Diseño

La interfaz usa:
- ✨ Gradientes modernos (púrpura/rosa)
- 🌙 Tema oscuro
- 📱 Diseño responsive
- ⚡ Animaciones suaves
- 🎯 Iconos de Lucide React

---

## 🔄 Estado Actual

### ✅ Implementado
- [x] Página de gestión de usuarios
- [x] Formulario de creación
- [x] Tabla de usuarios
- [x] Estadísticas
- [x] Habilitar/deshabilitar
- [x] Eliminar usuario
- [x] Validación de formulario
- [x] Protección de ruta (solo admin)
- [x] Servicio de API
- [x] Integración en router

### ⏳ Pendiente (Backend)
- [ ] Implementar endpoints en el backend
- [ ] Integración con Keycloak
- [ ] Gestión de roles en Keycloak
- [ ] Validación de duplicados

---

## 🚀 Próximos Pasos

1. **Implementar Backend**:
   - Crear controlador de usuarios
   - Integrar con Keycloak Admin API
   - Implementar validaciones

2. **Probar Funcionalidad**:
   - Crear un organizador de prueba
   - Verificar que puede iniciar sesión
   - Verificar que puede crear eventos

3. **Mejorar**:
   - Agregar búsqueda de usuarios
   - Agregar filtros por rol
   - Agregar paginación
   - Agregar edición de usuarios

---

**Fecha**: 2026-01-09  
**Estado**: ✅ Frontend Completo - Esperando Backend
