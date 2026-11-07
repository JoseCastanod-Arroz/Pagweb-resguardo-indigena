using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CsCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 4 del diseño roles-cabildo-actas:
/// el rol Tesorero solo gestiona documentos financieros, es decir, documentos de
/// categorías marcadas con <see cref="CategoriaDocumento.EsFinanciero"/> igual a
/// <c>true</c>.
///
/// Feature: roles-cabildo-actas, Property 4: El Tesorero solo gestiona documentos
/// financieros.
///
/// Validates: Requirements 6.2, 6.3
///
/// La propiedad se expresa con dos aspectos complementarios, ambos ejercitados en
/// cada iteración con al menos <see cref="IteracionesMinimas"/> iteraciones:
///
/// <para>
/// <b>Aspecto A — invariante de filtrado a nivel de servicio</b>
/// (<see cref="DocumentoService"/>, fuente de verdad de "qué documentos son
/// financieros"). Para cada iteración se genera un conjunto aleatorio de
/// categorías, cada una marcada aleatoriamente como financiera o no y con
/// 0..n documentos; se persisten en una base SQLite en memoria recién creada y se
/// comprueba que:
/// </para>
/// <list type="number">
///   <item><c>ListarDocumentosFinancieros()</c> devuelve EXACTAMENTE el conjunto
///   de documentos cuya categoría es financiera (ni se pierde ninguno financiero
///   ni se cuela ninguno no financiero).</item>
///   <item><c>ListarCategoriasFinancieras()</c> devuelve exactamente las
///   categorías financieras.</item>
///   <item>para cada documento, <c>EsDocumentoFinanciero(id)</c> coincide con la
///   marca financiera de su categoría, y para cada categoría
///   <c>EsCategoriaFinanciera(id)</c> coincide con su marca.</item>
/// </list>
/// Esto fija de forma precisa el conjunto gestionable por el Tesorero y garantiza
/// que nunca se filtra un documento no financiero (Requisito 6.2).
///
/// <para>
/// <b>Aspecto B — invariante de autorización por política</b> (que el Tesorero
/// está habilitado por la política <c>GestionFinanciera</c> y no por las políticas
/// generales de contenido). Se construyen las políticas reales exactamente como en
/// <see cref="Program"/> y, para principales aleatorios, se comprueba que:
/// </para>
/// <list type="bullet">
///   <item>un principal <see cref="Rol.Tesorero"/> SÍ es autorizado por
///   <c>GestionFinanciera</c> (los documentos financieros son su dominio,
///   Requisito 6.2).</item>
///   <item>un principal <see cref="Rol.Tesorero"/> NO es autorizado por las
///   políticas generales que lo excluyen (<c>EliminarContenido</c>,
///   <c>GestionActas</c>), confirmando que el Tesorero no puede actuar fuera de
///   los documentos financieros (Requisito 6.3).</item>
///   <item>un principal <see cref="Rol.AdministradorGeneral"/> SÍ es autorizado
///   por <c>GestionFinanciera</c> (comprobación de cordura).</item>
/// </list>
///
/// En conjunto, ambos aspectos expresan "El Tesorero solo gestiona documentos
/// financieros": el conjunto de documentos financieros es exactamente el de
/// documentos con <c>EsFinanciero == true</c> (Aspecto A) y la autorización del
/// Tesorero se limita a la política financiera (Aspecto B).
/// </summary>
public class TesoreroDocumentosFinancierosPropertyTests
{
    private const int IteracionesMinimas = 100;

    private const string PoliticaFinanciera = "GestionFinanciera";
    private const string PoliticaEliminarContenido = "EliminarContenido";
    private const string PoliticaGestionActas = "GestionActas";

    // ---------------------------------------------------------------------
    // Aspecto A — invariante de filtrado a nivel de servicio
    // ---------------------------------------------------------------------

    // Feature: roles-cabildo-actas, Property 4: El Tesorero solo gestiona documentos financieros
    [Fact]
    public void DocumentosFinancieros_SonExactamenteLosDeCategoriasFinancieras()
    {
        GenCategorias()
            .Sample(categorias =>
            {
                using var fixture = new SqliteEnMemoria();

                // Sembrar categorías (financieras y no financieras) con sus
                // documentos en un contexto independiente.
                using (var contextoSiembra = fixture.CrearContexto())
                {
                    foreach (var categoria in categorias)
                    {
                        var entidadCategoria = new CategoriaDocumento
                        {
                            Nombre = categoria.Nombre,
                            EsFinanciero = categoria.EsFinanciero,
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

                // Conjunto esperado: ids de documentos cuya categoría es
                // financiera, e ids de categorías financieras, leídos directamente
                // de la base como fuente de verdad independiente del servicio.
                HashSet<int> idsDocumentosFinancierosEsperados;
                HashSet<int> idsCategoriasFinancierasEsperadas;
                List<(int DocId, bool EsFin)> documentosConMarca;
                List<(int CatId, bool EsFin)> categoriasConMarca;
                using (var contextoVerificacion = fixture.CrearContexto())
                {
                    idsCategoriasFinancierasEsperadas = contextoVerificacion.CategoriasDocumento
                        .Where(c => c.EsFinanciero)
                        .Select(c => c.Id)
                        .ToHashSet();

                    idsDocumentosFinancierosEsperados = contextoVerificacion.Documentos
                        .Where(d => d.CategoriaDocumento != null && d.CategoriaDocumento.EsFinanciero)
                        .Select(d => d.Id)
                        .ToHashSet();

                    documentosConMarca = contextoVerificacion.Documentos
                        .Select(d => new ValueTuple<int, bool>(
                            d.Id,
                            d.CategoriaDocumento != null && d.CategoriaDocumento.EsFinanciero))
                        .ToList();

                    categoriasConMarca = contextoVerificacion.CategoriasDocumento
                        .Select(c => new ValueTuple<int, bool>(c.Id, c.EsFinanciero))
                        .ToList();
                }

                using var contextoLectura = fixture.CrearContexto();
                var servicio = new DocumentoService(contextoLectura);

                // 1) ListarDocumentosFinancieros() devuelve exactamente el conjunto
                //    de documentos de categorías financieras: mismos ids, sin
                //    duplicados, sin pérdidas ni intrusos.
                var documentosFinancieros = servicio
                    .ListarDocumentosFinancieros()
                    .GetAwaiter()
                    .GetResult();

                var idsDocumentosDevueltos = documentosFinancieros.Select(d => d.Id).ToList();
                if (idsDocumentosDevueltos.Count != idsDocumentosDevueltos.Distinct().Count())
                {
                    return false;
                }
                if (!idsDocumentosDevueltos.ToHashSet().SetEquals(idsDocumentosFinancierosEsperados))
                {
                    return false;
                }

                // 2) ListarCategoriasFinancieras() devuelve exactamente las
                //    categorías financieras.
                var categoriasFinancieras = servicio
                    .ListarCategoriasFinancieras()
                    .GetAwaiter()
                    .GetResult();

                var idsCategoriasDevueltas = categoriasFinancieras.Select(c => c.Id).ToList();
                if (idsCategoriasDevueltas.Count != idsCategoriasDevueltas.Distinct().Count())
                {
                    return false;
                }
                if (!idsCategoriasDevueltas.ToHashSet().SetEquals(idsCategoriasFinancierasEsperadas))
                {
                    return false;
                }
                // Toda categoría devuelta debe estar efectivamente marcada como
                // financiera (ninguna no financiera se cuela).
                if (categoriasFinancieras.Any(c => !c.EsFinanciero))
                {
                    return false;
                }

                // 3) EsDocumentoFinanciero(id) coincide con la marca de la
                //    categoría del documento.
                foreach (var (docId, esFin) in documentosConMarca)
                {
                    var resultado = servicio
                        .EsDocumentoFinanciero(docId)
                        .GetAwaiter()
                        .GetResult();
                    if (resultado != esFin)
                    {
                        return false;
                    }
                }

                // 4) EsCategoriaFinanciera(id) coincide con la marca de la
                //    categoría.
                foreach (var (catId, esFin) in categoriasConMarca)
                {
                    var resultado = servicio
                        .EsCategoriaFinanciera(catId)
                        .GetAwaiter()
                        .GetResult();
                    if (resultado != esFin)
                    {
                        return false;
                    }
                }

                return true;
            }, iter: IteracionesMinimas);
    }

    // ---------------------------------------------------------------------
    // Aspecto B — invariante de autorización por política
    // ---------------------------------------------------------------------

    // Feature: roles-cabildo-actas, Property 4: El Tesorero solo gestiona documentos financieros
    [Fact]
    public void Tesorero_SoloAutorizadoPorPoliticaFinanciera()
    {
        // Construir las políticas reales exactamente como en Program.cs y resolver
        // los servicios de autorización una sola vez para toda la ejecución.
        var proveedor = new ServiceCollection()
            .AddLogging()
            .AddAuthorization(options =>
            {
                // GestionCuentas y EliminarContenido siguen exigiendo solo AdministradorGeneral.
                options.AddPolicy("GestionCuentas", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral)));
                options.AddPolicy("EliminarContenido", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral)));

                // GestionActas: AdministradorGeneral, Secretario.
                options.AddPolicy("GestionActas", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario)));
                // ConsultaActas: AdministradorGeneral, Secretario, Fiscal.
                options.AddPolicy("ConsultaActas", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario), nameof(Rol.Fiscal)));
                // GestionFinanciera: AdministradorGeneral, Tesorero.
                options.AddPolicy("GestionFinanciera", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Tesorero)));
                // GestionMensajes: AdministradorGeneral, Comisario.
                options.AddPolicy("GestionMensajes", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Comisario)));
                // Veeduria: AdministradorGeneral, Fiscal.
                options.AddPolicy("Veeduria", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Fiscal)));
                // GestionComunicadosEventos: AdministradorGeneral, Secretario, Comisario.
                options.AddPolicy("GestionComunicadosEventos", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario), nameof(Rol.Comisario)));
            })
            .BuildServiceProvider();

        var servicioAutorizacion = proveedor.GetRequiredService<IAuthorizationService>();
        var proveedorPoliticas = proveedor.GetRequiredService<IAuthorizationPolicyProvider>();

        // Confirmar que las políticas relevantes existen.
        Assert.NotNull(proveedorPoliticas.GetPolicyAsync(PoliticaFinanciera).GetAwaiter().GetResult());
        Assert.NotNull(proveedorPoliticas.GetPolicyAsync(PoliticaEliminarContenido).GetAwaiter().GetResult());
        Assert.NotNull(proveedorPoliticas.GetPolicyAsync(PoliticaGestionActas).GetAwaiter().GetResult());

        GenClaimsExtra()
            .Sample(claimsExtra =>
            {
                var tesorero = ConstruirPrincipal(nameof(Rol.Tesorero), claimsExtra);

                // El Tesorero SÍ es autorizado por la política financiera.
                var tesoreroEnFinanciera = servicioAutorizacion
                    .AuthorizeAsync(tesorero, resource: null, PoliticaFinanciera)
                    .GetAwaiter()
                    .GetResult();
                if (!tesoreroEnFinanciera.Succeeded)
                {
                    return false;
                }

                // El Tesorero NO es autorizado por políticas generales de contenido
                // que lo excluyen (no puede actuar fuera de los documentos
                // financieros a través de ellas).
                var tesoreroEnEliminar = servicioAutorizacion
                    .AuthorizeAsync(tesorero, resource: null, PoliticaEliminarContenido)
                    .GetAwaiter()
                    .GetResult();
                if (tesoreroEnEliminar.Succeeded)
                {
                    return false;
                }

                var tesoreroEnActas = servicioAutorizacion
                    .AuthorizeAsync(tesorero, resource: null, PoliticaGestionActas)
                    .GetAwaiter()
                    .GetResult();
                if (tesoreroEnActas.Succeeded)
                {
                    return false;
                }

                // Comprobación de cordura: el AdministradorGeneral SÍ es autorizado
                // por la política financiera.
                var administrador = ConstruirPrincipal(nameof(Rol.AdministradorGeneral), claimsExtra);
                var administradorEnFinanciera = servicioAutorizacion
                    .AuthorizeAsync(administrador, resource: null, PoliticaFinanciera)
                    .GetAwaiter()
                    .GetResult();

                return administradorEnFinanciera.Succeeded;
            }, iter: IteracionesMinimas);
    }

    // ---------------------------------------------------------------------
    // Generadores y utilidades
    // ---------------------------------------------------------------------

    /// <summary>
    /// Genera conjuntos de categorías (0 a 8), cada una marcada aleatoriamente como
    /// financiera o no y con 0 a 6 documentos, para cubrir en una misma ejecución
    /// categorías financieras vacías, financieras con documentos y no financieras
    /// con documentos (que nunca deben aparecer como financieros).
    /// </summary>
    private static Gen<List<CategoriaDocumento>> GenCategorias() =>
        GenCategoria().List[0, 8].Select(cs => cs.ToList());

    private static Gen<CategoriaDocumento> GenCategoria() =>
        Gen.Select(
            Gen.String[1, 20],
            Gen.Bool,
            GenDocumento().List[0, 6],
            (nombre, esFinanciero, documentos) => new CategoriaDocumento
            {
                Nombre = nombre,
                EsFinanciero = esFinanciero,
                Documentos = documentos.ToList()
            });

    private static Gen<Documento> GenDocumento() =>
        Gen.Select(
            Gen.String[1, 20],
            Gen.String[0, 20],
            Gen.OneOf(Gen.Const("balance.pdf"), Gen.Const("informe.docx"), Gen.Const("plan.pdf")),
            (titulo, descripcion, ruta) => new Documento
            {
                Titulo = titulo,
                Descripcion = descripcion,
                ArchivoRuta = ruta
            });

    /// <summary>
    /// Construye un <see cref="ClaimsPrincipal"/> autenticado con el rol indicado y
    /// claims adicionales arbitrarios para ejercitar principales variados.
    /// </summary>
    private static ClaimsPrincipal ConstruirPrincipal(string rol, IReadOnlyList<(string Tipo, string Valor)> claimsExtra)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, rol) };
        foreach (var (tipo, valor) in claimsExtra)
        {
            claims.Add(new Claim(tipo, valor));
        }

        var identidad = new ClaimsIdentity(claims, authenticationType: "PruebaCookies");
        return new ClaimsPrincipal(identidad);
    }

    /// <summary>
    /// Genera una lista de claims adicionales (nombre, identificador, etc.) para
    /// que el principal presente formas variadas; ninguno debe alterar la
    /// autorización por rol.
    /// </summary>
    private static Gen<IReadOnlyList<(string Tipo, string Valor)>> GenClaimsExtra() =>
        Gen.Select(
            GenTipoClaim(),
            Gen.String[0, 20],
            (tipoClaim, valor) => (tipoClaim, valor))
        .List[0, 4]
        .Select(lista => (IReadOnlyList<(string, string)>)lista);

    /// <summary>
    /// Tipos de claim habituales de un principal autenticado (ninguno relacionado
    /// con la autorización financiera).
    /// </summary>
    private static Gen<string> GenTipoClaim() =>
        Gen.OneOf(
            Gen.Const(ClaimTypes.Name),
            Gen.Const(ClaimTypes.NameIdentifier),
            Gen.Const(ClaimTypes.Email),
            Gen.Const("CustomClaim"));
}
