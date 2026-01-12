using System;

namespace BloquesConstruccion.Dominio;

public abstract class Entidad<TId> where TId : struct
{
 public TId Id { get; protected set; }

 protected Entidad()
 {
 }

 protected Entidad(TId id)
 {
 Id = id;
 }

 public override bool Equals(object? obj)
 {
 if (obj is not Entidad<TId> other)
 return false;

 return Id.Equals(other.Id);
 }

 public override int GetHashCode()
 {
 return Id.GetHashCode();
 }
}

public abstract class Entidad
{
 public Guid Id { get; protected set; } = Guid.NewGuid();

 public override bool Equals(object? obj)
 {
 if (obj is not Entidad other)
 return false;

 return Id == other.Id;
 }

 public override int GetHashCode()
 {
 return Id.GetHashCode();
 }
}
