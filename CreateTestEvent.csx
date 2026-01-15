using System;
using System.Data;
using Npgsql;

// Script simple para insertar el evento de prueba
var connectionString = "Host=localhost;Database=eventsdb;Username=postgres;Password=postgres";

using var connection = new NpgsqlConnection(connectionString);
connection.Open();

var sql = @"
INSERT INTO ""Eventos"" (
    ""Id"", ""Titulo"", ""Descripcion"", ""FechaInicio"", ""FechaFin"", 
    ""MaximoAsistentes"", ""OrganizadorId"", ""Categoria"", ""Estado"", ""PrecioBase"",
    ""NombreLugar"", ""Direccion"", ""Ciudad"", ""Pais""
) VALUES (
    @id, @titulo, @descripcion, @fechaInicio, @fechaFin,
    @maximoAsistentes, @organizadorId, @categoria, @estado, @precioBase,
    @nombreLugar, @direccion, @ciudad, @pais
)
ON CONFLICT (""Id"") DO UPDATE SET
    ""Estado"" = @estado;
";

using var command = new NpgsqlCommand(sql, connection);
command.Parameters.AddWithValue("id", Guid.Parse("6e92cc27-e4df-4602-8819-1756a038a7ce"));
command.Parameters.AddWithValue("titulo", "Concierto de Rock 2026");
command.Parameters.AddWithValue("descripcion", "Gran concierto de rock en vivo");
command.Parameters.AddWithValue("fechaInicio", new DateTime(2026, 2, 15, 20, 0, 0));
command.Parameters.AddWithValue("fechaFin", new DateTime(2026, 2, 15, 23, 0, 0));
command.Parameters.AddWithValue("maximoAsistentes", 1000);
command.Parameters.AddWithValue("organizadorId", "org-001");
command.Parameters.AddWithValue("categoria", "Musica");
command.Parameters.AddWithValue("estado", "Publicado");
command.Parameters.AddWithValue("precioBase", 50.00m);
command.Parameters.AddWithValue("nombreLugar", "Estadio Nacional");
command.Parameters.AddWithValue("direccion", "Av. Grecia 2001");
command.Parameters.AddWithValue("ciudad", "Santiago");
command.Parameters.AddWithValue("pais", "Chile");

var rowsAffected = command.ExecuteNonQuery();
Console.WriteLine($"✓ Evento creado/actualizado exitosamente. Filas afectadas: {rowsAffected}");
