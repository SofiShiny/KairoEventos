using Npgsql;

Console.WriteLine("🔍 Verificando columnas de la tabla Eventos...\n");

var connectionString = "Host=localhost;Port=5432;Database=EventsDB;Username=postgres;Password=postgres";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    
    var sql = @"
        SELECT column_name, data_type, is_nullable 
        FROM information_schema.columns 
        WHERE table_name = 'Eventos' 
        ORDER BY ordinal_position";
    
    using var command = new NpgsqlCommand(sql, connection);
    using var reader = await command.ExecuteReaderAsync();
    
    Console.WriteLine("Columnas en la tabla Eventos:");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    
    bool precioBaseExists = false;
    while (await reader.ReadAsync())
    {
        var columnName = reader.GetString(0);
        var dataType = reader.GetString(1);
        var nullable = reader.GetString(2);
        
        if (columnName.ToLower() == "preciobase")
        {
            precioBaseExists = true;
            Console.WriteLine($"✅ {columnName,-30} {dataType,-15} {nullable}");
        }
        else
        {
            Console.WriteLine($"   {columnName,-30} {dataType,-15} {nullable}");
        }
    }
    
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    
    if (precioBaseExists)
    {
        Console.WriteLine("✅ La columna PrecioBase EXISTE en la base de datos");
    }
    else
    {
        Console.WriteLine("❌ La columna PrecioBase NO EXISTE en la base de datos");
        Console.WriteLine("   Necesitas aplicar la migración: dotnet ef database update");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    return 1;
}

return 0;
