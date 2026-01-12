namespace BloquesConstruccion.Dominio;

public abstract class RaizAgregada : Entidad
{
 private readonly List<EventoDominio> _eventos = new();
 public IReadOnlyCollection<EventoDominio> Eventos => _eventos.AsReadOnly();
 protected void GenerarEventoDominio(EventoDominio evento) => _eventos.Add(evento);
}
