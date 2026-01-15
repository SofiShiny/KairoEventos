using Npgsql;

Console.WriteLine("🔧 Aplicando fix para permitir entradas gratuitas...\n");

var connectionString = "Host=localhost;Port=5432;Database=kairo_entradas;Username=postgres;Password=postgres";

try
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("✅ Conectado a la base de datos kairo_entradas");

    // Eliminar restricción antigua
    var dropSql = "ALTER TABLE entradas DROP CONSTRAINT IF EXISTS ck_entradas_monto_positivo";
    using var dropCmd = new NpgsqlCommand(dropSql, connection);
    await dropCmd.ExecuteNonQueryAsync();
    Console.WriteLine("✅ Restricción antigua eliminada");

    // Agregar nueva restricción
    var addSql = "ALTER TABLE entradas ADD CONSTRAINT ck_entradas_monto_no_negativo CHECK (monto >= 0)";
    using var addCmd = new NpgsqlCommand(addSql, connection);
    await addCmd.ExecuteNonQueryAsync();
    Console.WriteLine("✅ Nueva restricción agregada (monto >= 0)");

    // Verificar
    var verifySql = "SELECT conname, pg_get_constraintdef(oid) FROM pg_constraint WHERE conrelid = 'entradas'::regclass AND contype = 'c'";
    using var verifyCmd = new NpgsqlCommand(verifySql, connection);
    using var reader = await verifyCmd.ExecuteReaderAsync();
    
    Console.WriteLine("\n📋 Restricciones CHECK actuales:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"   - {reader.GetString(0)}: {reader.GetString(1)}");
    }
    
    Console.WriteLine("\n✅ Fix aplicado exitosamente!");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    return 1;
}

return 0;
