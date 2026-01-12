using FluentAssertions;
using Usuarios.Aplicacion.DTOs;
using Usuarios.Aplicacion.Validadores;
using Usuarios.Dominio.Enums;

namespace Usuarios.Pruebas.Aplicacion;

public class CrearUsuarioDtoValidatorTests
{
    private readonly CrearUsuarioDtoValidator _validator;

    public CrearUsuarioDtoValidatorTests()
    {
        _validator = new CrearUsuarioDtoValidator();
    }

    [Fact]
    public void Validate_ConDtoValido_PasaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
        resultado.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ConUsernameVacio_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Username");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void Validate_ConUsernameCorto_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "ab",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Username");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("al menos 3 caracteres"));
    }

    [Fact]
    public void Validate_ConCorreoInvalido_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "correo-invalido",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Correo");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("no es válido"));
    }

    [Fact]
    public void Validate_ConPasswordCorto_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Pass123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Password");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("al menos 8 caracteres"));
    }

    [Fact]
    public void Validate_ConRolInvalido_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = (Rol)999, // Rol inválido
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Rol");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("no es válido"));
    }

    [Fact]
    public void Validate_ConNombreVacio_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nombre");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void Validate_ConTelefonoVacio_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "",
            Direccion = "Calle Test 123",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Telefono");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("requerido"));
    }

    [Fact]
    public void Validate_ConDireccionCorta_FallaValidacion()
    {
        // Arrange
        var dto = new CrearUsuarioDto
        {
            Username = "testuser",
            Nombre = "Test User",
            Correo = "test@example.com",
            Telefono = "1234567890",
            Direccion = "Cal",
            Rol = Rol.User,
            Password = "Password123"
        };

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Direccion");
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("al menos 5 caracteres"));
    }
}
