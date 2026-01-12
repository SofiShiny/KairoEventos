namespace Usuarios.Dominio.Excepciones
{
    public class UsuarioNoEncontradoException : Exception
    {
        public Guid UsuarioId { get; }
        
        public UsuarioNoEncontradoException(Guid usuarioId)
            : base($"No se encontró el usuario con ID: {usuarioId}")
        {
            UsuarioId = usuarioId;
        }
    }
}
