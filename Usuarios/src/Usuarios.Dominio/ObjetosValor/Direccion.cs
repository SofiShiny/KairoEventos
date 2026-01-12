namespace Usuarios.Dominio.ObjetosValor
{
    public record Direccion
    {
        public string Valor { get; }
        
        private Direccion(string valor)
        {
            Valor = valor;
        }
        
        public static Direccion Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("La dirección no puede estar vacía");
            
            if (valor.Trim().Length < 5)
                throw new ArgumentException("La dirección debe tener al menos 5 caracteres");
            
            return new Direccion(valor.Trim());
        }
    }
}
