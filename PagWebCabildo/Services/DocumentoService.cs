using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IDocumentoService"/> sobre <see cref="AppDbContext"/>.
/// Concentra el CRUD de categorías y documentos y el agrupamiento por categoría
/// para el sitio público. Es la única fuente de verdad de la Property 12 del
/// diseño: cada documento se agrupa bajo su propia categoría (Requisito 13.1).
/// </summary>
/// <remarks>
/// La E/S física de archivos corresponde a <see cref="IArchivoService"/> en la
/// capa de controladores; este servicio solo persiste el valor de
/// <see cref="Documento.ArchivoRuta"/>.
/// </remarks>
public sealed class DocumentoService : IDocumentoService
{
    private readonly AppDbContext _contexto;

    /// <summary>
    /// Crea el servicio con el contexto de datos inyectado.
    /// </summary>
    /// <param name="contexto">Contexto de EF Core del sitio del resguardo.</param>
    public DocumentoService(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDocumento>> ListarAgrupadosPorCategoria()
    {
        // Agrupamiento por categoría (Property 12, Requisito 13.1). Al cargar las
        // categorías con su colección Documentos mediante Include, EF materializa
        // cada documento exactamente una vez, bajo la categoría a la que apunta su
        // clave foránea CategoriaDocumentoId (la relación 1-a-muchos está
        // configurada en AppDbContext.OnModelCreating). Esto garantiza que ningún
        // documento se pierda ni se duplique y que cada documento aparezca
        // únicamente bajo su propia categoría.
        return await _contexto.CategoriasDocumento
            .Include(c => c.Documentos)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDocumento>> ListarPublicasAgrupadas()
    {
        // Solo las categorías públicas se exponen en el sitio público. Las privadas
        // y sus documentos nunca se listan ni son descargables sin sesión.
        return await _contexto.CategoriasDocumento
            .Where(c => c.EsPublica)
            .Include(c => c.Documentos)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> EsDocumentoPublico(int documentoId)
    {
        return await _contexto.Documentos
            .AnyAsync(d => d.Id == documentoId
                && d.CategoriaDocumento != null
                && d.CategoriaDocumento.EsPublica);
    }

    // ----- CRUD de categorías -----

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDocumento>> ListarCategorias()
    {
        return await _contexto.CategoriasDocumento
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CategoriaDocumento?> ObtenerCategoria(int id)
    {
        return await _contexto.CategoriasDocumento.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<CategoriaDocumento> CrearCategoria(CategoriaDocumento categoria)
    {
        _contexto.CategoriasDocumento.Add(categoria);
        await _contexto.SaveChangesAsync();
        return categoria;
    }

    /// <inheritdoc />
    public async Task<CategoriaDocumento?> EditarCategoria(CategoriaDocumento categoria)
    {
        var existente = await _contexto.CategoriasDocumento.FindAsync(categoria.Id);
        if (existente is null)
        {
            return null;
        }

        existente.Nombre = categoria.Nombre;
        existente.EsFinanciero = categoria.EsFinanciero;
        existente.EsPublica = categoria.EsPublica;

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarCategoria(int id)
    {
        var existente = await _contexto.CategoriasDocumento.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.CategoriasDocumento.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }

    // ----- Filtrado financiero (rol Tesorero) -----

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoriaDocumento>> ListarCategoriasFinancieras()
    {
        return await _contexto.CategoriasDocumento
            .Where(c => c.EsFinanciero)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Documento>> ListarDocumentosFinancieros()
    {
        // Filtra por la marca financiera de la categoría a la que apunta la clave
        // foránea CategoriaDocumentoId (Property 4, Requisitos 6.1 y 6.2).
        return await _contexto.Documentos
            .Where(d => d.CategoriaDocumento != null && d.CategoriaDocumento.EsFinanciero)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> EsCategoriaFinanciera(int categoriaId)
    {
        return await _contexto.CategoriasDocumento
            .AnyAsync(c => c.Id == categoriaId && c.EsFinanciero);
    }

    /// <inheritdoc />
    public async Task<bool> EsDocumentoFinanciero(int documentoId)
    {
        return await _contexto.Documentos
            .AnyAsync(d => d.Id == documentoId
                && d.CategoriaDocumento != null
                && d.CategoriaDocumento.EsFinanciero);
    }

    // ----- CRUD de documentos -----

    /// <inheritdoc />
    public async Task<Documento?> ObtenerDocumento(int id)
    {
        return await _contexto.Documentos.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Documento> CrearDocumento(Documento documento)
    {
        _contexto.Documentos.Add(documento);
        await _contexto.SaveChangesAsync();
        return documento;
    }

    /// <inheritdoc />
    public async Task<Documento?> EditarDocumento(Documento documento)
    {
        var existente = await _contexto.Documentos.FindAsync(documento.Id);
        if (existente is null)
        {
            return null;
        }

        existente.Titulo = documento.Titulo;
        existente.Descripcion = documento.Descripcion;
        existente.ArchivoRuta = documento.ArchivoRuta;
        existente.CategoriaDocumentoId = documento.CategoriaDocumentoId;

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarDocumento(int id)
    {
        var existente = await _contexto.Documentos.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.Documentos.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }
}
