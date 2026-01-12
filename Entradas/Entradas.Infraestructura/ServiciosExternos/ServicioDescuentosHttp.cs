using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Entradas.Dominio.Interfaces;
using Entradas.Dominio.Enums;

namespace Entradas.Infraestructura.ServiciosExternos;

public class ServicioDescuentosHttp : IServicioDescuentos
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServicioDescuentosHttp> _logger;
    private readonly string _baseUrl;

    public ServicioDescuentosHttp(
        HttpClient httpClient, 
        IConfiguration configuration, 
        ILogger<ServicioDescuentosHttp> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["ServiciosExternos:MarketingUrl"] ?? "http://marketing-api:8080";
    }

    public async Task<ResultadoDescuento> CalcularDescuentoAsync(Guid usuarioId, List<string> cupones, decimal montoOriginal)
    {
        if (cupones == null || !cupones.Any())
            return new ResultadoDescuento(false, 0, "No se proporcionaron cupones.");

        decimal descuentoTotal = 0;
        var cuponesProcesados = new List<string>();

        try
        {
            foreach (var codigo in cupones)
            {
                _logger.LogInformation("Validando cupón {Codigo} para usuario {UsuarioId}", codigo, usuarioId);

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Marketing/validar", new { Codigo = codigo });

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<MarketingValidacionResponse>();
                    if (data != null && data.EsValido)
                    {
                        decimal valorDescuento = 0;
                        if (data.Tipo == "Porcentaje")
                        {
                            valorDescuento = montoOriginal * (data.Valor / 100);
                        }
                        else // MontoFijo
                        {
                            valorDescuento = data.Valor;
                        }

                        descuentoTotal += valorDescuento;
                        cuponesProcesados.Add(codigo);
                    }
                }
                else
                {
                    _logger.LogWarning("El cupón {Codigo} no es válido o hubo un error. Status: {Status}", codigo, response.StatusCode);
                }
            }

            if (descuentoTotal > 0)
            {
                return new ResultadoDescuento(true, descuentoTotal, $"Se aplicaron {cuponesProcesados.Count} cupones.");
            }
        }
        catch (Exception ex)
        {
            // RESILIENCIA: Fail-Safe
            _logger.LogError(ex, "Error conectando con Marketing.API. Se procederá sin descuento.");
            return new ResultadoDescuento(false, 0, "Error de conexión con el servicio de marketing.");
        }

        return new ResultadoDescuento(false, 0, "No se aplicaron descuentos.");
    }
}

public class MarketingValidacionResponse
{
    public bool EsValido { get; set; }
    public decimal Valor { get; set; }
    public string Tipo { get; set; } = string.Empty; // "Porcentaje" o "MontoFijo"
    public string Mensaje { get; set; } = string.Empty;
}
