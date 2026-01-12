namespace Usuarios.Dominio.Excepciones
{
    public class UsernameDuplicadoException : Exception
    {
        public string Username { get; }
        
        public UsernameDuplicadoException(string username)
            : base($"Ya existe un usuario con el username: {username}")
        {
            Username = username;
        }
    }
}
