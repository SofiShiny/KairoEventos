using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Entradas.Dominio.Interfaces;

namespace Entradas.Infraestructura.Servicios;

/// <summary>
/// Implementación del generador de códigos QR únicos para entradas
/// Utiliza componentes criptográficamente seguros para garantizar unicidad
/// </summary>
public class GeneradorCodigoQr : IGeneradorCodigoQr
{
    private static readonly Regex FormatoQrRegex = new(@"^TICKET-[A-F0-9]{8}-\d{4}$", RegexOptions.Compiled);

    /// <summary>
    /// Genera un código QR único con formato "TICKET-{Guid}-{Random}"
    /// Utiliza Guid para unicidad y RandomNumberGenerator para seguridad criptográfica
    /// </summary>
    /// <returns>Código QR único como string</returns>
    public string GenerarCodigoUnico()
    {
        // Generar parte del Guid (8 caracteres hexadecimales en mayúsculas)
        var guid = Guid.NewGuid().ToString("N")[..8].ToUpper();
        
        // Generar número aleatorio criptográficamente seguro (4 dígitos)
        var random = GenerarNumeroAleatorioSeguro(1000, 9999);
        
        return $"TICKET-{guid}-{random}";
    }

    /// <summary>
    /// Valida que un código QR tenga el formato correcto "TICKET-{Guid}-{Random}"
    /// </summary>
    /// <param name="codigoQr">Código QR a validar</param>
    /// <returns>True si el formato es válido, false en caso contrario</returns>
    public bool ValidarFormato(string codigoQr)
    {
        if (string.IsNullOrWhiteSpace(codigoQr))
            return false;

        return FormatoQrRegex.IsMatch(codigoQr);
    }

    /// <summary>
    /// Genera un número aleatorio criptográficamente seguro en el rango especificado
    /// </summary>
    /// <param name="minValue">Valor mínimo (inclusivo)</param>
    /// <param name="maxValue">Valor máximo (inclusivo)</param>
    /// <returns>Número aleatorio seguro</returns>
    private static int GenerarNumeroAleatorioSeguro(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentException("El valor mínimo debe ser menor que el valor máximo");

        using var rng = RandomNumberGenerator.Create();
        var range = maxValue - minValue + 1;
        var bytes = new byte[4];
        
        int result;
        do
        {
            rng.GetBytes(bytes);
            result = Math.Abs(BitConverter.ToInt32(bytes, 0)) % range + minValue;
        } 
        while (result < minValue || result > maxValue);

        return result;
    }
}