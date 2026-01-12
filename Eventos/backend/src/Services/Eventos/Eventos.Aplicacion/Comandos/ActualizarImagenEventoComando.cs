using MediatR;
using System;
using System.IO;
using BloquesConstruccion.Aplicacion.Comun;

namespace Eventos.Aplicacion.Comandos;

public record ActualizarImagenEventoComando(Guid EventoId, Stream Archivo, string NombreArchivo) : IRequest<Resultado<string>>;
