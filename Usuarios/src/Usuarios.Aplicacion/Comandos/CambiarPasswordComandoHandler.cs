using MediatR;
using Microsoft.Extensions.Logging;

namespace Usuarios.Aplicacion.Comandos;

public class CambiarPasswordComandoHandler : IRequestHandler<CambiarPasswordComando, bool>
{
    private readonly ILogger<CambiarPasswordComandoHandler> _logger;

    public CambiarPasswordComandoHandler(ILogger<CambiarPasswordComandoHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CambiarPasswordComando request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cambiando contraseña para usuario: {UsuarioId}", request.UsuarioId);
        // Lógica de cambio de contraseña aquí...
        return Task.FromResult(true);
    }
}
