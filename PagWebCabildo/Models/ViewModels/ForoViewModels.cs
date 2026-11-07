using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Models.ViewModels;

/// <summary>
/// Modelo de vista para que un visitante anónimo envíe una publicación (receta o
/// historia) al foro comunitario. La publicación queda pendiente de moderación.
/// La imagen es opcional; su guardado físico corre por cuenta de
/// <c>IArchivoService</c> en el controlador.
/// </summary>
public class PublicacionForoFormViewModel
{
    /// <summary>
    /// Nombre con el que firma el visitante (obligatorio, 1–100 caracteres).
    /// </summary>
    [Display(Name = "Tu nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Autor { get; set; } = string.Empty;

    /// <summary>Tipo de publicación: receta o historia (obligatorio).</summary>
    [Display(Name = "Tipo de publicación")]
    [Required(ErrorMessage = "Elige si es una receta o una historia.")]
    public TipoPublicacionForo Tipo { get; set; } = TipoPublicacionForo.Receta;

    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Contenido")]
    [Required(ErrorMessage = "El contenido es obligatorio.")]
    [StringLength(8000, MinimumLength = 1,
        ErrorMessage = "El contenido debe tener entre {2} y {1} caracteres.")]
    public string Contenido { get; set; } = string.Empty;

    /// <summary>Imagen opcional de la receta o historia.</summary>
    [Display(Name = "Imagen (opcional)")]
    public IFormFile? Imagen { get; set; }
}

/// <summary>
/// Modelo de vista para que un visitante anónimo envíe un comentario a una
/// publicación del foro. El comentario queda pendiente de moderación.
/// </summary>
public class ComentarioForoFormViewModel
{
    /// <summary>Publicación a la que se comenta.</summary>
    public int PublicacionForoId { get; set; }

    [Display(Name = "Tu nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Display(Name = "Comentario")]
    [Required(ErrorMessage = "El comentario es obligatorio.")]
    [StringLength(2000, MinimumLength = 1,
        ErrorMessage = "El comentario debe tener entre {2} y {1} caracteres.")]
    public string Contenido { get; set; } = string.Empty;
}

/// <summary>
/// Modelo de vista compuesto para el detalle público de una publicación del foro:
/// la publicación con sus comentarios aprobados y el formulario para comentar.
/// </summary>
public class DetallePublicacionForoViewModel
{
    public PublicacionForo Publicacion { get; set; } = new PublicacionForo();

    public ComentarioForoFormViewModel NuevoComentario { get; set; } = new ComentarioForoFormViewModel();
}

/// <summary>
/// Entrada del feed público del foro: una publicación aprobada junto con el número
/// de comentarios aprobados, para mostrarlos en el listado estilo foro.
/// </summary>
public class PublicacionForoFeedItem
{
    public PublicacionForo Publicacion { get; set; } = new PublicacionForo();

    public int ComentariosAprobados { get; set; }
}

/// <summary>
/// Criterio de ordenamiento del feed del foro.
/// </summary>
public enum OrdenForo
{
    /// <summary>Más recientes primero (por fecha de envío).</summary>
    Recientes = 0,

    /// <summary>Más comentados primero (por número de comentarios aprobados).</summary>
    Comentados = 1
}

/// <summary>
/// Página de resultados del feed del foro. Encapsula los elementos de la página
/// actual junto con los datos de paginación, búsqueda y orden, de modo que la
/// vista pueda pintar los controles conservando el estado. Escala para cuando el
/// foro tenga muchas publicaciones: solo se consulta y renderiza una página.
/// </summary>
public class ForoFeedPaginado
{
    public IReadOnlyList<PublicacionForoFeedItem> Items { get; set; } = new List<PublicacionForoFeedItem>();

    /// <summary>Total de publicaciones que cumplen el filtro (todas las páginas).</summary>
    public int TotalItems { get; set; }

    /// <summary>Página actual (1-based).</summary>
    public int Pagina { get; set; } = 1;

    /// <summary>Tamaño de página (elementos por página).</summary>
    public int TamañoPagina { get; set; } = 10;

    /// <summary>Término de búsqueda aplicado (puede ser nulo).</summary>
    public string? Busqueda { get; set; }

    /// <summary>Tipo filtrado (puede ser nulo = todos).</summary>
    public TipoPublicacionForo? Tipo { get; set; }

    /// <summary>Orden aplicado.</summary>
    public OrdenForo Orden { get; set; } = OrdenForo.Recientes;

    /// <summary>Número total de páginas (mínimo 1).</summary>
    public int TotalPaginas => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)TamañoPagina);

    public bool TienePaginaAnterior => Pagina > 1;

    public bool TienePaginaSiguiente => Pagina < TotalPaginas;

    /// <summary>Indica si hay algún filtro o búsqueda activa.</summary>
    public bool HayFiltro => !string.IsNullOrWhiteSpace(Busqueda) || Tipo is not null;
}
