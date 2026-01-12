using System;
using Eventos.Dominio.ObjetosDeValor;
using Eventos.Dominio.Entidades;

namespace Eventos.Pruebas.Shared;

public static class TestHelpers
{
 public static readonly DateTime Now = DateTime.UtcNow;
 public static DateTime Future(int days=2) => Now.AddDays(days);
 public static Ubicacion BuildUbicacion(
 string nombre="Lugar",
 string direccion="Dir",
 string ciudad="Ciudad",
 string region="Region",
 string codigoPostal="0000",
 string pais="Pais") => new(nombre,direccion,ciudad,region,codigoPostal,pais);

 public static Evento BuildEvento(
 string titulo="Titulo",
 string descripcion="Desc",
 int maximo=10,
 string organizador="org-1",
 int startOffsetDays=5,
 int durationHours=2)
 {
 var inicio = Future(startOffsetDays);
 return new Evento(titulo, descripcion, BuildUbicacion(), inicio, inicio.AddHours(durationHours), maximo, organizador);
 }
}
