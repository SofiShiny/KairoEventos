using MediatR;
using Usuarios.Application.Dtos;

namespace Usuarios.Application.Handlers.Querys.Query;

public class ConsultarPerfilUsuarioPorIdQuery: IRequest<ConsultarPerfilUsuarioPorIdDto>
{
    public Guid IdUsuario { get; set; }
}