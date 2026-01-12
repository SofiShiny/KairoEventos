using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text.Json;
using MassTransit;
using Usuarios.Dominio.Eventos;

namespace Usuarios.API.Filters;

public class AuditoriaAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        var metodo = request.Method;

        // Solo auditamos POST, PUT, DELETE
        if (metodo == "POST" || metodo == "PUT" || metodo == "DELETE")
        {
            var publishEndpoint = context.HttpContext.RequestServices.GetService<IPublishEndpoint>();
            
            if (publishEndpoint != null)
            {
                var usuarioIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid.TryParse(usuarioIdClaim, out Guid usuarioId);

                // Serializar datos de la petición (simplificado para el ejemplo)
                var datos = string.Empty;
                if (context.ActionArguments.Any())
                {
                    // Evitar serializar tipos que dan error como CancellationToken
                    var serializableArgs = context.ActionArguments
                        .Where(a => !(a.Value is CancellationToken))
                        .ToDictionary(a => a.Key, a => a.Value);

                    datos = JsonSerializer.Serialize(serializableArgs);
                }

                var evento = new UsuarioAccionRealizada(
                    usuarioId,
                    metodo,
                    request.Path,
                    datos,
                    DateTime.UtcNow
                );

                await publishEndpoint.Publish(evento);
            }
        }

        await next();
    }
}
