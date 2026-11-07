using System;
using CsCheck;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 11 del diseño para la transición de
/// publicación de comunicados.
///
/// Validates: Requirements 32.4, 32.5, 32.6
///
/// El sistema bajo prueba es
/// <see cref="ComunicadoService.Publicar(int)"/>. Según los Requisitos 32.4, 32.5 y
/// 32.6, publicar solo procede desde el estado <see cref="EstadoComunicado.Borrador"/>:
/// <list type="bullet">
///   <item>Si el comunicado está en borrador, <c>Publicar</c> tiene éxito y su
///   estado pasa a <see cref="EstadoComunicado.Publicado"/>.</item>
///   <item>Si el comunicado ya está publicado, <c>Publicar</c> lanza
///   <see cref="InvalidOperationException"/> y el estado permanece sin cambios
///   (sigue siendo <see cref="EstadoComunicado.Publicado"/>).</item>
/// </list>
///
/// Para cada iteración se genera un comunicado con estado inicial aleatorio
/// (borrador o publicado) junto a título, contenido y fecha aleatorios; se crea una
/// base SQLite en memoria recién inicializada, se persiste el comunicado, se
/// construye el servicio y se verifica el resultado esperado según el estado
/// inicial, releyendo el comunicado para comprobar su estado final.
/// </summary>
public class ComunicadoPublicarPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 11: Publicar solo procede desde el estado borrador
    [Fact]
    public void Publicar_SoloProcedeDesdeBorrador()
    {
        GenComunicado()
            .Sample(datos =>
            {
                using var fixture = new SqliteEnMemoria();

                // Sembrar el comunicado generado y capturar su Id.
                int id;
                using (var contextoSiembra = fixture.CrearContexto())
                {
                    var comunicado = new Comunicado
                    {
                        Titulo = datos.Titulo,
                        Fecha = datos.Fecha,
                        Contenido = datos.Contenido,
                        Estado = datos.Estado
                    };
                    contextoSiembra.Comunicados.Add(comunicado);
                    contextoSiembra.SaveChanges();
                    id = comunicado.Id;
                }

                // Construir el servicio sobre un contexto nuevo y ejecutar Publicar.
                using (var contextoServicio = fixture.CrearContexto())
                {
                    var servicio = new ComunicadoService(contextoServicio);

                    if (datos.Estado == EstadoComunicado.Borrador)
                    {
                        // Borrador: Publicar debe tener éxito (no lanza excepción).
                        servicio.Publicar(id).GetAwaiter().GetResult();
                    }
                    else
                    {
                        // Ya publicado: Publicar debe lanzar InvalidOperationException.
                        Assert.ThrowsAsync<InvalidOperationException>(() => servicio.Publicar(id))
                            .GetAwaiter().GetResult();
                    }
                }

                // Releer el comunicado en un contexto independiente para comprobar
                // su estado final.
                using var contextoLectura = fixture.CrearContexto();
                var relectura = contextoLectura.Comunicados.Find(id);

                if (relectura is null)
                {
                    return false;
                }

                // Desde borrador el estado final es Publicado; desde publicado el
                // estado permanece Publicado (sin cambios).
                return relectura.Estado == EstadoComunicado.Publicado;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera comunicados con estado inicial aleatorio (borrador o publicado) y
    /// título, contenido y fecha aleatorios.
    /// </summary>
    private static Gen<(string Titulo, string Contenido, DateTime Fecha, EstadoComunicado Estado)> GenComunicado() =>
        Gen.Select(
            Gen.String[1, 40],
            Gen.String[0, 80],
            GenFecha(),
            Gen.OneOf(
                Gen.Const(EstadoComunicado.Borrador),
                Gen.Const(EstadoComunicado.Publicado)),
            (titulo, contenido, fecha, estado) => (titulo, contenido, fecha, estado));

    /// <summary>
    /// Genera fechas dentro de un rango razonable (incluye el valor por defecto con
    /// frecuencia para ejercitar la rama que fija la fecha al publicar).
    /// </summary>
    private static Gen<DateTime> GenFecha() =>
        Gen.OneOf(
            Gen.Const(default(DateTime)),
            Gen.DateTime[new DateTime(2000, 1, 1), new DateTime(2030, 12, 31)]);
}
