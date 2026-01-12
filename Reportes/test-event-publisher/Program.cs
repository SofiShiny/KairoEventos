using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Eventos.Dominio.EventosDeDominio;
using Asientos.Dominio.EventosDominio;

Console.WriteLine("=== Test Event Publisher para Microservicio de Reportes ===");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

// Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var host = builder.Build();
await host.StartAsync();

var publishEndpoint = host.Services.GetRequiredService<IPublishEndpoint>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    Console.WriteLine("Conectado a RabbitMQ. Publicando eventos de prueba...");
    Console.WriteLine();

    // Escenario 1: Crear un evento y publicarlo
    var eventoId = Guid.NewGuid();
    var mapaId = Guid.NewGuid();
    
    Console.WriteLine("📅 Escenario 1: Publicar evento nuevo");
    var eventoPublicado = new EventoPublicadoEventoDominio
    {
        EventoId = eventoId,
        TituloEvento = "Concierto de Rock 2024",
        FechaInicio = DateTime.UtcNow.AddDays(30),
        FechaFin = DateTime.UtcNow.AddDays(30).AddHours(4),
        Descripcion = "Gran concierto de rock con bandas internacionales",
        Ubicacion = "Estadio Nacional",
        CapacidadMaxima = 5000,
        OrganizadorId = "org-123"
    };
    
    await publishEndpoint.Publish(eventoPublicado);
    logger.LogInformation("✓ Publicado: EventoPublicadoEventoDominio - {EventoId}", eventoId);
    Console.WriteLine($"  ✓ EventoPublicado: {eventoPublicado.TituloEvento}");
    await Task.Delay(1000);

    // Escenario 2: Crear mapa de asientos
    Console.WriteLine();
    Console.WriteLine("🪑 Escenario 2: Crear mapa de asientos");
    var mapaCreado = new MapaAsientosCreadoEventoDominio
    {
        MapaId = mapaId,
        EventoId = eventoId,
        CapacidadTotal = 5000,
        FechaCreacion = DateTime.UtcNow
    };
    
    await publishEndpoint.Publish(mapaCreado);
    logger.LogInformation("✓ Publicado: MapaAsientosCreadoEventoDominio - {MapaId}", mapaId);
    Console.WriteLine($"  ✓ MapaCreado: Capacidad {mapaCreado.CapacidadTotal} asientos");
    await Task.Delay(1000);

    // Escenario 3: Registrar asistentes
    Console.WriteLine();
    Console.WriteLine("👥 Escenario 3: Registrar asistentes");
    var asistentes = new[]
    {
        new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = "user-001",
            NombreUsuario = "Juan Pérez",
            Email = "juan.perez@example.com",
            FechaRegistro = DateTime.UtcNow
        },
        new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = "user-002",
            NombreUsuario = "María García",
            Email = "maria.garcia@example.com",
            FechaRegistro = DateTime.UtcNow
        },
        new AsistenteRegistradoEventoDominio
        {
            EventoId = eventoId,
            UsuarioId = "user-003",
            NombreUsuario = "Carlos López",
            Email = "carlos.lopez@example.com",
            FechaRegistro = DateTime.UtcNow
        }
    };

    foreach (var asistente in asistentes)
    {
        await publishEndpoint.Publish(asistente);
        logger.LogInformation("✓ Publicado: AsistenteRegistradoEventoDominio - {Usuario}", asistente.NombreUsuario);
        Console.WriteLine($"  ✓ Asistente registrado: {asistente.NombreUsuario}");
        await Task.Delay(500);
    }

    // Escenario 4: Reservar asientos
    Console.WriteLine();
    Console.WriteLine("🎫 Escenario 4: Reservar asientos");
    var reservas = new[]
    {
        new AsientoReservadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId,
            Fila = 1,
            Numero = 10,
            UsuarioId = "user-001",
            CategoriaAsiento = "VIP",
            Precio = 150.00m,
            FechaReserva = DateTime.UtcNow
        },
        new AsientoReservadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId,
            Fila = 2,
            Numero = 15,
            UsuarioId = "user-002",
            CategoriaAsiento = "General",
            Precio = 75.00m,
            FechaReserva = DateTime.UtcNow
        },
        new AsientoReservadoEventoDominio
        {
            MapaId = mapaId,
            EventoId = eventoId,
            Fila = 3,
            Numero = 20,
            UsuarioId = "user-003",
            CategoriaAsiento = "General",
            Precio = 75.00m,
            FechaReserva = DateTime.UtcNow
        }
    };

    foreach (var reserva in reservas)
    {
        await publishEndpoint.Publish(reserva);
        logger.LogInformation("✓ Publicado: AsientoReservadoEventoDominio - Fila {Fila}, Número {Numero}", 
            reserva.Fila, reserva.Numero);
        Console.WriteLine($"  ✓ Asiento reservado: Fila {reserva.Fila}, Número {reserva.Numero} ({reserva.CategoriaAsiento})");
        await Task.Delay(500);
    }

    // Escenario 5: Liberar un asiento
    Console.WriteLine();
    Console.WriteLine("🔓 Escenario 5: Liberar asiento");
    var asientoLiberado = new AsientoLiberadoEventoDominio
    {
        MapaId = mapaId,
        EventoId = eventoId,
        Fila = 3,
        Numero = 20,
        FechaLiberacion = DateTime.UtcNow
    };
    
    await publishEndpoint.Publish(asientoLiberado);
    logger.LogInformation("✓ Publicado: AsientoLiberadoEventoDominio - Fila {Fila}, Número {Numero}", 
        asientoLiberado.Fila, asientoLiberado.Numero);
    Console.WriteLine($"  ✓ Asiento liberado: Fila {asientoLiberado.Fila}, Número {asientoLiberado.Numero}");
    await Task.Delay(1000);

    // Escenario 6: Publicar más eventos para testing
    Console.WriteLine();
    Console.WriteLine("📅 Escenario 6: Publicar eventos adicionales");
    
    var evento2Id = Guid.NewGuid();
    var eventoPublicado2 = new EventoPublicadoEventoDominio
    {
        EventoId = evento2Id,
        TituloEvento = "Festival de Jazz",
        FechaInicio = DateTime.UtcNow.AddDays(45),
        FechaFin = DateTime.UtcNow.AddDays(45).AddHours(6),
        Descripcion = "Festival de jazz con artistas internacionales",
        Ubicacion = "Teatro Municipal",
        CapacidadMaxima = 2000,
        OrganizadorId = "org-456"
    };
    
    await publishEndpoint.Publish(eventoPublicado2);
    Console.WriteLine($"  ✓ EventoPublicado: {eventoPublicado2.TituloEvento}");
    await Task.Delay(1000);

    Console.WriteLine();
    Console.WriteLine("=== Todos los eventos publicados exitosamente ===");
    Console.WriteLine();
    Console.WriteLine("Esperando 5 segundos para que los consumidores procesen...");
    await Task.Delay(5000);
    
    Console.WriteLine();
    Console.WriteLine("✅ Publicación completada. Verifica los datos con:");
    Console.WriteLine();
    Console.WriteLine("  MongoDB:");
    Console.WriteLine("    docker exec reportes-mongodb mongosh reportes_db --eval 'db.metricas_evento.find().pretty()'");
    Console.WriteLine("    docker exec reportes-mongodb mongosh reportes_db --eval 'db.historial_asistencia.find().pretty()'");
    Console.WriteLine("    docker exec reportes-mongodb mongosh reportes_db --eval 'db.reportes_ventas_diarias.find().pretty()'");
    Console.WriteLine("    docker exec reportes-mongodb mongosh reportes_db --eval 'db.logs_auditoria.find().pretty()'");
    Console.WriteLine();
    Console.WriteLine("  API Endpoints:");
    Console.WriteLine("    curl http://localhost:5002/api/reportes/resumen-ventas");
    Console.WriteLine($"    curl http://localhost:5002/api/reportes/asistencia/{eventoId}");
    Console.WriteLine("    curl http://localhost:5002/api/reportes/auditoria");
    Console.WriteLine();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error publicando eventos");
    Console.WriteLine($"❌ Error: {ex.Message}");
    return 1;
}
finally
{
    await host.StopAsync();
}

return 0;
