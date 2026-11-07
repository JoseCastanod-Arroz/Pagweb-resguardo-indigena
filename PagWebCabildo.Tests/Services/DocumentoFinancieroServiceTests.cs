using System.Linq;
using System.Threading.Tasks;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Pruebas unitarias basadas en ejemplos (no en propiedades) del filtrado
/// financiero de <see cref="DocumentoService"/> (implementación de
/// <see cref="IDocumentoService"/>), que sustenta el rol Tesorero:
///
/// <list type="bullet">
/// <item>ListarCategoriasFinancieras devuelve solo categorías con
/// <see cref="CategoriaDocumento.EsFinanciero"/> igual a <c>true</c>.</item>
/// <item>ListarDocumentosFinancieros devuelve solo documentos cuya categoría es
/// financiera.</item>
/// <item>EsCategoriaFinanciera y EsDocumentoFinanciero devuelven el booleano
/// correcto, incluido <c>false</c> para identificadores inexistentes.</item>
/// <item>EditarCategoria persiste el marcado financiero.</item>
/// </list>
///
/// Cada prueba usa una base SQLite en memoria recién creada
/// (<see cref="SqliteEnMemoria"/>) con contextos independientes para escribir y
/// releer, de modo que las aserciones observen datos efectivamente persistidos.
///
/// Validates: Requirements 3.2, 4.1, 5.1
/// </summary>
public sealed class DocumentoFinancieroServiceTests
{
    // -----------------------------------------------------------------------
    // ListarCategoriasFinancieras: solo categorías financieras.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListarCategoriasFinancieras_DevuelveSoloCategoriasFinancieras()
    {
        using var bd = new SqliteEnMemoria();
        await SembrarCategoriasYDocumentos(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new DocumentoService(ctx);

        var financieras = await servicio.ListarCategoriasFinancieras();

        Assert.All(financieras, c => Assert.True(c.EsFinanciero));
        Assert.Contains(financieras, c => c.Nombre == "Finanzas");
        Assert.DoesNotContain(financieras, c => c.Nombre == "General");
    }

    // -----------------------------------------------------------------------
    // ListarDocumentosFinancieros: solo documentos de categorías financieras.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListarDocumentosFinancieros_DevuelveSoloDocumentosDeCategoriasFinancieras()
    {
        using var bd = new SqliteEnMemoria();
        var ids = await SembrarCategoriasYDocumentos(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new DocumentoService(ctx);

        var documentos = await servicio.ListarDocumentosFinancieros();

        // Solo el documento bajo la categoría "Finanzas" debe aparecer.
        Assert.Single(documentos);
        Assert.Equal(ids.CategoriaFinancieraId, documentos[0].CategoriaDocumentoId);
        Assert.Equal("Balance 2025", documentos[0].Titulo);
    }

    // -----------------------------------------------------------------------
    // EsCategoriaFinanciera / EsDocumentoFinanciero: booleanos correctos.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EsCategoriaFinanciera_DevuelveBooleanoCorrectoIncluyendoInexistente()
    {
        using var bd = new SqliteEnMemoria();
        var ids = await SembrarCategoriasYDocumentos(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new DocumentoService(ctx);

        Assert.True(await servicio.EsCategoriaFinanciera(ids.CategoriaFinancieraId));
        Assert.False(await servicio.EsCategoriaFinanciera(ids.CategoriaNoFinancieraId));
        // Categoría inexistente: false.
        Assert.False(await servicio.EsCategoriaFinanciera(999999));
    }

    [Fact]
    public async Task EsDocumentoFinanciero_DevuelveBooleanoCorrectoIncluyendoInexistente()
    {
        using var bd = new SqliteEnMemoria();
        var ids = await SembrarCategoriasYDocumentos(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new DocumentoService(ctx);

        Assert.True(await servicio.EsDocumentoFinanciero(ids.DocumentoFinancieroId));
        Assert.False(await servicio.EsDocumentoFinanciero(ids.DocumentoNoFinancieroId));
        // Documento inexistente: false.
        Assert.False(await servicio.EsDocumentoFinanciero(999999));
    }

    // -----------------------------------------------------------------------
    // EditarCategoria: persiste el marcado financiero.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EditarCategoria_PersisteMarcadoFinanciero()
    {
        using var bd = new SqliteEnMemoria();

        int categoriaId;

        // Crear una categoría no financiera.
        using (var ctxCrear = bd.CrearContexto())
        {
            var servicio = new DocumentoService(ctxCrear);
            var creada = await servicio.CrearCategoria(new CategoriaDocumento
            {
                Nombre = "Informes",
                EsFinanciero = false
            });
            categoriaId = creada.Id;
        }

        // Editar para marcarla como financiera.
        using (var ctxEditar = bd.CrearContexto())
        {
            var servicio = new DocumentoService(ctxEditar);
            var editada = await servicio.EditarCategoria(new CategoriaDocumento
            {
                Id = categoriaId,
                Nombre = "Informes financieros",
                EsFinanciero = true
            });

            Assert.NotNull(editada);
            Assert.True(editada!.EsFinanciero);
        }

        // Releer con contexto independiente y confirmar la persistencia del flag.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new DocumentoService(ctxLectura);
            var leida = await servicio.ObtenerCategoria(categoriaId);
            Assert.NotNull(leida);
            Assert.True(leida!.EsFinanciero);
            Assert.Equal("Informes financieros", leida.Nombre);
        }
    }

    // -----------------------------------------------------------------------
    // Siembra compartida.
    // -----------------------------------------------------------------------

    /// <summary>
    /// Siembra dos categorías (una financiera y otra no) con un documento cada
    /// una y devuelve los identificadores asignados por EF.
    /// </summary>
    private static async Task<IdsSembrados> SembrarCategoriasYDocumentos(SqliteEnMemoria bd)
    {
        using var ctx = bd.CrearContexto();

        var financiera = new CategoriaDocumento { Nombre = "Finanzas", EsFinanciero = true };
        var general = new CategoriaDocumento { Nombre = "General", EsFinanciero = false };
        ctx.CategoriasDocumento.AddRange(financiera, general);
        await ctx.SaveChangesAsync();

        var docFinanciero = new Documento
        {
            Titulo = "Balance 2025",
            Descripcion = "Balance financiero anual.",
            ArchivoRuta = "uploads/documentos/balance-2025.pdf",
            CategoriaDocumentoId = financiera.Id
        };
        var docGeneral = new Documento
        {
            Titulo = "Reglamento interno",
            Descripcion = "Documento general del resguardo.",
            ArchivoRuta = "uploads/documentos/reglamento.pdf",
            CategoriaDocumentoId = general.Id
        };
        ctx.Documentos.AddRange(docFinanciero, docGeneral);
        await ctx.SaveChangesAsync();

        return new IdsSembrados
        {
            CategoriaFinancieraId = financiera.Id,
            CategoriaNoFinancieraId = general.Id,
            DocumentoFinancieroId = docFinanciero.Id,
            DocumentoNoFinancieroId = docGeneral.Id
        };
    }

    /// <summary>Identificadores de los datos sembrados para las aserciones.</summary>
    private sealed class IdsSembrados
    {
        public int CategoriaFinancieraId { get; init; }
        public int CategoriaNoFinancieraId { get; init; }
        public int DocumentoFinancieroId { get; init; }
        public int DocumentoNoFinancieroId { get; init; }
    }
}
