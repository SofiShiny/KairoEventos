using BloquesConstruccion.Dominio;

namespace Asientos.Dominio.ObjetosDeValor;

public class CategoriaAsiento : ObjetoValor
{
 public string Nombre { get; }
 public decimal? PrecioBase { get; }
 public bool TienePrioridad { get; } // true = prioridad especial

 private CategoriaAsiento() { }

	private CategoriaAsiento(string nombre, decimal? precioBase, bool tienePrioridad)
 {
  Nombre = nombre;
  PrecioBase = precioBase;
  TienePrioridad = tienePrioridad;
 }

 public static CategoriaAsiento Crear(string nombre, decimal? precioBase = null, bool tienePrioridad = false)
 {
  if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Nombre categoria requerido", nameof(nombre));
  return new CategoriaAsiento(nombre.Trim(), precioBase, tienePrioridad);
 }

 protected override IEnumerable<object> ObtenerComponentesDeIgualdad()
 {
  yield return Nombre.ToLowerInvariant();
 }
}

