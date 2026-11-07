namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Cuenta de administrador del sitio. El <see cref="NombreUsuario"/> es único.
/// </summary>
public class Administrador
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string HashContraseña { get; set; } = string.Empty;

    public Rol Rol { get; set; }

    public bool Activo { get; set; }
}
