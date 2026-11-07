using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Models.ViewModels;

/// <summary>
/// Modelo de vista para la página de inicio del sitio público (Requisitos 1.1,
/// 1.3 y 1.4). Agrupa el texto de presentación del resguardo, los últimos
/// eventos registrados (ordenados por proximidad) y los últimos comunicados
/// publicados (recientes primero).
/// </summary>
public class InicioViewModel
{
    /// <summary>
    /// Texto de presentación/historia del resguardo que se muestra en el inicio
    /// (Requisito 1.1).
    /// </summary>
    public string Presentacion { get; set; } = string.Empty;

    /// <summary>
    /// Últimos eventos registrados, ordenados por proximidad de fecha
    /// (Requisito 1.3).
    /// </summary>
    public IReadOnlyList<Evento> UltimosEventos { get; set; } = new List<Evento>();

    /// <summary>
    /// Últimos comunicados publicados, del más reciente al más antiguo
    /// (Requisito 1.4).
    /// </summary>
    public IReadOnlyList<Comunicado> UltimosComunicados { get; set; } = new List<Comunicado>();
}
