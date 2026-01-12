using Marketing.Dominio.Enums;

namespace Marketing.API.DTOs;

public record CrearCuponDto(string Codigo, TipoDescuento Tipo, decimal Valor, DateTime FechaExpiracion);

public record EnviarCuponDto(Guid UsuarioId);

public record ValidarCuponDto(string Codigo);

public record CuponResponseDto(
    Guid Id, 
    string Codigo, 
    string Tipo, 
    decimal Valor, 
    DateTime Expiracion, 
    string Estado,
    Guid? Destinatario,
    Guid? QuienLoUso,
    DateTime? FechaUso);
