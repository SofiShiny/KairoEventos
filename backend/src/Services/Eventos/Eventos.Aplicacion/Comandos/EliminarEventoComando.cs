using BloquesConstruccion.Aplicacion.Comun;
using BloquesConstruccion.Aplicacion.Comandos;

namespace Eventos.Aplicacion.Comandos;

public record EliminarEventoComando(Guid EventoId) : IComando<Resultado>;