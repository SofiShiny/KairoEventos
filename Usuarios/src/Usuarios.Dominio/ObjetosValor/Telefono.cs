namespace Usuarios.Dominio.ObjetosValor
{
    public record Telefono
    {
        public string Valor { get; }
        
        private Telefono(string valor)
        {
            Valor = valor;
        }
        
        public static Telefono Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El teléfono no puede estar vacío");
            
            // Remover espacios y caracteres especiales
            var telefonoLimpio = new string(valor.Where(char.IsDigit).ToArray());
            
            if (telefonoLimpio.Length < 7 || telefonoLimpio.Length > 15)
                throw new ArgumentException("El teléfono debe tener entre 7 y 15 dígitos");
            
            return new Telefono(telefonoLimpio);
        }
    }
}
