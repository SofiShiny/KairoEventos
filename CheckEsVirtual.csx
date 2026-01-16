
using System;
using Npgsql;

var connectionString = "Host=localhost;Database=kairo_eventos;Username=postgres;Password=postgres";
using var conn = new NpgsqlConnection(connectionString);
conn.Open();

using var cmd = new NpgsqlCommand("SELECT \"Id\", \"Titulo\", \"EsVirtual\" FROM \"Eventos\" ORDER BY \"Id\" DESC LIMIT 10", conn);
using var reader = cmd.ExecuteReader();

Console.WriteLine("ID | Titulo | EsVirtual");
Console.WriteLine("-----------------------");
while (reader.Read())
{
    Console.WriteLine($"{reader.GetGuid(0)} | {reader.GetString(1)} | {reader.GetBoolean(2)}");
}
