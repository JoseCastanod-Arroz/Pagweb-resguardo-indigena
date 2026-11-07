namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Información de contacto del resguardo. Entidad singleton (un único registro).
/// </summary>
public class InformacionContacto
{
    public int Id { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string CorreoElectronico { get; set; } = string.Empty;

    public string OtrosDatos { get; set; } = string.Empty;
}
