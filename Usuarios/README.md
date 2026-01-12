# 👥 Microservicio de Usuarios

Microservicio para la gestión de usuarios del sistema con integración a Keycloak para autenticación y autorización.

## 🏗️ Arquitectura

- **Patrón:** Hexagonal (Ports & Adapters) con DDD
- **CQRS:** Separación estricta Commands/Queries con MediatR
- **Base de Datos:** PostgreSQL con Entity Framework Core
- **Autenticación:** Integración con Keycloak
- **Validación:** FluentValidation
- **Logging:** Serilog con logging estructurado
- **Testing:** Property-Based Testing + Unit Tests + Integration Tests

## 📋 Tabla de Contenidos

- [Arquitectura](#arquitectura)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Características](#características)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Endpoints Disponibles](#endpoints-disponibles)
- [Configuración](#configuración)
- [Integración con Keycloak](#integración-con-keycloak)
- [Testing](#testing)
- [Desarrollo](#desarrollo)
- [Troubleshooting](#troubleshooting)

## 📦 Estructura del Proyecto

```
Usuarios/
├── src/
│   ├── Usuarios.Dominio/           # Capa de dominio (Entidades, Value Objects, Interfaces)
│   │   ├── Entidades/
│   │   │   └── Usuario.cs
│   │   ├── ObjetosValor/
│   │   │   ├── Correo.cs
│   │   │   ├── Telefono.cs
│   │   │   └── Direccion.cs
│   │   ├── Enums/
│   │   │   └── Rol.cs
│   │   ├── Repositorios/
│   │   │   └── IRepositorioUsuarios.cs
│   │   ├── Servicios/
│   │   │   └── IServicioKeycloak.cs
│   │   └── Excepciones/
│   │       ├── UsuarioNoEncontradoException.cs
│   │       ├── CorreoDuplicadoException.cs
│   │       └── UsernameDuplicadoException.cs
│   │
│   ├── Usuarios.Aplicacion/        # Capa de aplicación (Commands, Queries, Handlers)
│   │   ├── Comandos/
│   │   │   ├── AgregarUsuarioComando.cs
│   │   │   ├── ActualizarUsuarioComando.cs
│   │   │   └── EliminarUsuarioComando.cs
│   │   ├── Consultas/
│   │   │   ├── ConsultarUsuarioQuery.cs
│   │   │   └── ConsultarUsuariosQuery.cs
│   │   ├── DTOs/
│   │   │   ├── UsuarioDto.cs
│   │   │   ├── CrearUsuarioDto.cs
│   │   │   └── ActualizarUsuarioDto.cs
│   │   └── Validadores/
│   │       ├── CrearUsuarioDtoValidator.cs
│   │       └── ActualizarUsuarioDtoValidator.cs
│   │
│   ├── Usuarios.Infraestructura/   # Capa de infraestructura (Repositorios, DbContext)
│   │   ├── Persistencia/
│   │   │   ├── UsuariosDbContext.cs
│   │   │   └── Configuraciones/
│   │   │       └── UsuarioEntityConfiguration.cs
│   │   ├── Repositorios/
│   │   │   └── RepositorioUsuarios.cs
│   │   └── Servicios/
│   │       └── ServicioKeycloak.cs
│   │
│   ├── Usuarios.API/               # Capa de presentación (Controllers, Middleware)
│   │   ├── Controllers/
│   │   │   └── UsuariosController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── Usuarios.Pruebas/           # Tests (Property + Unit + Integration)
│       ├── Dominio/
│       ├── Aplicacion/
│       ├── Infraestructura/
│       ├── API/
│       └── Propiedades/
│
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## 🚀 Características

### **Modelo de Dominio Rico:**
- ✅ Entidad Usuario como Aggregate Root con lógica de negocio
- ✅ Value Objects inmutables (Correo, Telefono, Direccion)
- ✅ Validaciones de dominio en constructores y métodos
- ✅ Enum Rol (User, Admin, Organizator)

### **Commands (Escritura):**
- ✅ Crear usuario (con sincronización a Keycloak)
- ✅ Actualizar usuario
- ✅ Eliminar usuario (eliminación lógica)

### **Queries (Lectura):**
- ✅ Obtener usuario por ID
- ✅ Obtener todos los usuarios activos

### **Validaciones:**
- ✅ Unicidad de username y correo
- ✅ Formato de email válido
- ✅ Teléfono con 7-15 dígitos
- ✅ Dirección mínima de 5 caracteres
- ✅ Password mínimo de 8 caracteres

### **Integración con Keycloak:**
- ✅ Creación de usuarios en Keycloak
- ✅ Actualización de usuarios
- ✅ Desactivación de usuarios
- ✅ Asignación de roles

## 🔧 Requisitos Previos

### Software Requerido

- **.NET 8 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker** - [Descargar](https://www.docker.com/products/docker-desktop)
- **Docker Compose** - Incluido con Docker Desktop

### Verificar Instalación

```bash
# Verificar .NET
dotnet --version
# Debe mostrar: 8.0.x o superior

# Verificar Docker
docker --version
docker-compose --version
```

## 🏃 Instalación y Ejecución

### Opción 1: Ejecución Completa con Docker (Recomendado)

Esta opción levanta todos los servicios (PostgreSQL, Keycloak y la API) en contenedores:

```bash
# 1. Navegar al directorio del proyecto
cd Usuarios

# 2. Construir y levantar todos los servicios
docker-compose up --build

# 3. Verificar que los servicios estén corriendo
docker-compose ps
```

Los servicios estarán disponibles en:
- **API de Usuarios:** http://localhost:8083
- **Swagger UI:** http://localhost:8083/swagger
- **Health Check:** http://localhost:8083/health
- **PostgreSQL:** localhost:5432 (Base de datos: kairo_usuarios)
- **Keycloak:** http://localhost:8080 (admin/admin)

### Opción 2: Desarrollo Local (API en local, infraestructura en Docker)

Esta opción es útil para desarrollo activo:

```bash
# 1. Levantar solo la infraestructura (PostgreSQL y Keycloak)
# Desde el directorio Infraestructura/
cd ../Infraestructura
docker-compose up postgres keycloak -d

# 2. Volver al directorio de Usuarios
cd ../Usuarios

# 3. Restaurar dependencias
dotnet restore

# 4. Ejecutar la API localmente
cd src/Usuarios.API
dotnet run

# La API estará disponible en http://localhost:5000
```

### Opción 3: Ejecución Manual (Sin Docker)

Si prefieres instalar PostgreSQL y Keycloak localmente:

```bash
# 1. Instalar PostgreSQL localmente
# Windows: https://www.postgresql.org/download/windows/
# macOS: brew install postgresql
# Linux: sudo apt-get install postgresql

# 2. Crear base de datos
createdb kairo_usuarios

# 3. Configurar variables de entorno
export ConnectionStrings__PostgresConnection="Host=localhost;Database=kairo_usuarios;Username=postgres;Password=postgres"
export Keycloak__Authority="http://localhost:8080/realms/Kairo"
export Keycloak__AdminUrl="http://localhost:8080/admin/realms/Kairo"

# 4. Ejecutar la API
cd src/Usuarios.API
dotnet run
```

### Detener Servicios

```bash
# Detener servicios Docker
docker-compose down

# Detener y eliminar volúmenes (limpia datos)
docker-compose down -v
```

## 📡 Endpoints Disponibles

### Usuarios

#### Crear Usuario
```http
POST /api/usuarios
Content-Type: application/json

{
  "username": "juan.perez",
  "nombre": "Juan Pérez",
  "correo": "juan.perez@example.com",
  "telefono": "+1234567890",
  "direccion": "Calle Principal 123",
  "rol": 1,
  "password": "Password123!"
}

Response: 201 Created
Location: /api/usuarios/{id}
Body: "3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

#### Obtener Usuario por ID
```http
GET /api/usuarios/{id}

Response: 200 OK
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "juan.perez",
  "nombre": "Juan Pérez",
  "correo": "juan.perez@example.com",
  "telefono": "1234567890",
  "direccion": "Calle Principal 123",
  "rol": 1,
  "fechaCreacion": "2024-12-30T10:00:00Z"
}
```

#### Obtener Todos los Usuarios
```http
GET /api/usuarios

Response: 200 OK
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "juan.perez",
    "nombre": "Juan Pérez",
    ...
  }
]
```

#### Actualizar Usuario
```http
PUT /api/usuarios/{id}
Content-Type: application/json

{
  "nombre": "Juan Carlos Pérez",
  "telefono": "+1234567891",
  "direccion": "Calle Secundaria 456"
}

Response: 204 No Content
```

#### Eliminar Usuario (Eliminación Lógica)
```http
DELETE /api/usuarios/{id}

Response: 204 No Content
```

### Health Checks

```http
GET /health

Response: 200 OK
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "keycloak": "Healthy"
  }
}
```

### Swagger

Acceder a la documentación interactiva:
```
http://localhost:8083/swagger
```

## ⚙️ Configuración

### Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `ConnectionStrings__PostgresConnection` | Cadena de conexión a PostgreSQL | `Host=postgres;Database=kairo_usuarios;Username=postgres;Password=postgres` |
| `Keycloak__Authority` | URL del realm de Keycloak | `http://keycloak:8080/realms/Kairo` |
| `Keycloak__AdminUrl` | URL de administración de Keycloak | `http://keycloak:8080/admin/realms/Kairo` |
| `Keycloak__ClientId` | Client ID de Keycloak | `usuarios-service` |
| `Keycloak__ClientSecret` | Client Secret de Keycloak | `secret` |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución | `Development` |

### appsettings.json

```json
{
  "ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Database=kairo_usuarios;Username=postgres;Password=postgres"
  },
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/Kairo",
    "AdminUrl": "http://localhost:8080/admin/realms/Kairo",
    "ClientId": "usuarios-service",
    "ClientSecret": "secret"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### appsettings.Development.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

## 🔐 Integración con Keycloak

### Flujo de Creación de Usuario

1. **Validación:** Se validan los datos del usuario con FluentValidation
2. **Verificación de Unicidad:** Se verifica que username y correo no existan
3. **Creación en Keycloak:** Se crea el usuario en Keycloak primero
4. **Asignación de Rol:** Se asigna el rol correspondiente en Keycloak
5. **Persistencia en BD:** Se guarda el usuario en PostgreSQL
6. **Rollback:** Si falla algún paso, se revierten los cambios

### Roles Disponibles

| Rol | Valor | Descripción |
|-----|-------|-------------|
| User | 1 | Usuario regular del sistema |
| Admin | 2 | Administrador con permisos completos |
| Organizator | 3 | Organizador de eventos |

### Configuración de Keycloak

El microservicio requiere que Keycloak esté configurado con:
- Realm: `Kairo`
- Client: `usuarios-service` (confidential)
- Roles: `user`, `admin`, `organizator`

Ver `Infraestructura/configs/keycloak/realm-export.json` para la configuración completa.

## 🧪 Testing

El proyecto incluye una estrategia de testing exhaustiva con >90% de cobertura:

### Tipos de Tests

- **7 Property-Based Tests** (100+ iteraciones cada uno)
- **~40 Unit Tests** para casos específicos
- **~15 Integration Tests** end-to-end

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Solo property tests
dotnet test --filter "Category=Property"

# Solo unit tests
dotnet test --filter "Category=Unit"

# Solo integration tests
dotnet test --filter "Category=Integration"

# Con cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generar reporte HTML de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

### Property Tests Implementados

1. **Unicidad de Username:** Verifica que no se puedan crear dos usuarios con el mismo username
2. **Unicidad de Correo:** Verifica que no se puedan crear dos usuarios con el mismo correo
3. **Validación de Correo:** Verifica que solo correos válidos se acepten
4. **Validación de Teléfono:** Verifica que solo teléfonos de 7-15 dígitos se acepten
5. **Inmutabilidad de Value Objects:** Verifica que los Value Objects sean inmutables
6. **Eliminación Lógica:** Verifica que usuarios eliminados no aparezcan en consultas
7. **Logging de Operaciones:** Verifica que todas las operaciones se registren

### Ejemplo de Property Test

```csharp
[Property(MaxTest = 100)]
public Property Propiedad_UnicidadUsername()
{
    // Feature: refactorizacion-usuarios, Property 1
    return Prop.ForAll<string, string>((username1, username2) =>
    {
        // Si los usernames son diferentes, ambos usuarios deben poder crearse
        // Si son iguales, el segundo debe fallar
        var resultado = username1.ToLower() != username2.ToLower() 
            ? PuedenCrearseDosUsuarios(username1, username2)
            : !PuedeCrearseSegundoUsuario(username1, username2);
        
        return resultado.ToProperty();
    });
}
```

## 💻 Desarrollo

### Flujo de Trabajo de Desarrollo

1. **Crear una rama de feature**
   ```bash
   git checkout -b feature/nueva-funcionalidad
   ```

2. **Hacer cambios siguiendo la arquitectura hexagonal**
   - Dominio primero (entidades, value objects, interfaces)
   - Infraestructura (implementaciones de repositorios)
   - Aplicación (commands, queries, handlers)
   - API (controllers, middleware)

3. **Escribir tests**
   - Property tests para propiedades universales
   - Unit tests para casos específicos
   - Integration tests para flujos completos

4. **Ejecutar tests localmente**
   ```bash
   dotnet test
   ```

5. **Verificar cobertura**
   ```bash
   dotnet test /p:CollectCoverage=true
   ```

6. **Commit y push**
   ```bash
   git add .
   git commit -m "feat: descripción del cambio"
   git push origin feature/nueva-funcionalidad
   ```

### Hot Reload durante Desarrollo

```bash
cd src/Usuarios.API
dotnet watch run
```

Esto reiniciará automáticamente la aplicación cuando detecte cambios en el código.

### Debugging

#### Visual Studio Code
1. Abrir el proyecto en VS Code
2. Presionar F5 o ir a Run > Start Debugging
3. Seleccionar ".NET Core Launch (web)"

#### Visual Studio
1. Abrir `Usuarios.API.sln`
2. Establecer `Usuarios.API` como proyecto de inicio
3. Presionar F5

### Agregar Nuevos Endpoints

1. **Definir DTO en `Usuarios.Aplicacion/DTOs/`**
2. **Crear Command/Query en `Usuarios.Aplicacion/Comandos/` o `Consultas/`**
3. **Crear Handler correspondiente**
4. **Agregar validador en `Usuarios.Aplicacion/Validadores/`**
5. **Agregar endpoint en `Usuarios.API/Controllers/UsuariosController.cs`**
6. **Escribir tests**

### Principios de Diseño

#### CQRS Estricto
- Commands retornan solo `Guid` o `Unit`
- Queries retornan DTOs inmutables
- Separación completa entre escritura y lectura

#### Controladores "Thin"
- Sin lógica de negocio
- Solo orquestación con MediatR
- Sin construcción manual de ViewModels

#### Arquitectura Hexagonal
- Dominio independiente de infraestructura
- Inversión de dependencias
- Puertos y adaptadores

#### Value Objects Inmutables
- Records de C# para inmutabilidad
- Factory methods para creación con validación
- Equality por valor

## 🔍 Monitoreo y Logs

### Logging Estructurado con Serilog

El servicio utiliza Serilog para logging estructurado:

- **Consola:** Logs en formato legible para desarrollo
- **Archivo:** Logs persistidos en `logs/` para análisis
- **Contexto de Correlación:** Cada request tiene un ID único

### Niveles de Log

- **Debug:** Información de debugging (solo en desarrollo)
- **Information:** Eventos normales del sistema
- **Warning:** Situaciones anómalas pero manejables
- **Error:** Errores que requieren atención
- **Fatal:** Errores críticos que detienen el servicio

### Consultar Logs

```bash
# Ver logs en tiempo real
docker-compose logs -f usuarios-api

# Ver logs de PostgreSQL
docker-compose logs postgres

# Ver logs de Keycloak
docker-compose logs keycloak
```

## 🚨 Troubleshooting

### Problema: PostgreSQL no se conecta

**Síntomas:**
```
Error connecting to database: Connection refused
```

**Soluciones:**
1. Verificar que PostgreSQL esté corriendo:
   ```bash
   docker-compose ps postgres
   ```

2. Verificar la cadena de conexión:
   ```bash
   echo $ConnectionStrings__PostgresConnection
   ```

3. Reiniciar PostgreSQL:
   ```bash
   docker-compose restart postgres
   ```

### Problema: Keycloak no responde

**Síntomas:**
```
HttpRequestException: Connection refused to Keycloak
```

**Soluciones:**
1. Verificar que Keycloak esté corriendo:
   ```bash
   docker-compose ps keycloak
   ```

2. Verificar que Keycloak haya iniciado completamente:
   ```bash
   docker-compose logs keycloak | grep "started"
   ```

3. Esperar a que Keycloak termine de iniciar (puede tomar 1-2 minutos)

4. Verificar configuración en appsettings.json

### Problema: Tests de integración fallan

**Síntomas:**
```
Test failed: Connection refused
```

**Soluciones:**
1. Asegurarse de que PostgreSQL esté corriendo:
   ```bash
   docker-compose up postgres -d
   ```

2. Limpiar y reconstruir:
   ```bash
   dotnet clean
   dotnet build
   dotnet test
   ```

3. Verificar que no haya procesos usando los puertos:
   ```bash
   # Windows
   netstat -ano | findstr :5432
   
   # Linux/Mac
   lsof -i :5432
   ```

### Problema: Migraciones no se aplican

**Síntomas:**
```
Table 'Usuarios' doesn't exist
```

**Soluciones:**
1. Aplicar migraciones manualmente:
   ```bash
   cd src/Usuarios.API
   dotnet ef database update
   ```

2. Verificar que las migraciones existan:
   ```bash
   dotnet ef migrations list
   ```

3. Crear migración si no existe:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

### Problema: Usuario duplicado en Keycloak

**Síntomas:**
```
User already exists in Keycloak
```

**Soluciones:**
1. Eliminar usuario de Keycloak manualmente:
   - Ir a http://localhost:8080
   - Login: admin/admin
   - Ir a Users y eliminar el usuario

2. Limpiar base de datos:
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

## 📚 Recursos Adicionales

- **Especificación Completa:** `.kiro/specs/refactorizacion-usuarios/`
- **Documento de Requisitos:** `.kiro/specs/refactorizacion-usuarios/requirements.md`
- **Documento de Diseño:** `.kiro/specs/refactorizacion-usuarios/design.md`
- **Plan de Tareas:** `.kiro/specs/refactorizacion-usuarios/tasks.md`

## 🤝 Contribución

### Proceso de Contribución

1. **Fork del repositorio**
2. **Crear rama de feature:** `git checkout -b feature/nueva-funcionalidad`
3. **Hacer cambios siguiendo las convenciones**
4. **Escribir tests (cobertura >90%)**
5. **Ejecutar tests:** `dotnet test`
6. **Commit:** `git commit -m "feat: descripción"`
7. **Push:** `git push origin feature/nueva-funcionalidad`
8. **Crear Pull Request**

### Convenciones de Código

- Seguir las guías de estilo de C# (.NET)
- Usar nombres descriptivos en español para el dominio
- Documentar métodos públicos con XML comments
- Mantener métodos pequeños y enfocados
- Escribir tests para toda nueva funcionalidad

### Convenciones de Commits

Seguir [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` Nueva funcionalidad
- `fix:` Corrección de bug
- `docs:` Cambios en documentación
- `test:` Agregar o modificar tests
- `refactor:` Refactorización de código
- `perf:` Mejoras de rendimiento
- `chore:` Tareas de mantenimiento

## 📄 Licencia

Este proyecto es parte del Sistema de Gestión de Eventos Kairo.

---

**Última actualización:** 30 de Diciembre de 2024  
**Versión:** 2.0.0 (Refactorización Arquitectura Hexagonal + CQRS)
