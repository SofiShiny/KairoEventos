namespace Usuarios.Dominio.ObjetosValor
{
    public record Correo
    {
        public string Valor { get; }
        
        private Correo(string valor)
        {
            Valor = valor;
        }
        
        public static Correo Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El correo no puede estar vacío");
            
            if (!EsCorreoValido(valor))
                throw new ArgumentException($"El correo '{valor}' no es válido");
            
            return new Correo(valor.ToLowerInvariant());
        }
        
        private static bool EsCorreoValido(string correo)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(correo);
                return addr.Address == correo;
            }
            catch
            {
                return false;
            }
        }
    }
}
