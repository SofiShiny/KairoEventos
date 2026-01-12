using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Usuarios.Application.Dtos;
using Usuarios.Domain.Entidades;

namespace Usuarios.Application.Handlers.Querys.Query
{
    public class ConsultarUsuariosQuery : IRequest<IEnumerable<ConsultarUsuarioDto>>
    {
        public required BusquedaUsuarioDto Busqueda { get; set; }
    }
}
