using System.Text.RegularExpressions;

namespace Usuarios.Domain.ObjetosValor;

public class Correo
{
    public string Value { get; private set; }
    private Regex _regex = new (@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public Correo(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException("El correo no puede ser vacio");
        }

        if (!_regex.IsMatch(value))
        {
            throw new InvalidOperationException("El correo no es valido");
        }
        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}