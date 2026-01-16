using Servicios.Dominio.Entidades;
using Servicios.Infraestructura.Persistencia;

namespace Servicios.API;

public static class DbInitializer
{
    public static void Initialize(ServiciosDbContext context)
    {
        // Asegurarse de que la BD existe
        context.Database.EnsureCreated();

        // Buscar si ya hay servicios
        if (context.ServiciosGlobales.Any())
        {
            return;   // DB ya tiene datos
        }

        var servicios = new ServicioGlobal[]
        {
            new ServicioGlobal(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Catering VIP", 25.00m),
            new ServicioGlobal(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Transporte Premium", 15.00m),
            new ServicioGlobal(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Merchandising Pack", 30.00m),
            new ServicioGlobal(Guid.NewGuid(), "Acceso Backstage", 50.00m)
        };

        context.ServiciosGlobales.AddRange(servicios);
        context.SaveChanges();
    }
}
