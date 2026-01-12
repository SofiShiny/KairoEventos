namespace BloquesConstruccion.Aplicacion.Comun;

public class Resultado
{
 public bool EsExitoso { get; }
 public string Error { get; }
 public bool EsFallido => !EsExitoso;

 // Alternate property expected by tests
 public bool IsExito => EsExitoso;

 protected Resultado(bool esExitoso, string error)
 {
 EsExitoso = esExitoso;
 Error = error;
 }

 public static Resultado Exito() => new Resultado(true, string.Empty);
 public static Resultado Falla(string error) => new Resultado(false, error);
 
 public static Resultado<T> Exito<T>(T valor) => new Resultado<T>(valor, true, string.Empty);
 public static Resultado<T> Falla<T>(string error) => new Resultado<T>(default!, false, error);
}

public class Resultado<T> : Resultado
{
 public T Valor { get; }

 // Alternate property expected by tests
 public T Value => Valor;

 protected internal Resultado(T valor, bool esExitoso, string error) 
 : base(esExitoso, error)
 {
 Valor = valor;
 }
 
 public static Resultado<T> Exito(T valor) => new Resultado<T>(valor, true, string.Empty);
 public static new Resultado<T> Falla(string error) => new Resultado<T>(default!, false, error);
}
