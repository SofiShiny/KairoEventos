using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using Usuarios.API.Controllers;
using Usuarios.Application.Dtos;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Handlers;
using Usuarios.Application.Handlers.Commands;
using Usuarios.Application.Handlers.Commands.Command;
using Usuarios.Application.Handlers.Querys;
using Usuarios.Application.Handlers.Querys.Query;
using Usuarios.Domain.Entidades;

namespace Usuarios.Test.API.Controllers
{
    public class TestUsuarioController
    {
        private Mock<IMediator> _meDiator;
        private Mock<IMapper> _mapPer;
        private Mock<ILogger<UsuarioController>> _logger;
        private UsuarioController _Controller;
        private IEnumerable<ConsultarUsuarioDto> _ConsultarUsuarioDtos;
        private ConsultarUsuarioDto _ConsultarUsuarioDto;
        private ConsultarUsuarioDto _ConsultarUsuarioAdminDto;
        private AgregarUsuarioDto _agregarUsuarioDto;
        private ActualizarUsuarioDto _actualizarUsuarioDto;
        private EliminarUsuarioDto EliminarUsuarioDto;
        private ConsultarPerfilUsuarioPorIdDto _perfilUsuarioPorIdDto;
        private RegistroTokenDto TokenDto;

        public TestUsuarioController()
        {
            _meDiator = new();
            _mapPer = new();
            _logger = new();
            _Controller = new(_meDiator.Object,_mapPer.Object,_logger.Object);

            _ConsultarUsuarioDto = new()
            {
                IdUsuario = Guid.Empty,
                Username = "test",
                Nombre = "test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test",
                Rol = "test",
                IsActive = true
            };

            _ConsultarUsuarioDtos = new[] { _ConsultarUsuarioDto };

            _agregarUsuarioDto = new()
            {
                Username = "test",
                Nombre = "test",
                Contrasena = "test123456789*",
                ConfirmarContrasena = "test123456789*",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test",
                Rol = "Organizador"
            };

            _actualizarUsuarioDto = new()
            {
                Nombre = "test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test"
            };

            EliminarUsuarioDto = new()
            {
                Nombre = "test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test"
            };

            _ConsultarUsuarioAdminDto = new()
            {
                IdUsuario = Guid.Empty,
                Username = "test",
                Nombre = "test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test",
                Rol = "Administrador",
                IsActive = true
            };

            _perfilUsuarioPorIdDto = new()
            {
                Nombre = "test",
                Correo = "test@gmail.com",
                Telefono = "12345678910",
                Direccion = "test"
            };

            TokenDto = new()
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30"

            };
        }

        [Fact]
        public async Task Test_ConsultarUsuario_ExistenRegistros_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuariosQuery>(), CancellationToken.None)).ReturnsAsync(_ConsultarUsuarioDtos);

            var result = await _Controller.ConsultarUsuario(It.IsAny<string>());

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Test_ConsultarUsuario_NoHayRegistros_NoContent()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuariosQuery>(), CancellationToken.None)).ThrowsAsync(new RegistroNoEncontradoException("Usuario no encontrado"));

            var result = await _Controller.ConsultarUsuario(It.IsAny<string>());

            Assert.IsType<NoContentResult>(result.Result);
        }

        [Fact]
        public async Task Test_AgregarUsuario_SeRegistroSatisfactoriamente_CreatedAt()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<AgregarUsuarioCommand>(), CancellationToken.None)).ReturnsAsync(_agregarUsuarioDto);

            var result = await _Controller.AgregarUsuario(_agregarUsuarioDto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task Test_ActualizarUsuario_SeActualizoSatisfactoriamente_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ReturnsAsync(It.IsAny<ConsultarUsuarioDto>());
            _meDiator.Setup(m => m.Send(It.IsAny<ActualizarUsuarioCommand>(), CancellationToken.None)).ReturnsAsync(_actualizarUsuarioDto);

            var result = await _Controller.ActualizarUsuario(It.IsAny<Guid>(), _actualizarUsuarioDto);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Test_ActualizarUsuario_NoEncontroElUsuario_NotFound()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ThrowsAsync(new RegistroNoEncontradoException("No se encontro el usuario"));

            var result = await _Controller.ActualizarUsuario(It.IsAny<Guid>(), It.IsAny<ActualizarUsuarioDto>());

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Test_EliminarUsuario_SeActualizoSatisfactoriamente_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ReturnsAsync(_ConsultarUsuarioDto);
            _meDiator.Setup(m => m.Send(It.IsAny<EliminarUsuarioCommand>(), CancellationToken.None)).Returns(Task.CompletedTask);
            _mapPer.Setup(m => m.Map<EliminarUsuarioDto>(It.IsAny<Usuario>())).Returns(EliminarUsuarioDto);

            var result = await _Controller.EliminarUsuario(It.IsAny<Guid>());

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Test_EliminarUsuario_NoEncontroElUsuario_NotFound()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ThrowsAsync(new RegistroNoEncontradoException("No se encontro el usuario"));

            var result = await _Controller.EliminarUsuario(It.IsAny<Guid>());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Test_EliminarUsuario_ElUsuarioAEliminarEsAdministrador_NotFound()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ReturnsAsync(_ConsultarUsuarioAdminDto);

            var result = await _Controller.EliminarUsuario(It.IsAny<Guid>());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_ConsultarPerfilUsuarioPorId_ExistenRegistros_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarPerfilUsuarioPorIdQuery>(), CancellationToken.None)).ReturnsAsync(_perfilUsuarioPorIdDto);

            var result = await _Controller.ConsultarPerfilUsuarioPorId(It.IsAny<Guid>());

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Test_ConsultarPerfilUsuarioPorId_NoSeEncontroElRegistro_NoContent()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarPerfilUsuarioPorIdQuery>(), CancellationToken.None)).ThrowsAsync(new RegistroNoEncontradoException("Usuario no encontrado"));

            var result = await _Controller.ConsultarPerfilUsuarioPorId(It.IsAny<Guid>());

            Assert.IsType<NotFoundResult>(result.Result);
        }
        [Fact]
        public async Task Test_RegistroUsuarioToken_RegistraElUsuario_CreationAd()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<RegistroTokenCommand>(), CancellationToken.None));

            var result = await _Controller.RegistroUsuarioToken(TokenDto);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task Test_RegistroUsuarioToken_YaExisteElUsuario_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<RegistroTokenCommand>(), CancellationToken.None)).ThrowsAsync(new InvalidOperationException("El usuario se encontro"));

            var result = await _Controller.RegistroUsuarioToken(TokenDto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Test_ConsultarUsuarioPorId_EncontroElRegistro_Ok()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ReturnsAsync(_ConsultarUsuarioDto);

            var result = await _Controller.ConsultarUsuarioPorId(It.IsAny<Guid>());

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Test_ConsultarUsuarioPorId_NoSeEncontroElRegistro_NoContent()
        {
            _meDiator.Setup(m => m.Send(It.IsAny<ConsultarUsuarioPorIdQuery>(), CancellationToken.None)).ThrowsAsync(new RegistroNoEncontradoException("Usuario no encontrado"));

            var result = await _Controller.ConsultarUsuarioPorId(It.IsAny<Guid>());

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
