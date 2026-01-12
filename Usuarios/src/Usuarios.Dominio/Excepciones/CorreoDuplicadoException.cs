namespace Usuarios.Dominio.Excepciones
{
    public class CorreoDuplicadoException : Exception
    {
        public string Correo { get; }
        
        public CorreoDuplicadoException(string correo)
            : base($"Ya existe un usuario con el correo: {correo}")
        {
            Correo = correo;
        }
    }
}
