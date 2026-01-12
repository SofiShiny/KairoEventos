// Contratos espejo para publicar eventos de prueba
namespace Eventos.Dominio.EventosDeDominio;

public record EventoPublicadoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; } = string.Empty;
    public DateTime FechaInicio { get; init; }
    public DateTime FechaFin { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string Ubicacion { get; init; } = string.Empty;
    public int CapacidadMaxima { get; init; }
    public string OrganizadorId { get; init; } = string.Empty;
}

public record AsistenteRegistradoEventoDominio
{
    public Guid EventoId { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public string NombreUsuario { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime FechaRegistro { get; init; }
}

public record EventoCanceladoEventoDominio
{
    public Guid EventoId { get; init; }
    public string TituloEvento { get; init; } = string.Empty;
    public string Razon { get; init; } = string.Empty;
    public DateTime FechaCancelacion { get; init; }
}

namespace Asientos.Dominio.EventosDominio;

public record AsientoReservadoEventoDominio
{
    public Guid MapaId { get; init; }
    public Guid EventoId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public string CategoriaAsiento { get; init; } = string.Empty;
    public decimal Precio { get; init; }
    public DateTime FechaReserva { get; init; }
}

public record AsientoLiberadoEventoDominio
{
    public Guid MapaId { get; init; }
    public Guid EventoId { get; init; }
    public int Fila { get; init; }
    public int Numero { get; init; }
    public DateTime FechaLiberacion { get; init; }
}

public record MapaAsientosCreadoEventoDominio
{
    public Guid MapaId { get; init; }
    public Guid EventoId { get; init; }
    public int CapacidadTotal { get; init; }
    public DateTime FechaCreacion { get; init; }
}
