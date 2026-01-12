using MassTransit;
using Microsoft.Extensions.Logging;
using Reportes.Aplicacion.Eventos;
using Pagos.Aplicacion.Eventos;
using Reportes.Dominio.ModelosLectura;
using Reportes.Infraestructura.Persistencia;
using System.Threading.Tasks;

namespace Reportes.Aplicacion.Consumers
{
    public class EntradaCompradaConsumer : IConsumer<EntradaCompradaEvento>
    {
        private readonly ReportesMongoDbContext _context;
        private readonly ILogger<EntradaCompradaConsumer> _logger;

        public EntradaCompradaConsumer(ReportesMongoDbContext context, ILogger<EntradaCompradaConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EntradaCompradaEvento> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Registrando historial: Entrada comprada por {UsuarioId}", msg.UsuarioId);

            var elemento = new ElementoHistorial
            {
                Id = Guid.NewGuid(),
                UsuarioId = msg.UsuarioId,
                Tipo = "Entrada",
                Descripcion = $"Compra de entrada para evento {msg.EventoId} (Categoria: {msg.Categoria})",
                Fecha = DateTime.UtcNow,
                Metadata = new { msg.EntradaId, msg.EventoId, msg.Categoria }
            };

            await _context.Timeline.InsertOneAsync(elemento);
        }
    }

    public class PagoAprobadoConsumer : IConsumer<PagoAprobadoEvento>
    {
        private readonly ReportesMongoDbContext _context;
        private readonly ILogger<PagoAprobadoConsumer> _logger;

        public PagoAprobadoConsumer(ReportesMongoDbContext context, ILogger<PagoAprobadoConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PagoAprobadoEvento> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Registrando historial: Pago aprobado para {UsuarioId}", msg.UsuarioId);

            var elemento = new ElementoHistorial
            {
                Id = Guid.NewGuid(),
                UsuarioId = msg.UsuarioId,
                Tipo = "Pago",
                Descripcion = $"Pago aprobado por monto {msg.Monto} (Ref: {msg.TransaccionId})",
                Fecha = DateTime.UtcNow,
                Metadata = new { msg.TransaccionId, msg.OrdenId, msg.Monto, msg.UrlFactura }
            };

            await _context.Timeline.InsertOneAsync(elemento);
        }
    }

    public class ServicioReservadoConsumer : IConsumer<ServicioReservadoEvento>
    {
        private readonly ReportesMongoDbContext _context;
        private readonly ILogger<ServicioReservadoConsumer> _logger;

        public ServicioReservadoConsumer(ReportesMongoDbContext context, ILogger<ServicioReservadoConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ServicioReservadoEvento> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Registrando historial: Servicio reservado por {UsuarioId}", msg.UsuarioId);

            var elemento = new ElementoHistorial
            {
                Id = Guid.NewGuid(),
                UsuarioId = msg.UsuarioId,
                Tipo = "Servicio",
                Descripcion = $"Reserva de servicio: {msg.NombreServicio} para evento {msg.EventoId}",
                Fecha = DateTime.UtcNow,
                Metadata = new { msg.ReservaId, msg.EventoId, msg.NombreServicio, msg.Precio }
            };

            await _context.Timeline.InsertOneAsync(elemento);
        }
    }
}
