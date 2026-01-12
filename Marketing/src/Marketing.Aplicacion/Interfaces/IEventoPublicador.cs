namespace Marketing.Aplicacion.Interfaces;

public interface IEventoPublicador
{
    Task PublicarAsync<T>(T evento) where T : class;
}
