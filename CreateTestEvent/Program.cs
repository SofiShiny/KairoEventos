using System;
using Npgsql;

// Script para crear el evento de prueba con la estructura correcta
var connectionString = "Host=localhost;Database=EventsDB;Username=postgres;Password=postgres";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("✓ Conectado a la base de datos EventsDB\n");

    // Insertar el evento con las columnas correctas
    var sql = @"
    INSERT INTO ""Eventos"" (
        ""Id"", ""Titulo"", ""Descripcion"", ""FechaInicio"", ""FechaFin"", 
        ""MaximoAsistentes"", ""OrganizadorId"", ""Categoria"", ""Estado"", 
        ""CreadoEn"", ""ActualizadoEn"", ""UrlImagen"",
        ""Ubicacion_NombreLugar"", ""Ubicacion_Direccion"", ""Ubicacion_Ciudad"", 
        ""Ubicacion_Region"", ""Ubicacion_CodigoPostal"", ""Ubicacion_Pais""
    ) VALUES (
        @id, @titulo, @descripcion, @fechaInicio, @fechaFin,
        @maximoAsistentes, @organizadorId, @categoria, @estado,
        @creadoEn, @actualizadoEn, @urlImagen,
        @nombreLugar, @direccion, @ciudad, @region, @codigoPostal, @pais
    )
    ON CONFLICT (""Id"") DO UPDATE SET
        ""Estado"" = @estado,
        ""Titulo"" = @titulo,
        ""ActualizadoEn"" = @actualizadoEn;
    ";

    using var command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("id", Guid.Parse("6e92cc27-e4df-4602-8819-1756a038a7ce"));
    command.Parameters.AddWithValue("titulo", "Concierto de Rock 2026");
    command.Parameters.AddWithValue("descripcion", "Gran concierto de rock en vivo con las mejores bandas");
    command.Parameters.AddWithValue("fechaInicio", DateTime.SpecifyKind(new DateTime(2026, 2, 15, 20, 0, 0), DateTimeKind.Utc));
    command.Parameters.AddWithValue("fechaFin", DateTime.SpecifyKind(new DateTime(2026, 2, 15, 23, 0, 0), DateTimeKind.Utc));
    command.Parameters.AddWithValue("maximoAsistentes", 1000);
    command.Parameters.AddWithValue("organizadorId", "org-001");
    command.Parameters.AddWithValue("categoria", "Musica");
    command.Parameters.AddWithValue("estado", "Publicado");
    command.Parameters.AddWithValue("creadoEn", DateTime.UtcNow);
    command.Parameters.AddWithValue("actualizadoEn", DateTime.UtcNow);
    command.Parameters.AddWithValue("urlImagen", DBNull.Value);
    command.Parameters.AddWithValue("nombreLugar", "Estadio Nacional");
    command.Parameters.AddWithValue("direccion", "Av. Grecia 2001");
    command.Parameters.AddWithValue("ciudad", "Santiago");
    command.Parameters.AddWithValue("region", "Metropolitana");
    command.Parameters.AddWithValue("codigoPostal", "7750000");
    command.Parameters.AddWithValue("pais", "Chile");

    var rowsAffected = command.ExecuteNonQuery();
    Console.WriteLine($"✓ Evento creado/actualizado exitosamente!");
    Console.WriteLine($"  Filas afectadas: {rowsAffected}");
    Console.WriteLine($"  ID del evento: 6e92cc27-e4df-4602-8819-1756a038a7ce");
    Console.WriteLine($"  Título: Concierto de Rock 2026");
    Console.WriteLine($"  Estado: Publicado");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Detalles: {ex.InnerException.Message}");
    }
    return 1;
}

return 0;
