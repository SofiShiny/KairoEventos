using System;

namespace BloquesConstruccion.Dominio;

public abstract class EventoDominio
{
    public Guid IdAgregado { get; protected set; }
    public DateTime OcurrioEn { get; protected set; } = DateTime.UtcNow;
}
