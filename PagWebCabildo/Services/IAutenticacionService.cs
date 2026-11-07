using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de autenticación del Panel de Administración (Requisito 18). Valida las
/// credenciales de inicio de sesión comprobando que la cuenta exista, esté activa y
/// que la contraseña coincida con el hash almacenado.
/// </summary>
public interface IAutenticacionService
{
    /// <summary>
    /// Valida las credenciales de un administrador.
    /// </summary>
    /// <param name="usuario">Nombre de usuario de la cuenta.</param>
    /// <param name="contraseña">Contraseña en texto plano a verificar.</param>
    /// <returns>
    /// El <see cref="Administrador"/> cuando la cuenta existe, está activa y la
    /// contraseña verifica; de lo contrario <c>null</c> (no existe, cuenta inactiva —
    /// Requisito 22.2 — o contraseña incorrecta — Requisito 18.2).
    /// </returns>
    Task<Administrador?> ValidarCredenciales(string usuario, string contraseña);
}
