namespace Usuarios.Application.Exceptions;

public class AutenticacionException: Exception
{
    public AutenticacionException(string message) : base(message)
    {
    }
}