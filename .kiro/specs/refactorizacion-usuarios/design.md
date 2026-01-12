# Design Document - Refactorización Microservicio Usuarios

## Overview

Este diseño transforma el microservicio Usuarios desde su arquitectura actual (que mezcla capas y no sigue patrones claros) hacia una Arquitectura Hexagonal profesional con CQRS, siguiendo exactamente los mismos estándares de calidad que los microservicios Eventos, Asientos y Reportes.

**Problemas del Código Actual:**
- Mezcla de responsabilidades entre capas (Core, Application, Infrastructure)
- No hay separación clara entre Commands y Queries
- Entidades anémicas sin lógica de negocio
- Repositorios genéricos que no expresan el dominio
- Validaciones dispersas sin un patrón claro
- Tests insuficientes y sin cobertura adecuada
- Integración con Keycloak acoplada a la infraestructura

**Solución Propuesta:**
- Arquitectura Hexagonal con 4 capas bien definidas
- CQRS con MediatR para separar lecturas y escrituras
- Modelo de dominio rico con Value Objects
- Repository Pattern específico del dominio
- Validación centralizada con FluentValidation
- Testing comprehensivo (>90% cobertura)
- Integración con Keycloak mediante servicios de dominio

## Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Usuarios.API                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │         Controllers (REST Endpoints)             │  │
│  │  - UsuariosController                            │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │         Middleware & Configuration               │  │
│  │  - ExceptionHandlingMiddleware                   │  │
│  │  - Program.cs (DI Configuration)                 │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP/REST
                     ▼
┌─────────────────────────────────────────────────────────┐
│                Usuarios.Aplicacion                      │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Commands (Escritura)                │  │
│  │  - AgregarUsuarioComando                         │  │
│  │  - ActualizarUsuarioComando                      │  │
│  │  - EliminarUsuarioComando                        │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Queries (Lectura)                   │  │
│  │  - ConsultarUsuarioQuery                         │  │
│  │  - ConsultarUsuariosQuery                        │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Handlers                            │  │
│  │  - AgregarUsuarioComandoHandler                  │  │
│  │  - ConsultarUsuarioQueryHandler                  │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              DTOs & Validators                   │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │ Interfaces
                     ▼
┌─────────────────────────────────────────────────────────┐
│                 Usuarios.Dominio                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Entidades                           │  │
│  │  - Usuario (Aggregate Root)                      │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Value Objects                       │  │
│  │  - Correo, Telefono, Direccion                   │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Enums                               │  │
│  │  - Rol (User, Admin, Organizator)                │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Interfaces de Repositorio           │  │
│  │  - IRepositorioUsuarios                          │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Servicios de Dominio                │  │
│  │  - IServicioKeycloak                             │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Excepciones de Dominio              │  │
│  │  - UsuarioNoEncontradoException                  │  │
│  │  - CorreoDuplicadoException                      │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │ Implementación
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Usuarios.Infraestructura                   │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Repositorios                        │  │
│  │  - RepositorioUsuarios (EF Core)                 │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Persistencia                        │  │
│  │  - UsuariosDbContext                             │  │
│  │  - UsuarioEntityConfiguration                    │  │
│  └──────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────┐  │
│  │              Servicios Externos                  │  │
│  │  - ServicioKeycloak                              │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Component Diagram

```
Usuarios/
├── src/
│   ├── Usuarios.Dominio/
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
│   ├── Usuarios.Aplicacion/
│   │   ├── Comandos/
│   │   │   ├── AgregarUsuarioComando.cs
│   │   │   ├── AgregarUsuarioComandoHandler.cs
│   │   │   ├── ActualizarUsuarioComando.cs
│   │   │   ├── ActualizarUsuarioComandoHandler.cs
│   │   │   ├── EliminarUsuarioComando.cs
│   │   │   └── EliminarUsuarioComandoHandler.cs
│   │   ├── Consultas/
│   │   │   ├── ConsultarUsuarioQuery.cs
│   │   │   ├── ConsultarUsuarioQueryHandler.cs
│   │   │   ├── ConsultarUsuariosQuery.cs
│   │   │   └── ConsultarUsuariosQueryHandler.cs
│   │   ├── DTOs/
│   │   │   ├── UsuarioDto.cs
│   │   │   ├── CrearUsuarioDto.cs
│   │   │   └── ActualizarUsuarioDto.cs
│   │   ├── Validadores/
│   │   │   ├── CrearUsuarioDtoValidator.cs
│   │   │   └── ActualizarUsuarioDtoValidator.cs
│   │   └── InyeccionDependencias.cs
│   │
│   ├── Usuarios.Infraestructura/
│   │   ├── Persistencia/
│   │   │   ├── UsuariosDbContext.cs
│   │   │   └── Configuraciones/
│   │   │       └── UsuarioEntityConfiguration.cs
│   │   ├── Repositorios/
│   │   │   └── RepositorioUsuarios.cs
│   │   ├── Servicios/
│   │   │   └── ServicioKeycloak.cs
│   │   └── InyeccionDependencias.cs
│   │
│   ├── Usuarios.API/
│   │   ├── Controllers/
│   │   │   └── UsuariosController.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   │
│   └── Usuarios.Pruebas/
│       ├── Dominio/
│       │   ├── UsuarioTests.cs
│       │   ├── CorreoTests.cs
│       │   └── TelefonoTests.cs
│       ├── Aplicacion/
│       │   ├── AgregarUsuarioComandoHandlerTests.cs
│       │   └── ConsultarUsuarioQueryHandlerTests.cs
│       ├── Infraestructura/
│       │   └── RepositorioUsuariosTests.cs
│       └── API/
│           └── UsuariosControllerTests.cs
│
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Components and Interfaces

### 1. Capa de Dominio

#### Usuario (Aggregate Root)

```csharp
namespace Usuarios.Dominio.Entidades
{
    public class Usuario
    {
        // Propiedades privadas
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Nombre { get; private set; }
        public Correo Correo { get; private set; }
        public Telefono Telefono { get; private set; }
        public Direccion Direccion { get; private set; }
        public Rol Rol { get; private set; }
        public bool EstaActivo { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }
        
        // Constructor privado para EF Core
        private Usuario() { }
        
        // Factory method para crear usuario
        public static Usuario Crear(
            string username,
            string nombre,
            Correo correo,
            Telefono telefono,
            Direccion direccion,
            Rol rol)
        {
            // Validaciones de negocio
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El username no puede estar vacío");
            
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            
            return new Usuario
            {
                Id = Guid.NewGuid(),
                Username = username,
                Nombre = nombre,
                Correo = correo,
                Telefono = telefono,
                Direccion = direccion,
                Rol = rol,
                EstaActivo = true,
                FechaCreacion = DateTime.UtcNow
            };
        }
        
        // Métodos de negocio
        public void Actualizar(
            string nombre,
            Telefono telefono,
            Direccion direccion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío");
            
            Nombre = nombre;
            Telefono = telefono;
            Direccion = direccion;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void CambiarRol(Rol nuevoRol)
        {
            Rol = nuevoRol;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void Desactivar()
        {
            EstaActivo = false;
            FechaActualizacion = DateTime.UtcNow;
        }
        
        public void Reactivar()
        {
            EstaActivo = true;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
```

#### Value Objects

```csharp
namespace Usuarios.Dominio.ObjetosValor
{
    public record Correo
    {
        public string Valor { get; }
        
        private Correo(string valor)
        {
            Valor = valor;
        }
        
        public static Correo Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El correo no puede estar vacío");
            
            if (!EsCorreoValido(valor))
                throw new ArgumentException($"El correo '{valor}' no es válido");
            
            return new Correo(valor.ToLowerInvariant());
        }
        
        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }
    }
    
    public record Telefono
    {
        public string Valor { get; }
        
        private Telefono(string valor)
        {
            Valor = valor;
        }
        
        public static Telefono Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El teléfono no puede estar vacío");
            
            // Remover espacios y caracteres especiales
            var telefonoLimpio = new string(valor.Where(char.IsDigit).ToArray());
            
            if (telefonoLimpio.Length < 7 || telefonoLimpio.Length > 15)
                throw new ArgumentException("El teléfono debe tener entre 7 y 15 dígitos");
            
            return new Telefono(telefonoLimpio);
        }
    }
    
    public record Direccion
    {
        public string Valor { get; }
        
        private Direccion(string valor)
        {
            Valor = valor;
        }
        
        public static Direccion Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("La dirección no puede estar vacía");
            
            if (valor.Length < 5)
                throw new ArgumentException("La dirección debe tener al menos 5 caracteres");
            
            return new Direccion(valor.Trim());
        }
    }
}
```

#### Enum Rol

```csharp
namespace Usuarios.Dominio.Enums
{
    public enum Rol
    {
        User = 1,
        Admin = 2,
        Organizator = 3
    }
}
```

#### Interfaz de Repositorio

```csharp
namespace Usuarios.Dominio.Repositorios
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Usuario?> ObtenerPorUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Usuario?> ObtenerPorCorreoAsync(Correo correo, CancellationToken cancellationToken = default);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Usuario>> ObtenerActivosAsync(CancellationToken cancellationToken = default);
        Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task<bool> ExisteUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExisteCorreoAsync(Correo correo, CancellationToken cancellationToken = default);
    }
}
```

#### Servicio de Dominio para Keycloak

```csharp
namespace Usuarios.Dominio.Servicios
{
    public interface IServicioKeycloak
    {
        Task<string> CrearUsuarioAsync(Usuario usuario, string password, CancellationToken cancellationToken = default);
        Task ActualizarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
        Task DesactivarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
        Task AsignarRolAsync(Guid usuarioId, Rol rol, CancellationToken cancellationToken = default);
    }
}
```

#### Excepciones de Dominio

```csharp
namespace Usuarios.Dominio.Excepciones
{
    public class UsuarioNoEncontradoException : Exception
    {
        public Guid UsuarioId { get; }
        
        public UsuarioNoEncontradoException(Guid usuarioId)
            : base($"No se encontró el usuario con ID: {usuarioId}")
        {
            UsuarioId = usuarioId;
        }
    }
    
    public class CorreoDuplicadoException : Exception
    {
        public string Correo { get; }
        
        public CorreoDuplicadoException(string correo)
            : base($"Ya existe un usuario con el correo: {correo}")
        {
            Correo = correo;
        }
    }
    
    public class UsernameDuplicadoException : Exception
    {
        public string Username { get; }
        
        public UsernameDuplicadoException(string username)
            : base($"Ya existe un usuario con el username: {username}")
        {
            Username = username;
        }
    }
}
```

### 2. Capa de Aplicación

#### Commands

```csharp
namespace Usuarios.Aplicacion.Comandos
{
    public record AgregarUsuarioComando : IRequest<Guid>
    {
        public string Username { get; init; }
        public string Nombre { get; init; }
        public string Correo { get; init; }
        public string Telefono { get; init; }
        public string Direccion { get; init; }
        public Rol Rol { get; init; }
        public string Password { get; init; }
    }
    
    public class AgregarUsuarioComandoHandler : IRequestHandler<AgregarUsuarioComando, Guid>
    {
        private readonly IRepositorioUsuarios _repositorio;
        private readonly IServicioKeycloak _servicioKeycloak;
        private readonly ILogger<AgregarUsuarioComandoHandler> _logger;
        
        public AgregarUsuarioComandoHandler(
            IRepositorioUsuarios repositorio,
            IServicioKeycloak servicioKeycloak,
            ILogger<AgregarUsuarioComandoHandler> logger)
        {
            _repositorio = repositorio;
            _servicioKeycloak = servicioKeycloak;
            _logger = logger;
        }
        
        public async Task<Guid> Handle(
            AgregarUsuarioComando comando,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Agregando usuario: {Username}", comando.Username);
            
            // Validar unicidad de username
            if (await _repositorio.ExisteUsernameAsync(comando.Username, cancellationToken))
            {
                throw new UsernameDuplicadoException(comando.Username);
            }
            
            // Crear value objects
            var correo = Correo.Crear(comando.Correo);
            
            // Validar unicidad de correo
            if (await _repositorio.ExisteCorreoAsync(correo, cancellationToken))
            {
                throw new CorreoDuplicadoException(comando.Correo);
            }
            
            var telefono = Telefono.Crear(comando.Telefono);
            var direccion = Direccion.Crear(comando.Direccion);
            
            // Crear usuario
            var usuario = Usuario.Crear(
                comando.Username,
                comando.Nombre,
                correo,
                telefono,
                direccion,
                comando.Rol);
            
            try
            {
                // Crear en Keycloak primero
                await _servicioKeycloak.CrearUsuarioAsync(usuario, comando.Password, cancellationToken);
                
                // Luego persistir en BD
                await _repositorio.AgregarAsync(usuario, cancellationToken);
                
                _logger.LogInformation("Usuario agregado exitosamente: {UsuarioId}", usuario.Id);
                
                return usuario.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar usuario: {Username}", comando.Username);
                throw;
            }
        }
    }
}
```



#### Queries

```csharp
namespace Usuarios.Aplicacion.Consultas
{
    public record ConsultarUsuarioQuery : IRequest<UsuarioDto?>
    {
        public Guid UsuarioId { get; init; }
    }
    
    public class ConsultarUsuarioQueryHandler : IRequestHandler<ConsultarUsuarioQuery, UsuarioDto?>
    {
        private readonly IRepositorioUsuarios _repositorio;
        private readonly ILogger<ConsultarUsuarioQueryHandler> _logger;
        
        public ConsultarUsuarioQueryHandler(
            IRepositorioUsuarios repositorio,
            ILogger<ConsultarUsuarioQueryHandler> logger)
        {
            _repositorio = repositorio;
            _logger = logger;
        }
        
        public async Task<UsuarioDto?> Handle(
            ConsultarUsuarioQuery query,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consultando usuario: {UsuarioId}", query.UsuarioId);
            
            var usuario = await _repositorio.ObtenerPorIdAsync(query.UsuarioId, cancellationToken);
            
            if (usuario == null || !usuario.EstaActivo)
            {
                return null;
            }
            
            return new UsuarioDto
            {
                Id = usuario.Id,
                Username = usuario.Username,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo.Valor,
                Telefono = usuario.Telefono.Valor,
                Direccion = usuario.Direccion.Valor,
                Rol = usuario.Rol,
                FechaCreacion = usuario.FechaCreacion
            };
        }
    }
}
```

#### DTOs

```csharp
namespace Usuarios.Aplicacion.DTOs
{
    public record UsuarioDto
    {
        public Guid Id { get; init; }
        public string Username { get; init; }
        public string Nombre { get; init; }
        public string Correo { get; init; }
        public string Telefono { get; init; }
        public string Direccion { get; init; }
        public Rol Rol { get; init; }
        public DateTime FechaCreacion { get; init; }
    }
    
    public record CrearUsuarioDto
    {
        public string Username { get; init; }
        public string Nombre { get; init; }
        public string Correo { get; init; }
        public string Telefono { get; init; }
        public string Direccion { get; init; }
        public Rol Rol { get; init; }
        public string Password { get; init; }
    }
}
```

#### Validators

```csharp
namespace Usuarios.Aplicacion.Validadores
{
    public class CrearUsuarioDtoValidator : AbstractValidator<CrearUsuarioDto>
    {
        public CrearUsuarioDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El username es requerido")
                .MinimumLength(3).WithMessage("El username debe tener al menos 3 caracteres")
                .MaximumLength(50).WithMessage("El username no puede exceder 50 caracteres");
            
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");
            
            RuleFor(x => x.Correo)
                .NotEmpty().WithMessage("El correo es requerido")
                .EmailAddress().WithMessage("El correo no es válido");
            
            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es requerido");
            
            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es requerida")
                .MinimumLength(5).WithMessage("La dirección debe tener al menos 5 caracteres");
            
            RuleFor(x => x.Rol)
                .IsInEnum().WithMessage("El rol no es válido");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres");
        }
    }
}
```

### 3. Capa de Infraestructura

#### DbContext

```csharp
namespace Usuarios.Infraestructura.Persistencia
{
    public class UsuariosDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        
        public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new UsuarioEntityConfiguration());
        }
    }
    
    public class UsuarioEntityConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(u => u.Username)
                .IsUnique();
            
            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            // Value Object: Correo
            builder.OwnsOne(u => u.Correo, correo =>
            {
                correo.Property(c => c.Valor)
                    .HasColumnName("Correo")
                    .IsRequired()
                    .HasMaxLength(100);
                
                correo.HasIndex(c => c.Valor)
                    .IsUnique();
            });
            
            // Value Object: Telefono
            builder.OwnsOne(u => u.Telefono, telefono =>
            {
                telefono.Property(t => t.Valor)
                    .HasColumnName("Telefono")
                    .IsRequired()
                    .HasMaxLength(15);
            });
            
            // Value Object: Direccion
            builder.OwnsOne(u => u.Direccion, direccion =>
            {
                direccion.Property(d => d.Valor)
                    .HasColumnName("Direccion")
                    .IsRequired()
                    .HasMaxLength(200);
            });
            
            builder.Property(u => u.Rol)
                .IsRequired()
                .HasConversion<int>();
            
            builder.Property(u => u.EstaActivo)
                .IsRequired();
            
            builder.Property(u => u.FechaCreacion)
                .IsRequired();
            
            builder.Property(u => u.FechaActualizacion);
        }
    }
}
```

#### Repositorio

```csharp
namespace Usuarios.Infraestructura.Repositorios
{
    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly UsuariosDbContext _context;
        private readonly ILogger<RepositorioUsuarios> _logger;
        
        public RepositorioUsuarios(
            UsuariosDbContext context,
            ILogger<RepositorioUsuarios> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<Usuario?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
        
        public async Task<Usuario?> ObtenerPorUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }
        
        public async Task<Usuario?> ObtenerPorCorreoAsync(
            Correo correo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo, cancellationToken);
        }
        
        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios.ToListAsync(cancellationToken);
        }
        
        public async Task<IEnumerable<Usuario>> ObtenerActivosAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .Where(u => u.EstaActivo)
                .ToListAsync(cancellationToken);
        }
        
        public async Task AgregarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            await _context.Usuarios.AddAsync(usuario, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Usuario agregado: {UsuarioId}", usuario.Id);
        }
        
        public async Task ActualizarAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Usuario actualizado: {UsuarioId}", usuario.Id);
        }
        
        public async Task<bool> ExisteUsernameAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Username == username, cancellationToken);
        }
        
        public async Task<bool> ExisteCorreoAsync(
            Correo correo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Correo == correo, cancellationToken);
        }
    }
}
```

#### Servicio Keycloak

```csharp
namespace Usuarios.Infraestructura.Servicios
{
    public class ServicioKeycloak : IServicioKeycloak
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServicioKeycloak> _logger;
        
        public ServicioKeycloak(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServicioKeycloak> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        
        public async Task<string> CrearUsuarioAsync(
            Usuario usuario,
            string password,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creando usuario en Keycloak: {Username}", usuario.Username);
            
            // Implementación de integración con Keycloak
            // Similar a la implementación actual pero con mejor manejo de errores
            
            return usuario.Id.ToString();
        }
        
        public async Task ActualizarUsuarioAsync(
            Usuario usuario,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Actualizando usuario en Keycloak: {UsuarioId}", usuario.Id);
            
            // Implementación de actualización en Keycloak
        }
        
        public async Task DesactivarUsuarioAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Desactivando usuario en Keycloak: {UsuarioId}", usuarioId);
            
            // Implementación de desactivación en Keycloak
        }
        
        public async Task AsignarRolAsync(
            Guid usuarioId,
            Rol rol,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Asignando rol {Rol} a usuario {UsuarioId}", rol, usuarioId);
            
            // Implementación de asignación de rol en Keycloak
        }
    }
}
```

### 4. Capa de API

#### Controller

```csharp
namespace Usuarios.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsuariosController> _logger;
        
        public UsuariosController(
            IMediator mediator,
            ILogger<UsuariosController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear(
            [FromBody] CrearUsuarioDto dto,
            CancellationToken cancellationToken)
        {
            var comando = new AgregarUsuarioComando
            {
                Username = dto.Username,
                Nombre = dto.Nombre,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Rol = dto.Rol,
                Password = dto.Password
            };
            
            var usuarioId = await _mediator.Send(comando, cancellationToken);
            
            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = usuarioId },
                usuarioId);
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPorId(
            Guid id,
            CancellationToken cancellationToken)
        {
            var query = new ConsultarUsuarioQuery { UsuarioId = id };
            var usuario = await _mediator.Send(query, cancellationToken);
            
            if (usuario == null)
            {
                return NotFound();
            }
            
            return Ok(usuario);
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UsuarioDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTodos(CancellationToken cancellationToken)
        {
            var query = new ConsultarUsuariosQuery();
            var usuarios = await _mediator.Send(query, cancellationToken);
            
            return Ok(usuarios);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Actualizar(
            Guid id,
            [FromBody] ActualizarUsuarioDto dto,
            CancellationToken cancellationToken)
        {
            var comando = new ActualizarUsuarioComando
            {
                UsuarioId = id,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion
            };
            
            await _mediator.Send(comando, cancellationToken);
            
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Eliminar(
            Guid id,
            CancellationToken cancellationToken)
        {
            var comando = new EliminarUsuarioComando { UsuarioId = id };
            await _mediator.Send(comando, cancellationToken);
            
            return NoContent();
        }
    }
}
```

## Data Models

### Database Schema

```sql
CREATE TABLE Usuarios (
    Id UUID PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Nombre VARCHAR(100) NOT NULL,
    Correo VARCHAR(100) NOT NULL UNIQUE,
    Telefono VARCHAR(15) NOT NULL,
    Direccion VARCHAR(200) NOT NULL,
    Rol INTEGER NOT NULL,
    EstaActivo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaCreacion TIMESTAMP NOT NULL,
    FechaActualizacion TIMESTAMP NULL,
    
    CONSTRAINT CK_Rol CHECK (Rol IN (1, 2, 3))
);

CREATE INDEX IX_Usuarios_Username ON Usuarios(Username);
CREATE INDEX IX_Usuarios_Correo ON Usuarios(Correo);
CREATE INDEX IX_Usuarios_EstaActivo ON Usuarios(EstaActivo);
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Unicidad de Username
*For any* dos usuarios en el sistema, sus usernames deben ser diferentes (case-insensitive).
**Validates: Requirements 5.2**

### Property 2: Unicidad de Correo
*For any* dos usuarios en el sistema, sus correos electrónicos deben ser diferentes (case-insensitive).
**Validates: Requirements 5.1**

### Property 3: Validación de Correo
*For any* correo almacenado en el sistema, debe tener un formato válido de email (contiene @ y dominio válido).
**Validates: Requirements 3.3, 7.2**

### Property 4: Validación de Teléfono
*For any* teléfono almacenado en el sistema, debe contener solo dígitos y tener entre 7 y 15 caracteres.
**Validates: Requirements 3.4, 7.3**

### Property 5: Inmutabilidad de Value Objects
*For any* Value Object (Correo, Telefono, Direccion), una vez creado no puede ser modificado (inmutable).
**Validates: Requirements 3.3, 3.4, 3.5**

### Property 6: Eliminación Lógica
*For any* usuario eliminado, su propiedad EstaActivo debe ser false y no debe aparecer en consultas de usuarios activos.
**Validates: Requirements 5.5**

### Property 7: Sincronización con Keycloak
*For any* usuario creado en el sistema, debe existir un usuario correspondiente en Keycloak con el mismo ID.
**Validates: Requirements 6.1, 6.2**

### Property 8: Validación de Comandos
*For any* comando recibido, si los datos son inválidos, el sistema debe retornar error de validación antes de ejecutar lógica de negocio.
**Validates: Requirements 7.1**

### Property 9: Logging de Operaciones
*For any* operación (comando o query), debe existir una entrada de log correspondiente con timestamp y resultado.
**Validates: Requirements 11.2, 11.3**

### Property 10: Transaccionalidad
*For any* operación que falla después de modificar Keycloak, los cambios en la base de datos no deben persistirse.
**Validates: Requirements 6.5**

## Error Handling

### Domain Exceptions

- **UsuarioNoEncontradoException** → HTTP 404 Not Found
- **CorreoDuplicadoException** → HTTP 400 Bad Request
- **UsernameDuplicadoException** → HTTP 400 Bad Request
- **ArgumentException** (validaciones de Value Objects) → HTTP 400 Bad Request

### Application Exceptions

- **ValidationException** (FluentValidation) → HTTP 400 Bad Request con detalles

### Infrastructure Exceptions

- **DbUpdateException** → HTTP 500 Internal Server Error
- **HttpRequestException** (Keycloak) → HTTP 502 Bad Gateway

### Middleware de Manejo de Excepciones

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UsuarioNoEncontradoException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
        }
        catch (CorreoDuplicadoException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (UsernameDuplicadoException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (HttpRequestException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadGateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
        }
    }
}
```

## Testing Strategy

### Unit Tests (xUnit + Moq + FluentAssertions)

**Dominio:**
- Tests de entidad Usuario (creación, actualización, validaciones)
- Tests de Value Objects (Correo, Telefono, Direccion)
- Tests de métodos de negocio

**Aplicación:**
- Tests de Handlers (mocking repositorios y servicios)
- Tests de Validators (FluentValidation)

**Infraestructura:**
- Tests de Repositorio (usando InMemory database)
- Tests de ServicioKeycloak (mocking HttpClient)

### Integration Tests

- Tests de endpoints de API (usando WebApplicationFactory)
- Tests de persistencia con PostgreSQL real (Testcontainers)

### Coverage Target

- Objetivo: >90% de cobertura de código
- Herramienta: Coverlet

## Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Usuarios.API/Usuarios.API.csproj", "Usuarios.API/"]
COPY ["src/Usuarios.Aplicacion/Usuarios.Aplicacion.csproj", "Usuarios.Aplicacion/"]
COPY ["src/Usuarios.Dominio/Usuarios.Dominio.csproj", "Usuarios.Dominio/"]
COPY ["src/Usuarios.Infraestructura/Usuarios.Infraestructura.csproj", "Usuarios.Infraestructura/"]
RUN dotnet restore "Usuarios.API/Usuarios.API.csproj"
COPY src/ .
WORKDIR "/src/Usuarios.API"
RUN dotnet build "Usuarios.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Usuarios.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Usuarios.API.dll"]
```

### Docker Compose

```yaml
version: '3.8'

services:
  usuarios-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: kairo-usuarios-api
    ports:
      - "8083:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgresConnection=Host=postgres;Database=kairo_usuarios;Username=postgres;Password=postgres
      - Keycloak__Authority=http://keycloak:8080/realms/Kairo
    networks:
      - kairo-network
    depends_on:
      - postgres
      - keycloak

networks:
  kairo-network:
    external: true
```

## Migration Path

### Fase 1: Preparación
1. Crear nueva estructura de carpetas
2. Copiar código existente como referencia
3. Configurar proyectos y dependencias

### Fase 2: Dominio
1. Crear entidad Usuario con lógica de negocio
2. Crear Value Objects (Correo, Telefono, Direccion)
3. Crear interfaces de repositorio
4. Crear excepciones de dominio

### Fase 3: Aplicación
1. Crear Commands y Queries
2. Crear Handlers
3. Crear DTOs
4. Crear Validators

### Fase 4: Infraestructura
1. Crear DbContext y configuraciones
2. Implementar RepositorioUsuarios
3. Implementar ServicioKeycloak
4. Configurar migraciones

### Fase 5: API
1. Crear Controllers
2. Configurar middleware
3. Configurar inyección de dependencias
4. Configurar Serilog

### Fase 6: Testing
1. Escribir tests unitarios
2. Escribir tests de integración
3. Verificar cobertura >90%

### Fase 7: Deployment
1. Crear Dockerfile
2. Actualizar docker-compose
3. Probar en entorno local
4. Documentar
