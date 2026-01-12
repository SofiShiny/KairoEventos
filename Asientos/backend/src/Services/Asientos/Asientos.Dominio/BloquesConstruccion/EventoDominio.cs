namespace BloquesConstruccion.Dominio;

public abstract class EventoDominio
{
 public Guid Id { get; } = Guid.NewGuid();
 public DateTime OcurrioEn { get; } = DateTime.UtcNow;
 public Guid IdAgregado { get; protected set; }
}
