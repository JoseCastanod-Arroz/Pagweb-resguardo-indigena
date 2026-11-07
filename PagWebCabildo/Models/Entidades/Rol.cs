using System.ComponentModel.DataAnnotations;

namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Rol de un administrador dentro del Panel de Administración.
/// Cada valor corresponde a un cargo del cabildo con uso digital.
/// El antiguo valor <c>Editor = 1</c> se renombró a <c>Secretario = 1</c>
/// conservando el valor numérico para mantener la compatibilidad con las
/// cuentas existentes durante la migración (Requisito 1.4).
/// Las anotaciones <see cref="DisplayAttribute"/> proveen las etiquetas en
/// español que muestran los selectores de rol y los listados; no alteran los
/// nombres ni los valores numéricos, de los que dependen las políticas de
/// autorización y la compatibilidad de datos.
/// </summary>
public enum Rol
{
    /// <summary>Gobernador: control total del panel, cuentas y contenido.</summary>
    [Display(Name = "Administrador general (Gobernador)")]
    AdministradorGeneral = 0,

    /// <summary>Secretario: gestiona contenidos y el módulo de Actas (antiguo "Editor").</summary>
    [Display(Name = "Secretario")]
    Secretario = 1,

    /// <summary>Tesorero: gestiona documentos financieros; lectura del resto.</summary>
    [Display(Name = "Tesorero")]
    Tesorero = 2,

    /// <summary>Fiscal (Veeduría): solo lectura y consulta del registro de actividad.</summary>
    [Display(Name = "Fiscal")]
    Fiscal = 3,

    /// <summary>Comisario: gestiona mensajes de contacto y crea convocatorias.</summary>
    [Display(Name = "Comisario")]
    Comisario = 4
}
