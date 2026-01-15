using Npgsql;

var connectionString = "Host=localhost;Port=5432;Database=EventsDB;Username=postgres;Password=postgres";
var eventoId = "6e92cc27-e4df-4602-8819-1756a038a7ce";

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

// Verificar si la columna existe
var checkColumnSql = @"
    SELECT column_name 
    FROM information_schema.columns 
    WHERE table_name = 'Eventos' AND column_name = 'PrecioBase'";

using var checkCmd = new NpgsqlCommand(checkColumnSql, connection);
var columnExists = await checkCmd.ExecuteScalarAsync();

if (columnExists != null)
{
    Console.WriteLine($"✅ La columna PrecioBase EXISTE");
    
    // Consultar el evento
    var sql = "SELECT \"Id\", \"Titulo\", \"PrecioBase\" FROM \"Eventos\" WHERE \"Id\" = @id";
    using var cmd = new NpgsqlCommand(sql, connection);
    cmd.Parameters.AddWithValue("id", Guid.Parse(eventoId));
    
    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        Console.WriteLine($"\nEvento encontrado:");
        Console.WriteLine($"  ID: {reader.GetGuid(0)}");
        Console.WriteLine($"  Título: {reader.GetString(1)}");
        Console.WriteLine($"  PrecioBase: {reader.GetDecimal(2)}");
    }
}
else
{
    Console.WriteLine("❌ La columna PrecioBase NO EXISTE en la tabla Eventos");
}
