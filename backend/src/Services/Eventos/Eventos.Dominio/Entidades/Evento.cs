using BloquesConstruccion.Dominio;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Enumeraciones;
using Eventos.Dominio.EventosDeDominio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventos.Dominio.Entidades;

public class Evento : RaizAgregada<Guid>
{
 public string Titulo { get; private set; } = string.Empty;
 public string Descripcion { get; private set; } = string.Empty;
 public Ubicacion Ubicacion { get; private set; } = null!;
 public DateTime FechaInicio { get; private set; }
 public DateTime FechaFin { get; private set; }
 public int MaximoAsistentes { get; private set; }
 public EstadoEvento Estado { get; private set; }
 public string OrganizadorId { get; private set; } = string.Empty;
 
 private readonly List<Asistente> _asistentes = new();
 public IReadOnlyCollection<Asistente> Asistentes => _asistentes.AsReadOnly();
 
 public int ConteoAsistentesActual => _asistentes.Count;
 public bool EstaCompleto => ConteoAsistentesActual >= MaximoAsistentes;
 public bool EstaPublicado => Estado == EstadoEvento.Publicado;
 public bool EstaCancelado => Estado == EstadoEvento.Cancelado;

 private Evento() { } // Para EF Core

 public Evento(
 string titulo,
 string descripcion,
 Ubicacion ubicacion,
 DateTime fechaInicio,
 DateTime fechaFin,
 int maximoAsistentes,
 string organizadorId)
 {
 if (string.IsNullOrWhiteSpace(titulo))
 throw new ArgumentException("El título del evento no puede estar vacío", nameof(titulo));
 
 if (string.IsNullOrWhiteSpace(descripcion))
 throw new ArgumentException("La descripción del evento no puede estar vacía", nameof(descripcion));
 
 if (ubicacion == null)
 throw new ArgumentNullException(nameof(ubicacion));
 
 if (fechaInicio <= DateTime.UtcNow)
 throw new ArgumentException("La fecha de inicio debe ser en el futuro", nameof(fechaInicio));
 
 if (fechaFin <= fechaInicio)
 throw new ArgumentException("La fecha de finalización debe ser posterior a la fecha de inicio", nameof(fechaFin));
 
 if (maximoAsistentes <=0)
 throw new ArgumentException("El número máximo de asistentes debe ser mayor que cero", nameof(maximoAsistentes));
 
 if (string.IsNullOrWhiteSpace(organizadorId))
 throw new ArgumentException("El identificador del organizador no puede estar vacío", nameof(organizadorId));

 Id = Guid.NewGuid();
 Titulo = titulo;
 Descripcion = descripcion;
 Ubicacion = ubicacion;
 FechaInicio = fechaInicio;
 FechaFin = fechaFin;
 MaximoAsistentes = maximoAsistentes;
 OrganizadorId = organizadorId;
 Estado = EstadoEvento.Borrador;
 }

 public void Publicar()
 {
 if (Estado != EstadoEvento.Borrador)
 throw new InvalidOperationException($"No se puede publicar el evento cuando está en estado {Estado}");

 Estado = EstadoEvento.Publicado;
 
 GenerarEventoDominio(new EventoPublicadoEventoDominio(Id, Titulo, FechaInicio));
 }

 public void Cancelar()
 {
 if (Estado == EstadoEvento.Cancelado)
 throw new InvalidOperationException("El evento ya está cancelado");
 
 if (Estado == EstadoEvento.Completado)
 throw new InvalidOperationException("No se puede cancelar un evento completado");

 Estado = EstadoEvento.Cancelado;
 
 GenerarEventoDominio(new EventoCanceladoEventoDominio(Id, Titulo));
 }

 public void RegistrarAsistente(string usuarioId, string nombreUsuario, string correo)
 {
 // Reordenado para permitir distinguir evento cancelado
 if (EstaCancelado)
 throw new InvalidOperationException("No se puede registrar en un evento cancelado");
 
 if (!EstaPublicado)
 throw new InvalidOperationException("No se puede registrar en un evento no publicado");
 
 if (EstaCompleto)
 throw new InvalidOperationException("El evento está completo");
 
 if (_asistentes.Any(a => a.UsuarioId == usuarioId))
 throw new InvalidOperationException("El usuario ya está registrado");

 var asistente = new Asistente(Id, usuarioId, nombreUsuario, correo);
 _asistentes.Add(asistente);
 
 GenerarEventoDominio(new AsistenteRegistradoEventoDominio(Id, usuarioId, nombreUsuario));
 }

 public void AnularRegistroAsistente(string usuarioId)
 {
 var asistente = _asistentes.FirstOrDefault(a => a.UsuarioId == usuarioId);
 if (asistente == null)
 throw new InvalidOperationException("El usuario no está registrado en este evento");

 _asistentes.Remove(asistente);
 }

 public void Actualizar(string titulo, string descripcion, Ubicacion ubicacion, DateTime fechaInicio, DateTime fechaFin, int maximoAsistentes)
 {
 if (Estado == EstadoEvento.Cancelado)
 throw new InvalidOperationException("No se puede actualizar un evento cancelado");
 
 if (Estado == EstadoEvento.Completado)
 throw new InvalidOperationException("No se puede actualizar un evento completado");

 if (string.IsNullOrWhiteSpace(titulo))
 throw new ArgumentException("El título del evento no puede estar vacío", nameof(titulo));
 
 if (maximoAsistentes < ConteoAsistentesActual)
 throw new ArgumentException($"No se puede reducir el número máximo de asistentes por debajo del recuento actual ({ConteoAsistentesActual})", nameof(maximoAsistentes));

 Titulo = titulo;
 Descripcion = descripcion;
 Ubicacion = ubicacion;
 FechaInicio = fechaInicio;
 FechaFin = fechaFin;
 MaximoAsistentes = maximoAsistentes;
 }
}
