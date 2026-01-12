namespace BloquesConstruccion.Dominio;

public abstract class Entidad
{
 public Guid Id { get; protected set; } = Guid.NewGuid();

 public override bool Equals(object? obj)
 {
 if (ReferenceEquals(this, obj)) return true;
 if (obj is not Entidad other) return false;
 if (Id == Guid.Empty || other.Id == Guid.Empty) return false;
 return Id == other.Id;
 }

 public override int GetHashCode() => Id.GetHashCode();
}
