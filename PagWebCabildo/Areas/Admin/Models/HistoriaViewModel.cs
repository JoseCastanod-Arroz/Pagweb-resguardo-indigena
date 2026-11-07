using System.ComponentModel.DataAnnotations;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista del formulario de edición del texto de historia/presentación
/// del resguardo en el Panel de Administración (Requisito 24.1). Recoge el texto
/// que se persiste sobre la entidad singleton <c>PresentacionResguardo</c>.
/// </summary>
public class HistoriaViewModel
{
    /// <summary>
    /// Texto de presentación/historia del resguardo a persistir.
    /// </summary>
    [Display(Name = "Texto de historia y presentación")]
    public string Texto { get; set; } = string.Empty;
}
