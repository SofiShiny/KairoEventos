namespace Recomendaciones.Dominio.Entidades;

public class AfinidadUsuario
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Categoria { get; private set; } = string.Empty;
    public int Puntuacion { get; private set; }

    private AfinidadUsuario() { }

    public AfinidadUsuario(Guid usuarioId, string categoria)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Categoria = categoria;
        Puntuacion = 0;
    }

    public void SumarPuntos(int cantidad)
    {
        Puntuacion += cantidad;
    }
}

public class EventoProyeccion
{
    public Guid EventoId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Categoria { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public bool Activo { get; private set; }

    private EventoProyeccion() { }

    public EventoProyeccion(Guid eventoId, string titulo, string categoria, DateTime fecha)
    {
        EventoId = eventoId;
        Titulo = titulo;
        Categoria = categoria;
        Fecha = fecha;
        Activo = true;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
