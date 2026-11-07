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
/// Prueba basada en propiedades de la Property 5 del diseño para el servicio de
/// autoridades.
///
/// Validates: Requirements 4.2, 25.3
///
/// El sistema bajo prueba es
/// <see cref="AutoridadService.Listar"/>, que es la fuente de verdad del orden de
/// las autoridades: según el Requisito 25.3 (y su reflejo público 4.2), el listado
/// siempre se devuelve ordenado de forma ascendente por el campo
/// <see cref="Autoridad.Orden"/>.
///
/// Para cada iteración se genera un conjunto de autoridades con valores de
/// <c>Orden</c> variados, incluyendo valores repetidos, se persisten en una base
/// SQLite en memoria recién creada, se invoca <c>Listar()</c> y se verifica que el
/// resultado está ordenado ascendentemente por <c>Orden</c> y que contiene
/// exactamente las autoridades insertadas (sin pérdidas ni duplicaciones).
/// </summary>
public class AutoridadOrdenPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 5: Las autoridades se muestran en el orden definido
    [Fact]
    public void Listar_DevuelveAutoridadesOrdenadasAscendentementePorOrden()
    {
        GenAutoridades()
            .Sample(autoridades =>
            {
                using var fixture = new SqliteEnMemoria();

                // Sembrar las autoridades generadas en un contexto independiente.
                using (var contextoSiembra = fixture.CrearContexto())
                {
                    foreach (var autoridad in autoridades)
                    {
                        contextoSiembra.Autoridades.Add(new Autoridad
                        {
                            Nombre = autoridad.Nombre,
                            Cargo = autoridad.Cargo,
                            TipoAutoridad = autoridad.TipoAutoridad,
                            Orden = autoridad.Orden
                        });
                    }

                    contextoSiembra.SaveChanges();
                }

                // Construir el servicio sobre un contexto nuevo y listar.
                using var contextoLectura = fixture.CrearContexto();
                var servicio = new AutoridadService(contextoLectura);
                var resultado = servicio.Listar().GetAwaiter().GetResult();

                // 1) Orden ascendente: cada par adyacente cumple result[i] <= result[i+1].
                for (int i = 0; i + 1 < resultado.Count; i++)
                {
                    if (resultado[i].Orden > resultado[i + 1].Orden)
                    {
                        return false;
                    }
                }

                // 2) Conjunto exacto: el multiconjunto de valores de Orden del
                //    resultado coincide con el de las autoridades insertadas
                //    (sin pérdidas ni duplicaciones).
                var ordenesEsperados = autoridades
                    .Select(a => a.Orden)
                    .OrderBy(o => o)
                    .ToList();
                var ordenesResultado = resultado
                    .Select(a => a.Orden)
                    .OrderBy(o => o)
                    .ToList();

                return ordenesEsperados.SequenceEqual(ordenesResultado);
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera conjuntos de autoridades (0 a 20) con valores de <c>Orden</c> en un
    /// rango estrecho para forzar con frecuencia valores repetidos.
    /// </summary>
    private static Gen<List<Autoridad>> GenAutoridades() =>
        GenAutoridad().List[0, 20].Select(cs => cs.ToList());

    private static Gen<Autoridad> GenAutoridad() =>
        Gen.Select(
            Gen.String[1, 20],
            Gen.String[0, 20],
            Gen.OneOf(Gen.Const("autoridad"), Gen.Const("cargo"), Gen.Const("comité")),
            // Rango estrecho de Orden (incluye negativos) para provocar repeticiones.
            Gen.Int[-5, 10],
            (nombre, cargo, tipo, orden) => new Autoridad
            {
                Nombre = nombre,
                Cargo = cargo,
                TipoAutoridad = tipo,
                Orden = orden
            });
}
