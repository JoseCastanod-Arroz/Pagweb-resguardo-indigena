using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IAutenticacionService"/> sobre <see cref="AppDbContext"/>.
/// Usa el mismo <see cref="PasswordHasher{TUser}"/> que <see cref="UsuarioService"/>
/// para verificar las contraseñas, lo que garantiza compatibilidad con el
/// administrador sembrado ("admin" / "Admin123*").
/// </summary>
public sealed class AutenticacionService : IAutenticacionService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Administrador> _hasher = new();

    public AutenticacionService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<Administrador?> ValidarCredenciales(string usuario, string contraseña)
    {
        var nombreUsuario = (usuario ?? string.Empty).Trim();

        var administrador = await _db.Administradores
            .FirstOrDefaultAsync(a => a.NombreUsuario == nombreUsuario);

        // Cuenta inexistente: credenciales inválidas (Requisito 18.2).
        if (administrador is null)
        {
            return null;
        }

        // Cuenta desactivada: no puede iniciar sesión (Requisitos 18.2, 22.2).
        if (!administrador.Activo)
        {
            return null;
        }

        var resultado = _hasher.VerifyHashedPassword(
            administrador, administrador.HashContraseña, contraseña ?? string.Empty);

        // La contraseña verifica cuando el resultado es Success o SuccessRehashNeeded
        // (este último indica que el hash es válido pero conviene re-hashear).
        if (resultado == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return administrador;
    }
}
