using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuarios.Application.Dtos;
using Usuarios.Domain;

namespace Usuarios.Application.Handlers.Commands.Command
{
    public class EliminarUsuarioCommand : IRequest
    {
        public required Guid Id { get; set; }
        public required EliminarUsuarioDto EliminarUsuarioDto { get; set; }
    }
}
