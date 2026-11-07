using System;
using System.Collections.Generic;
using System.Linq;
using CsCheck;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 3 del diseño: los borradores nunca
/// aparecen en las vistas públicas del sitio.
///
/// Validates: Requirements 8.1, 32.3
///
/// El servicio bajo prueba es <see cref="ComunicadoService"/>, a través de sus dos
/// rutas de acceso público:
/// <list type="bullet">
///   <item><see cref="ComunicadoService.ListarPublicadosRecientesPrimero"/>: el
///   listado público nunca debe contener un comunicado en estado
///   <see cref="EstadoComunicado.Borrador"/> y debe coincidir exactamente con el
///   subconjunto de comunicados <see cref="EstadoComunicado.Publicado"/>
///   (Requisito 8.1).</item>
///   <item><see cref="ComunicadoService.ObtenerPublicado(int)"/>: el acceso directo
///   por identificador devuelve <c>null</c> cuando el comunicado es un borrador y
///   devuelve el comunicado solo cuando está publicado (Requisito 32.3).</item>
/// </list>
///
/// Cada iteración genera un conjunto mixto de comunicados (mezcla de borradores y
/// publicados), crea una base SQLite en memoria nueva, persiste el conjunto,
/// construye un <see cref="ComunicadoService"/> sobre un contexto independiente y
/// verifica ambas rutas. La base se libera al final de cada iteración.
/// </summary>
public class ComunicadoBorradorOcultoPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 3: Los borradores nunca aparecen en vistas públicas
    [Fact]
    public void VistasPublicas_NuncaExponenBorradores()
    {
        GenConjuntoComunicados()
            .Sample(conjunto =>
            {
                using var baseDatos = new SqliteEnMemoria();

                // Persistir el conjunto mixto con un contexto independiente.
                List<(int Id, EstadoComunicado Estado)> persistidos;
                using (var contextoEscritura = baseDatos.CrearContexto())
                {
                    var entidades = conjunto.Select(c => new Comunicado
                    {
                        Titulo = c.Titulo,
                        Fecha = c.Fecha,
                        Contenido = c.Contenido,
                        Estado = c.Estado
                    }).ToList();

                    contextoEscritura.Comunicados.AddRange(entidades);
                    contextoEscritura.SaveChanges();

                    persistidos = entidades.Select(e => (e.Id, e.Estado)).ToList();
                }

                // Releer a través del servicio con un contexto distinto.
                using var contextoLectura = baseDatos.CrearContexto();
                var servicio = new ComunicadoService(contextoLectura);

                // 1) El listado público solo contiene publicados, y coincide
                //    exactamente con el subconjunto de publicados persistidos.
                var listados = servicio.ListarPublicadosRecientesPrimero()
                    .GetAwaiter().GetResult();

                bool ningunBorradorEnListado =
                    listados.All(c => c.Estado == EstadoComunicado.Publicado);

                var idsListados = listados.Select(c => c.Id).ToHashSet();
                var idsPublicadosEsperados = persistidos
                    .Where(p => p.Estado == EstadoComunicado.Publicado)
                    .Select(p => p.Id)
                    .ToHashSet();

                bool listadoCoincideConPublicados = idsListados.SetEquals(idsPublicadosEsperados);

                // 2) El acceso directo oculta los borradores y expone los publicados.
                bool accesoDirectoCorrecto = persistidos.All(p =>
                {
                    var obtenido = servicio.ObtenerPublicado(p.Id).GetAwaiter().GetResult();
                    return p.Estado == EstadoComunicado.Borrador
                        ? obtenido is null
                        : obtenido is not null && obtenido.Id == p.Id;
                });

                return ningunBorradorEnListado
                    && listadoCoincideConPublicados
                    && accesoDirectoCorrecto;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera un conjunto mixto de comunicados con una mezcla de estados borrador y
    /// publicado. El rango de tamaño incluye el conjunto vacío y conjuntos con
    /// varios elementos para ejercitar el filtrado.
    /// </summary>
    private static Gen<ComunicadoGenerado[]> GenConjuntoComunicados() =>
        GenComunicado().Array[0, 12];

    /// <summary>
    /// Genera un comunicado con estado aleatorio (mezcla equilibrada de borrador y
    /// publicado), título y contenido no vacíos y una fecha dentro de un rango
    /// amplio para que el orden por fecha no sea trivial.
    /// </summary>
    private static Gen<ComunicadoGenerado> GenComunicado() =>
        Gen.Select(
            Gen.String[1, 30],
            Gen.String[1, 60],
            GenFecha(),
            Gen.OneOf(Gen.Const(EstadoComunicado.Borrador), Gen.Const(EstadoComunicado.Publicado)),
            (titulo, contenido, fecha, estado) =>
                new ComunicadoGenerado(titulo, contenido, fecha, estado));

    /// <summary>
    /// Genera una fecha a partir de un número de días de desplazamiento respecto a
    /// una fecha base, cubriendo un intervalo de varios años.
    /// </summary>
    private static Gen<DateTime> GenFecha() =>
        Gen.Int[0, 3650].Select(dias => new DateTime(2015, 1, 1).AddDays(dias));

    /// <summary>Datos de un comunicado generado para una iteración de la prueba.</summary>
    private readonly record struct ComunicadoGenerado(
        string Titulo,
        string Contenido,
        DateTime Fecha,
        EstadoComunicado Estado);
}
