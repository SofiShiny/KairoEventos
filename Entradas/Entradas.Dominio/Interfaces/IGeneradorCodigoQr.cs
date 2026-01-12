namespace Entradas.Dominio.Interfaces;

/// <summary>
/// Interface para generar códigos QR únicos para las entradas
/// </summary>
public interface IGeneradorCodigoQr
{
    /// <summary>
    /// Genera un código QR único con formato "TICKET-{Guid}-{Random}"
    /// </summary>
    /// <returns>Código QR único como string</returns>
    string GenerarCodigoUnico();

    /// <summary>
    /// Valida que un código QR tenga el formato correcto
    /// </summary>
    /// <param name="codigoQr">Código QR a validar</param>
    /// <returns>True si el formato es válido, false en caso contrario</returns>
    bool ValidarFormato(string codigoQr);
}