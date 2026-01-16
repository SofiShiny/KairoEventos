using Microsoft.EntityFrameworkCore;
using Encuestas.Infraestructura.Persistencia;
using Encuestas.Dominio.Entidades;

var optionsBuilder = new DbContextOptionsBuilder<EncuestasDbContext>();
optionsBuilder.UseNpgsql("Host=localhost;Database=kairo_encuestas;Username=postgres;Password=postgres");

using var context = new EncuestasDbContext(optionsBuilder.Options);

var encuestas = await context.Encuestas.ToListAsync();
Console.WriteLine($"Total Encuestas: {encuestas.Count}");
foreach (var e in encuestas)
{
    Console.WriteLine($"ID: {e.Id} | EventoID: {e.EventoId} | Titulo: {e.Titulo}");
}

var respuestas = await context.RespuestasUsuarios.ToListAsync();
Console.WriteLine($"\nTotal Respuestas: {respuestas.Count}");
foreach (var r in respuestas)
{
    Console.WriteLine($"ID: {r.Id} | EncuestaID: {r.EncuestaId} | UsuarioID: {r.UsuarioId}");
}
