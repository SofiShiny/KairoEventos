using System;
using System.Collections.Generic;
using System.Linq;

namespace BloquesConstruccion.Dominio
{
    public abstract class ObjetoValor : IEquatable<ObjetoValor>
    {
        protected abstract IEnumerable<object> ObtenerComponentesDeIgualdad();

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            var otro = (ObjetoValor)obj;
            return ObtenerComponentesDeIgualdad().SequenceEqual(otro.ObtenerComponentesDeIgualdad());
        }

        public override int GetHashCode()
        {
            return ObtenerComponentesDeIgualdad()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(ObjetoValor? izquierda, ObjetoValor? derecha)
        {
            if (ReferenceEquals(izquierda, derecha))
                return true;

            if (izquierda is null || derecha is null)
                return false;

            return izquierda.Equals(derecha);
        }

        public static bool operator !=(ObjetoValor? izquierda, ObjetoValor? derecha) => !(izquierda == derecha);

        public bool Equals(ObjetoValor? otro) => Equals((object?)otro);
    }
}
