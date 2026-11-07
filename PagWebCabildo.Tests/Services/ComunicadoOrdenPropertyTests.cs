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
/// Prueba basada en propiedades de la Property 2 del diseño para el orden público
/// de los comunicados.
///
/// Validates: Requirements 8.2
///
/// El método bajo prueba es
/// <see cref="ComunicadoService.ListarPublicadosRecientesPrimero"/>. Según el
/// Requisito 8.2, la sección pública de comunicados muestra únicamente los
/// comunicados publicados ordenados del más reciente al más antiguo (por
/// <see cref="Comunicado.Fecha"/> descendente). Los borradores no deben aparecer
/// en ese listado.
///
/// La prueba genera en cada iteración un conjunto mixto de comunicados (algunos en
/// borrador y otros publicados) con fechas variadas que incluyen con frecuencia
/// valores repetidos, los persiste en una base SQLite en memoria recién creada,
/// invoca el servicio y verifica dos invariantes: (1) cada par adyacente del
/// resultado cumple <c>resultado[i].Fecha &gt;= resultado[i+1].Fecha</c> (más
/// reciente primero) y (2) todo elemento devuelto tiene
/// <see cref="EstadoComunicado.Publicado"/>.
/// </summary>
public class ComunicadoOrdenPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 2: Los comunicados publicados se ordenan del más reciente al más antiguo
    [Fact]
    public void ListarPublicadosRecientesPrimero_OrdenaDescendenteYSoloPublicados()
    {
        GenComunicados()
            .Sample(comunicados =>
            {
                using var bd = new SqliteEnMemoria();

                // Persistir el conjunto mixto generado.
                using (var ctx = bd.CrearContexto())
                {
                    foreach (var datos in comunicados)
                    {
                        ctx.Comunicados.Add(new Comunicado
                        {
                            Titulo = datos.Titulo,
                            Fecha = datos.Fecha,
                            Contenido = datos.Contenido,
                            Estado = datos.Estado
                        });
                    }

                    ctx.SaveChanges();
                }

                // Invocar el servicio sobre un contexto independiente.
                using var ctxLectura = bd.CrearContexto();
                var servicio = new ComunicadoService(ctxLectura);
                var resultado = servicio.ListarPublicadosRecientesPrimero().GetAwaiter().GetResult();

                // Invariante 2: todo elemento devuelto está publicado (los borradores
                // no aparecen).
                if (resultado.Any(c => c.Estado != EstadoComunicado.Publicado))
                {
                    return false;
                }

                // La cantidad devuelta debe coincidir con los publicados generados.
                int publicadosEsperados = comunicados.Count(c => c.Estado == EstadoComunicado.Publicado);
                if (resultado.Count != publicadosEsperados)
                {
                    return false;
                }

                // Invariante 1: orden descendente por fecha (más reciente primero),
                // tolerando fechas repetidas con >=.
                for (int i = 0; i + 1 < resultado.Count; i++)
                {
                    if (resultado[i].Fecha < resultado[i + 1].Fecha)
                    {
                        return false;
                    }
                }

                return true;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Datos de un comunicado a sembrar en la prueba.
    /// </summary>
    private readonly record struct DatosComunicado(string Titulo, DateTime Fecha, string Contenido, EstadoComunicado Estado);

    /// <summary>
    /// Genera una lista mixta de comunicados. Las fechas se extraen de un conjunto
    /// reducido de días para forzar con frecuencia valores repetidos, condición
    /// exigida por la tarea.
    /// </summary>
    private static Gen<DatosComunicado[]> GenComunicados()
    {
        // Base común para generar fechas; al variar solo el número de días sobre un
        // rango pequeño, muchos comunicados comparten la misma fecha.
        var baseFecha = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        Gen<DateTime> genFecha = Gen.Int[0, 9].Select(dias => baseFecha.AddDays(dias));

        Gen<EstadoComunicado> genEstado =
            Gen.Int[0, 1].Select(n => n == 0 ? EstadoComunicado.Borrador : EstadoComunicado.Publicado);

        Gen<DatosComunicado> genComunicado =
            Gen.Select(genFecha, genEstado, (fecha, estado) =>
                new DatosComunicado(
                    Titulo: "Comunicado",
                    Fecha: fecha,
                    Contenido: "Contenido del comunicado.",
                    Estado: estado));

        // Conjuntos de 0 a 12 comunicados para cubrir listas vacías, de un solo
        // elemento y con muchas colisiones de fecha.
        return genComunicado.Array[0, 12];
    }
}
