using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;

namespace Eventos.Aplicacion.Comandos;

public record PublicarEventoComando(Guid EventoId) : IComando<Resultado>;
