using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=EventsDB;Username=postgres;Password=postgres";
var eventoId = "6e92cc27-e4df-4602-8819-1756a038a7ce";
var precioBase = 50.00m;

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

var sql = "UPDATE \"Eventos\" SET \"PrecioBase\" = @precio WHERE \"Id\" = @id";
using var command = new NpgsqlCommand(sql, connection);
command.Parameters.AddWithValue("precio", precioBase);
command.Parameters.AddWithValue("id", Guid.Parse(eventoId));

var rowsAffected = await command.ExecuteNonQueryAsync();
Console.WriteLine($"✅ Precio actualizado. Filas afectadas: {rowsAffected}");

// Verificar
var verifySql = "SELECT \"Titulo\", \"PrecioBase\" FROM \"Eventos\" WHERE \"Id\" = @id";
using var verifyCommand = new NpgsqlCommand(verifySql, connection);
verifyCommand.Parameters.AddWithValue("id", Guid.Parse(eventoId));

using var reader = await verifyCommand.ExecuteReaderAsync();
if (await reader.ReadAsync())
{
    Console.WriteLine($"Evento: {reader.GetString(0)}");
    Console.WriteLine($"Precio Base: {reader.GetDecimal(1)}");
}
