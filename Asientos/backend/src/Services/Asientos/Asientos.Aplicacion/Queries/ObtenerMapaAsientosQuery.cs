using MediatR;

namespace Asientos.Aplicacion.Queries;

public record ObtenerMapaAsientosQuery(Guid MapaId) : IRequest<MapaAsientosDto?>;

public record MapaAsientosDto(
    Guid MapaId,
    Guid EventoId,
    List<CategoriaDto> Categorias,
    List<AsientoDto> Asientos
);

public record CategoriaDto(string Nombre, decimal? PrecioBase, bool TienePrioridad);
public record AsientoDto(Guid Id, int Fila, int Numero, string Categoria, decimal Precio, bool Reservado);
