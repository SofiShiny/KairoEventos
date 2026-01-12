using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Usuarios.Application.Dtos;
using Usuarios.Application.Dtos.UsuarioKeycloakDto;
using Usuarios.Application.Validators;
using Usuarios.Core.Repository;
using Usuarios.Core.Services;

namespace Usuarios.Test.Application.Handlers;

public class DataSetTestCommandHandlers
{
    protected Mock<IAccesManagement<UsuarioKeycloak>> accesManagement;
    protected Mock<IRepository> repository;
    protected ValidationResult _result;
    protected ValidationResult _resultInValid;
    protected Mock<IMapper> mapper;

    public DataSetTestCommandHandlers()
    {
        accesManagement = new();
        repository = new();
        _result = new();
        var failures = new List<ValidationFailure>
        {
            new ("test", "test")
        };
        _resultInValid = new(failures);
        mapper = new();
    }
}