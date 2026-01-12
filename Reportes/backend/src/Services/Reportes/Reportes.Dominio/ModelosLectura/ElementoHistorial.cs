using System;

namespace Reportes.Dominio.ModelosLectura
{
    public class ElementoHistorial
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Tipo { get; set; } // Entrada, Pago, Servicio, Seguridad
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public object Metadata { get; set; } // Datos extra JSON
    }
}
