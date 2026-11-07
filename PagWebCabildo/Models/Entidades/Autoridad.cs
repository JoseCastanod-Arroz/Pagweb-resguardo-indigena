using System.ComponentModel.DataAnnotations;

namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Autoridad, cargo o comité de la organización del resguardo.
/// </summary>
public class Autoridad
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Cargo { get; set; } = string.Empty;

    /// <summary>
    /// Clasificación del registro: autoridad, cargo o comité.
    /// </summary>
    public string TipoAutoridad { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de las funciones del cargo (según el Reglamento Interno).
    /// Se muestra en el organigrama público al seleccionar el cargo.
    /// </summary>
    [Display(Name = "Función")]
    public string Funcion { get; set; } = string.Empty;

    public int Orden { get; set; }
}
