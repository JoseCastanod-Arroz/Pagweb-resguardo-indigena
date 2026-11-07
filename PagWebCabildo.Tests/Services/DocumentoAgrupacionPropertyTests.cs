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
/// Prueba basada en propiedades de la Property 12 del diseño para el servicio de
/// documentos.
///
/// Validates: Requirements 13.1
///
/// El sistema bajo prueba es
/// <see cref="DocumentoService.ListarAgrupadosPorCategoria"/>, la única fuente de
/// verdad del agrupamiento de documentos: según el Requisito 13.1 cada documento
/// debe aparecer agrupado bajo su propia categoría, sin perderse ni duplicarse.
///
/// Para cada iteración se genera un conjunto de categorías, cada una con 0..n
/// documentos asignados a esa categoría. Se persisten en una base SQLite en
/// memoria recién creada, se construye el servicio sobre un contexto nuevo y se
/// invoca <c>ListarAgrupadosPorCategoria()</c>. Luego se verifica que:
/// 1) todo documento devuelto aparece bajo exactamente su propia categoría
///    (su <c>CategoriaDocumentoId</c> coincide con el <c>Id</c> de la categoría
///    que lo contiene), y
/// 2) el conjunto de documentos agrupados (por <c>Id</c>) coincide exactamente
///    con el conjunto de documentos persistidos (mismo número y mismos ids, sin
///    pérdidas ni duplicaciones).
/// </summary>
public class DocumentoAgrupacionPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 12: Cada documento se agrupa bajo su propia categoría
    [Fact]
    public void ListarAgrupadosPorCategoria_AgrupaCadaDocumentoBajoSuPropiaCategoria()
    {
        GenCategorias()
            .Sample(categorias =>
            {
                using var fixture = new SqliteEnMemoria();

                // Sembrar las categorías con sus documentos en un contexto
                // independiente. Cada documento se asocia a su categoría mediante
                // la navegación, de modo que EF asigna la clave foránea
                // CategoriaDocumentoId a la categoría a la que pertenece.
                using (var contextoSiembra = fixture.CrearContexto())
                {
                    foreach (var categoria in categorias)
                    {
                        var entidadCategoria = new CategoriaDocumento
                        {
                            Nombre = categoria.Nombre,
                            Documentos = categoria.Documentos
                                .Select(d => new Documento
                                {
                                    Titulo = d.Titulo,
                                    Descripcion = d.Descripcion,
                                    ArchivoRuta = d.ArchivoRuta
                                })
                                .ToList()
                        };

                        contextoSiembra.CategoriasDocumento.Add(entidadCategoria);
                    }

                    contextoSiembra.SaveChanges();
                }

                int totalDocumentosPersistidos;
                HashSet<int> idsDocumentosPersistidos;
                using (var contextoVerificacion = fixture.CrearContexto())
                {
                    idsDocumentosPersistidos = contextoVerificacion.Documentos
                        .Select(d => d.Id)
                        .ToHashSet();
                    totalDocumentosPersistidos = idsDocumentosPersistidos.Count;
                }

                // Construir el servicio sobre un contexto nuevo y agrupar.
                using var contextoLectura = fixture.CrearContexto();
                var servicio = new DocumentoService(contextoLectura);
                var resultado = servicio
                    .ListarAgrupadosPorCategoria()
                    .GetAwaiter()
                    .GetResult();

                // 1) Cada documento aparece bajo exactamente su propia categoría:
                //    para cada categoría devuelta, todos sus documentos apuntan a
                //    esa categoría por clave foránea.
                foreach (var categoria in resultado)
                {
                    foreach (var documento in categoria.Documentos)
                    {
                        if (documento.CategoriaDocumentoId != categoria.Id)
                        {
                            return false;
                        }
                    }
                }

                // 2) El conjunto de documentos agrupados coincide exactamente con
                //    el conjunto persistido (sin pérdidas ni duplicaciones). Se
                //    comparan ids: la unión debe tener el mismo tamaño total (sin
                //    duplicados entre categorías) y el mismo conjunto de ids.
                var idsAgrupados = resultado
                    .SelectMany(c => c.Documentos)
                    .Select(d => d.Id)
                    .ToList();

                // Sin duplicaciones: el número de ids agrupados (contando
                // repeticiones) es igual al número de ids distintos.
                if (idsAgrupados.Count != idsAgrupados.Distinct().Count())
                {
                    return false;
                }

                // Mismo número total que lo persistido.
                if (idsAgrupados.Count != totalDocumentosPersistidos)
                {
                    return false;
                }

                // Mismo conjunto de ids que lo persistido.
                return idsAgrupados.ToHashSet().SetEquals(idsDocumentosPersistidos);
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera conjuntos de categorías (0 a 8), cada una con 0 a 6 documentos, para
    /// cubrir categorías vacías, categorías con un único documento y categorías con
    /// varios documentos en la misma iteración.
    /// </summary>
    private static Gen<List<CategoriaDocumento>> GenCategorias() =>
        GenCategoria().List[0, 8].Select(cs => cs.ToList());

    private static Gen<CategoriaDocumento> GenCategoria() =>
        Gen.Select(
            Gen.String[1, 20],
            GenDocumento().List[0, 6],
            (nombre, documentos) => new CategoriaDocumento
            {
                Nombre = nombre,
                Documentos = documentos.ToList()
            });

    private static Gen<Documento> GenDocumento() =>
        Gen.Select(
            Gen.String[1, 20],
            Gen.String[0, 20],
            Gen.OneOf(Gen.Const("doc.pdf"), Gen.Const("acta.docx"), Gen.Const("plan.pdf")),
            (titulo, descripcion, ruta) => new Documento
            {
                Titulo = titulo,
                Descripcion = descripcion,
                ArchivoRuta = ruta
            });
}
