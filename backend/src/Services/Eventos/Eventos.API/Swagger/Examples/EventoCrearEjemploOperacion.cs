using Eventos.Aplicacion.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace Eventos.API.Swagger.Examples;

public class EventoCrearEjemploOperacion : IExamplesProvider<EventoCreateDto>
{
 public EventoCreateDto GetExamples()
 {
 return new EventoCreateDto
 {
 Titulo = "Conferencia Tecnologia2025",
 Descripcion = "Evento para discutir las ultimas tendencias en IA, Cloud y DevOps.",
 Ubicacion = new UbicacionDto
 {
 NombreLugar = "Centro de Convenciones Metropolitano",
 Direccion = "Av. Principal123",
 Ciudad = "Ciudad Ejemplo",
 Region = "Estado Ejemplo",
 CodigoPostal = "12345",
 Pais = "VE"
 },
 FechaInicio = DateTime.UtcNow.AddMonths(6).Date.AddHours(9),
 FechaFin = DateTime.UtcNow.AddMonths(6).Date.AddDays(1).AddHours(18),
 MaximoAsistentes =150
 };
 }
}
