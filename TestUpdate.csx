
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

var client = new HttpClient();
var id = "TU_EVENTO_ID"; // Necesito un ID real o buscar uno
var baseUrl = "http://localhost:5001/api/eventos";

var resp = await client.GetAsync(baseUrl);
var body = await resp.Content.ReadAsStringAsync();
var eventos = JsonDocument.Parse(body).RootElement;
var evento = eventos[0];
var guid = evento.GetProperty("id").GetString();

Console.WriteLine($"Probando con Evento: {guid}");

var updateDto = new {
    titulo = evento.GetProperty("titulo").GetString(),
    descripcion = evento.GetProperty("descripcion").GetString(),
    ubicacion = evento.GetProperty("ubicacion"),
    fechaInicio = evento.GetProperty("fechaInicio").GetDateTime(),
    fechaFin = evento.GetProperty("fechaFin").GetDateTime(),
    maximoAsistentes = evento.GetProperty("maximoAsistentes").GetInt32(),
    precioBase = 150.5m,
    esVirtual = true
};

var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
var putResp = await client.PutAsync($"{baseUrl}/{guid}", content);
Console.WriteLine($"Status PUT: {putResp.StatusCode}");

var getResp = await client.GetAsync($"{baseUrl}/{guid}");
var getBody = await getResp.Content.ReadAsStringAsync();
Console.WriteLine($"Body recuperado: {getBody}");
