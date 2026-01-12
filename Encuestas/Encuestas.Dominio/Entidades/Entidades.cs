namespace Encuestas.Dominio.Entidades;

public enum TipoPregunta
{
    Estrellas,
    Texto
}

public class Encuesta
{
    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public bool Publicada { get; private set; }
    public ICollection<Pregunta> Preguntas { get; private set; } = new List<Pregunta>();

    private Encuesta() { }

    public Encuesta(Guid id, Guid eventoId, string titulo)
    {
        Id = id;
        EventoId = eventoId;
        Titulo = titulo;
        Publicada = false;
    }

    public void Publicar() => Publicada = true;
    public void Despublicar() => Publicada = false;

    public void AgregarPregunta(Pregunta pregunta) => Preguntas.Add(pregunta);
}

public class Pregunta
{
    public Guid Id { get; private set; }
    public Guid EncuestaId { get; private set; }
    public string Enunciado { get; private set; } = string.Empty;
    public TipoPregunta Tipo { get; private set; }

    private Pregunta() { }

    public Pregunta(Guid id, string enunciado, TipoPregunta tipo)
    {
        Id = id;
        Enunciado = enunciado;
        Tipo = tipo;
    }
}

public class RespuestaUsuario
{
    public Guid Id { get; private set; }
    public Guid EncuestaId { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime Fecha { get; private set; }
    public ICollection<ValorRespuesta> Valores { get; private set; } = new List<ValorRespuesta>();

    private RespuestaUsuario() { }

    public RespuestaUsuario(Guid encuestaId, Guid usuarioId)
    {
        Id = Guid.NewGuid();
        EncuestaId = encuestaId;
        UsuarioId = usuarioId;
        Fecha = DateTime.UtcNow;
    }

    public void AgregarRespuesta(Guid preguntaId, string valor)
    {
        Valores.Add(new ValorRespuesta(Id, preguntaId, valor));
    }
}

public class ValorRespuesta
{
    public Guid Id { get; private set; }
    public Guid RespuestaUsuarioId { get; private set; }
    public Guid PreguntaId { get; private set; }
    public string Valor { get; private set; } = string.Empty;

    private ValorRespuesta() { }

    public ValorRespuesta(Guid respuestaUsuarioId, Guid preguntaId, string valor)
    {
        Id = Guid.NewGuid();
        RespuestaUsuarioId = respuestaUsuarioId;
        PreguntaId = preguntaId;
        Valor = valor;
    }
}
