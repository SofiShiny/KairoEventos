using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Dominio.Enums;
using Usuarios.Dominio.Servicios;
using Usuarios.Infraestructura.Persistencia;

namespace Usuarios.Pruebas.API;

public class UsuariosIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsuariosIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task FlujoCRUD_CrearObtenerActualizarEliminar_FuncionaCorrectamente()
    {
        // 1. Crear usuario
        var crearDto = new CrearUsuarioDto
        {
            Username = "integrationuser",
            Nombre = "Integration Test User",
            Correo = "integration@test.com",
            Telefono = "1234567890",
            Direccion = "Integration Test Address 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/usuarios", crearDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var usuarioId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        usuarioId.Should().NotBeEmpty();

        // 2. Obtener usuario creado
        var getResponse = await _client.GetAsync($"/api/usuarios/{usuarioId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var usuarioDto = await getResponse.Content.ReadFromJsonAsync<UsuarioDto>();
        usuarioDto.Should().NotBeNull();
        usuarioDto!.Id.Should().Be(usuarioId);
        usuarioDto.Username.Should().Be(crearDto.Username);
        usuarioDto.Nombre.Should().Be(crearDto.Nombre);
        usuarioDto.Correo.Should().Be(crearDto.Correo.ToLowerInvariant());
        usuarioDto.Rol.Should().Be(crearDto.Rol);

        // 3. Actualizar usuario
        var actualizarDto = new ActualizarUsuarioDto
        {
            Nombre = "Updated Integration User",
            Telefono = "9876543210",
            Direccion = "Updated Integration Address 456"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/usuarios/{usuarioId}", actualizarDto);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. Verificar actualización
        var getUpdatedResponse = await _client.GetAsync($"/api/usuarios/{usuarioId}");
        getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var usuarioActualizado = await getUpdatedResponse.Content.ReadFromJsonAsync<UsuarioDto>();
        usuarioActualizado.Should().NotBeNull();
        usuarioActualizado!.Nombre.Should().Be(actualizarDto.Nombre);
        usuarioActualizado.Telefono.Should().Be(actualizarDto.Telefono);
        usuarioActualizado.Direccion.Should().Be(actualizarDto.Direccion);

        // 5. Eliminar usuario
        var deleteResponse = await _client.DeleteAsync($"/api/usuarios/{usuarioId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verificar que el usuario ya no está activo
        var getDeletedResponse = await _client.GetAsync($"/api/usuarios/{usuarioId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearUsuario_ConCorreoDuplicado_Retorna400BadRequest()
    {
        // Arrange - Crear primer usuario
        var primerUsuario = new CrearUsuarioDto
        {
            Username = "user1",
            Nombre = "User One",
            Correo = "duplicate@test.com",
            Telefono = "1111111111",
            Direccion = "Address One",
            Rol = Rol.User,
            Password = "Password123"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/usuarios", primerUsuario);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Intentar crear segundo usuario con mismo correo
        var segundoUsuario = new CrearUsuarioDto
        {
            Username = "user2",
            Nombre = "User Two",
            Correo = "duplicate@test.com", // Mismo correo
            Telefono = "2222222222",
            Direccion = "Address Two",
            Rol = Rol.User,
            Password = "Password456"
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/usuarios", segundoUsuario);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("correo");
    }

    [Fact]
    public async Task CrearUsuario_ConUsernameDuplicado_Retorna400BadRequest()
    {
        // Arrange - Crear primer usuario
        var primerUsuario = new CrearUsuarioDto
        {
            Username = "duplicateuser",
            Nombre = "User One",
            Correo = "user1@test.com",
            Telefono = "1111111111",
            Direccion = "Address One",
            Rol = Rol.User,
            Password = "Password123"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/usuarios", primerUsuario);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Intentar crear segundo usuario con mismo username
        var segundoUsuario = new CrearUsuarioDto
        {
            Username = "duplicateuser", // Mismo username
            Nombre = "User Two",
            Correo = "user2@test.com",
            Telefono = "2222222222",
            Direccion = "Address Two",
            Rol = Rol.User,
            Password = "Password456"
        };

        var secondResponse = await _client.PostAsJsonAsync("/api/usuarios", segundoUsuario);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("username");
    }

    [Fact]
    public async Task ObtenerUsuario_ConIdInexistente_Retorna404NotFound()
    {
        // Arrange
        var usuarioIdInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/usuarios/{usuarioIdInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerTodosLosUsuarios_RetornaListaDeUsuariosActivos()
    {
        // Arrange - Crear varios usuarios
        var usuarios = new[]
        {
            new CrearUsuarioDto
            {
                Username = "listuser1",
                Nombre = "List User 1",
                Correo = "listuser1@test.com",
                Telefono = "1111111111",
                Direccion = "Address 1",
                Rol = Rol.User,
                Password = "Password123"
            },
            new CrearUsuarioDto
            {
                Username = "listuser2",
                Nombre = "List User 2",
                Correo = "listuser2@test.com",
                Telefono = "2222222222",
                Direccion = "Address 2",
                Rol = Rol.Admin,
                Password = "Password456"
            }
        };

        var usuarioIds = new List<Guid>();
        foreach (var usuario in usuarios)
        {
            var response = await _client.PostAsJsonAsync("/api/usuarios", usuario);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            usuarioIds.Add(id);
        }

        // Act
        var getResponse = await _client.GetAsync("/api/usuarios");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var usuariosDto = await getResponse.Content.ReadFromJsonAsync<List<UsuarioDto>>();
        usuariosDto.Should().NotBeNull();
        usuariosDto!.Should().HaveCountGreaterOrEqualTo(2);
        usuariosDto.Should().Contain(u => u.Username == "listuser1");
        usuariosDto.Should().Contain(u => u.Username == "listuser2");
    }
}

// Custom WebApplicationFactory for integration tests
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "TestDatabase_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<UsuariosDbContext>));
            services.RemoveAll(typeof(UsuariosDbContext));

            // Add InMemory database for testing with a fixed name per factory instance
            services.AddDbContext<UsuariosDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            // Mock Keycloak service to avoid external dependencies
            services.RemoveAll(typeof(IServicioKeycloak));
            services.AddScoped<IServicioKeycloak, MockServicioKeycloak>();

            // Build service provider and ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UsuariosDbContext>();
            db.Database.EnsureCreated();
        });
    }
}

// Mock implementation of IServicioKeycloak for testing
public class MockServicioKeycloak : IServicioKeycloak
{
    public Task<string> CrearUsuarioAsync(
        Usuarios.Dominio.Entidades.Usuario usuario,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Mock implementation - just return the user ID
        return Task.FromResult(usuario.Id.ToString());
    }

    public Task ActualizarUsuarioAsync(
        Usuarios.Dominio.Entidades.Usuario usuario,
        CancellationToken cancellationToken = default)
    {
        // Mock implementation - do nothing
        return Task.CompletedTask;
    }

    public Task DesactivarUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        // Mock implementation - do nothing
        return Task.CompletedTask;
    }

    public Task AsignarRolAsync(
        Guid usuarioId,
        Rol rol,
        CancellationToken cancellationToken = default)
    {
        // Mock implementation - do nothing
        return Task.CompletedTask;
    }
}
