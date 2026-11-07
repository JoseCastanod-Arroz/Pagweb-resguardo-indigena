using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la gestión de <see cref="CategoriaDocumento"/> y
/// <see cref="Documento"/>. Expone el CRUD de categorías y de documentos y el
/// listado agrupado por categoría para el sitio público (Requisitos 13.1, 37.1,
/// 37.2, 38 y 39.1).
/// </summary>
/// <remarks>
/// El guardado y la apertura física de los archivos corre por cuenta de
/// <see cref="IArchivoService"/> en la capa de controladores; este servicio solo
/// persiste el valor de <see cref="Documento.ArchivoRuta"/> sobre la entidad y no
/// realiza operaciones de E/S de archivos.
/// </remarks>
public interface IDocumentoService
{
    // ----- Listado público agrupado -----

    /// <summary>
    /// Lista las categorías de documentos con sus documentos cargados, agrupando
    /// cada documento bajo su propia categoría (por <see cref="Documento.CategoriaDocumentoId"/>).
    /// </summary>
    /// <remarks>
    /// Es la única fuente de verdad de la Property 12 del diseño (Requisito 13.1):
    /// cada documento aparece únicamente bajo su categoría y el conjunto de
    /// documentos agrupados es exactamente el conjunto original, sin pérdidas ni
    /// duplicados. El agrupamiento se realiza estrictamente por la clave foránea
    /// <see cref="Documento.CategoriaDocumentoId"/>.
    /// </remarks>
    /// <returns>
    /// Las categorías con su colección <see cref="CategoriaDocumento.Documentos"/>
    /// poblada. Una categoría sin documentos aparece con la colección vacía.
    /// </returns>
    Task<IReadOnlyList<CategoriaDocumento>> ListarAgrupadosPorCategoria();

    /// <summary>
    /// Como <see cref="ListarAgrupadosPorCategoria"/>, pero devuelve únicamente las
    /// categorías marcadas como públicas (<see cref="CategoriaDocumento.EsPublica"/>).
    /// Es el listado que consume el sitio público: las categorías privadas y sus
    /// documentos nunca se exponen a visitantes.
    /// </summary>
    Task<IReadOnlyList<CategoriaDocumento>> ListarPublicasAgrupadas();

    /// <summary>
    /// Indica si un documento pertenece a una categoría pública. Se usa para
    /// autorizar la descarga pública: solo los documentos de categorías públicas
    /// se entregan a visitantes sin sesión.
    /// </summary>
    /// <param name="documentoId">Identificador del documento.</param>
    /// <returns><c>true</c> si el documento existe y su categoría es pública.</returns>
    Task<bool> EsDocumentoPublico(int documentoId);

    // ----- CRUD de categorías -----

    /// <summary>
    /// Lista todas las categorías de documentos (sin cargar sus documentos),
    /// ordenadas por nombre, para su gestión desde el Panel_Admin.
    /// </summary>
    /// <returns>Las categorías de documentos existentes.</returns>
    Task<IReadOnlyList<CategoriaDocumento>> ListarCategorias();

    /// <summary>
    /// Obtiene una categoría de documentos por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la categoría.</param>
    /// <returns>La <see cref="CategoriaDocumento"/> encontrada, o <c>null</c> si no existe.</returns>
    Task<CategoriaDocumento?> ObtenerCategoria(int id);

    /// <summary>
    /// Crea y persiste una nueva categoría de documentos.
    /// </summary>
    /// <param name="categoria">Datos de la categoría a crear.</param>
    /// <returns>La <see cref="CategoriaDocumento"/> creada con su identificador asignado.</returns>
    Task<CategoriaDocumento> CrearCategoria(CategoriaDocumento categoria);

    /// <summary>
    /// Actualiza los datos de una categoría de documentos existente.
    /// </summary>
    /// <param name="categoria">Categoría con el identificador y los datos actualizados.</param>
    /// <returns>La <see cref="CategoriaDocumento"/> actualizada, o <c>null</c> si no existe.</returns>
    Task<CategoriaDocumento?> EditarCategoria(CategoriaDocumento categoria);

    /// <summary>
    /// Elimina una categoría de documentos por su identificador. Por la regla de
    /// borrado en cascada del modelo, se eliminan también sus documentos asociados.
    /// </summary>
    /// <param name="id">Identificador de la categoría a eliminar.</param>
    /// <returns><c>true</c> si se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> EliminarCategoria(int id);

    // ----- Filtrado financiero (rol Tesorero) -----

    /// <summary>
    /// Lista las categorías de documentos marcadas como financieras
    /// (<see cref="CategoriaDocumento.EsFinanciero"/> igual a <c>true</c>),
    /// ordenadas por nombre, para la gestión del rol Tesorero.
    /// </summary>
    /// <remarks>
    /// Sustenta la Property 4 del diseño (Requisitos 6.1 y 6.2): el Tesorero solo
    /// gestiona documentos cuya categoría es financiera.
    /// </remarks>
    /// <returns>Las categorías financieras existentes.</returns>
    Task<IReadOnlyList<CategoriaDocumento>> ListarCategoriasFinancieras();

    /// <summary>
    /// Lista los documentos cuya categoría está marcada como financiera
    /// (<see cref="CategoriaDocumento.EsFinanciero"/> igual a <c>true</c>).
    /// </summary>
    /// <remarks>
    /// Sustenta la Property 4 del diseño (Requisitos 6.1 y 6.2): el conjunto
    /// devuelto es exactamente el de documentos que el Tesorero puede gestionar.
    /// </remarks>
    /// <returns>Los documentos de categorías financieras.</returns>
    Task<IReadOnlyList<Documento>> ListarDocumentosFinancieros();

    /// <summary>
    /// Indica si una categoría de documentos es financiera.
    /// </summary>
    /// <param name="categoriaId">Identificador de la categoría.</param>
    /// <returns>
    /// <c>true</c> si la categoría existe y tiene
    /// <see cref="CategoriaDocumento.EsFinanciero"/> igual a <c>true</c>;
    /// <c>false</c> si no es financiera o si la categoría no existe.
    /// </returns>
    Task<bool> EsCategoriaFinanciera(int categoriaId);

    /// <summary>
    /// Indica si un documento pertenece a una categoría financiera. Útil para la
    /// autorización del rol Tesorero sobre un documento concreto.
    /// </summary>
    /// <param name="documentoId">Identificador del documento.</param>
    /// <returns>
    /// <c>true</c> si el documento existe y su categoría tiene
    /// <see cref="CategoriaDocumento.EsFinanciero"/> igual a <c>true</c>;
    /// <c>false</c> si no es financiero o si el documento no existe.
    /// </returns>
    Task<bool> EsDocumentoFinanciero(int documentoId);

    // ----- CRUD de documentos -----

    /// <summary>
    /// Obtiene un documento por su identificador.
    /// </summary>
    /// <param name="id">Identificador del documento.</param>
    /// <returns>El <see cref="Documento"/> encontrado, o <c>null</c> si no existe.</returns>
    Task<Documento?> ObtenerDocumento(int id);

    /// <summary>
    /// Crea y persiste un nuevo documento. La ruta física del archivo
    /// (<see cref="Documento.ArchivoRuta"/>) debe fijarse por la capa de
    /// controladores mediante <see cref="IArchivoService"/> antes de llamar a este
    /// método.
    /// </summary>
    /// <param name="documento">Datos del documento a crear.</param>
    /// <returns>El <see cref="Documento"/> creado con su identificador asignado.</returns>
    Task<Documento> CrearDocumento(Documento documento);

    /// <summary>
    /// Actualiza los datos de un documento existente (título, descripción, ruta de
    /// archivo y categoría).
    /// </summary>
    /// <param name="documento">Documento con el identificador y los datos actualizados.</param>
    /// <returns>El <see cref="Documento"/> actualizado, o <c>null</c> si no existe.</returns>
    Task<Documento?> EditarDocumento(Documento documento);

    /// <summary>
    /// Elimina un documento por su identificador.
    /// </summary>
    /// <param name="id">Identificador del documento a eliminar.</param>
    /// <returns><c>true</c> si se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> EliminarDocumento(int id);
}
