using BloquesConstruccion.Aplicacion.Comandos;
using BloquesConstruccion.Aplicacion.Comun;
namespace Eventos.Aplicacion.Comandos;

public record EliminarEventoComando(Guid EventoId) : IComando<Resultado>;