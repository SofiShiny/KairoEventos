using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;

namespace Eventos.Aplicacion.Comandos;

public record RegistrarAsistenteComando(
    Guid EventoId,
    string UsuarioId,
    string NombreUsuario,
    string Correo
) : IComando<Resultado>;
