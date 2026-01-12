namespace BloquesConstruccion.Dominio;

public abstract class ObjetoValor
{
 protected abstract IEnumerable<object> ObtenerComponentesDeIgualdad();
 public override bool Equals(object? obj)
 {
 if (obj is not ObjetoValor other) return false;
 return ObtenerComponentesDeIgualdad().SequenceEqual(other.ObtenerComponentesDeIgualdad());
 }
 public override int GetHashCode()
 {
 return ObtenerComponentesDeIgualdad()
 .Aggregate(1, (acc, v) => HashCode.Combine(acc, v?.GetHashCode() ??0));
 }
}
