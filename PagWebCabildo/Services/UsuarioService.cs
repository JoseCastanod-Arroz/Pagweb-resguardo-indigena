using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IUsuarioService"/> sobre <see cref="AppDbContext"/>.
/// Usa <see cref="PasswordHasher{TUser}"/> para <i>hashear</i> y así nunca almacenar
/// contraseñas en texto plano. El mismo <i>hasher</i> lo emplea
/// <see cref="AutenticacionService"/> al verificar credenciales, garantizando
/// compatibilidad con el administrador sembrado.
/// </summary>
public sealed class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<Administrador> _hasher = new();

    public UsuarioService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Administrador>> Listar()
    {
        return await _db.Administradores
            .OrderBy(a => a.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Administrador?> Obtener(int id)
    {
        return await _db.Administradores.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Administrador> Crear(string nombre, string nombreUsuario, string contraseña, Rol rol)
    {
        nombreUsuario = (nombreUsuario ?? string.Empty).Trim();

        // Pre-chequeo del nombre de usuario único para dar un mensaje claro antes de
        // chocar con el índice único de la base de datos (Requisito 21.1).
        await AsegurarNombreUsuarioDisponible(nombreUsuario, idExcluido: null);

        var administrador = new Administrador
        {
            Nombre = nombre ?? string.Empty,
            NombreUsuario = nombreUsuario,
            Rol = rol,
            Activo = true
        };

        // El hash depende de la instancia de Administrador; se asigna tras construirla.
        administrador.HashContraseña = _hasher.HashPassword(administrador, contraseña ?? string.Empty);

        _db.Administradores.Add(administrador);
        await GuardarCapturandoDuplicado(nombreUsuario);
        return administrador;
    }

    /// <inheritdoc />
    public async Task Editar(int id, string nombre, string nombreUsuario, Rol rol, string? contraseña = null)
    {
        var administrador = await _db.Administradores.FindAsync(id)
            ?? throw new InvalidOperationException($"No existe un administrador con Id {id}.");

        nombreUsuario = (nombreUsuario ?? string.Empty).Trim();

        // El nombre de usuario debe seguir siendo único, excluyendo la propia cuenta.
        await AsegurarNombreUsuarioDisponible(nombreUsuario, idExcluido: id);

        administrador.Nombre = nombre ?? string.Empty;
        administrador.NombreUsuario = nombreUsuario;
        administrador.Rol = rol;

        // Solo se re-hashea cuando se proporciona una contraseña nueva; de lo
        // contrario se conserva el hash existente (Requisito 22.1).
        if (!string.IsNullOrWhiteSpace(contraseña))
        {
            administrador.HashContraseña = _hasher.HashPassword(administrador, contraseña);
        }

        await GuardarCapturandoDuplicado(nombreUsuario);
    }

    /// <inheritdoc />
    public Task Desactivar(int id) => CambiarEstado(id, activo: false);

    /// <inheritdoc />
    public async Task CambiarEstado(int id, bool activo)
    {
        var administrador = await _db.Administradores.FindAsync(id)
            ?? throw new InvalidOperationException($"No existe un administrador con Id {id}.");

        administrador.Activo = activo;
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task Eliminar(int id)
    {
        var administrador = await _db.Administradores.FindAsync(id);
        if (administrador is null)
        {
            return;
        }

        _db.Administradores.Remove(administrador);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Verifica que no exista otra cuenta con el mismo nombre de usuario.
    /// </summary>
    private async Task AsegurarNombreUsuarioDisponible(string nombreUsuario, int? idExcluido)
    {
        var existe = await _db.Administradores
            .AnyAsync(a => a.NombreUsuario == nombreUsuario &&
                           (idExcluido == null || a.Id != idExcluido));

        if (existe)
        {
            throw new InvalidOperationException(
                $"Ya existe un administrador con el nombre de usuario \"{nombreUsuario}\".");
        }
    }

    /// <summary>
    /// Guarda los cambios traduciendo una violación del índice único de nombre de
    /// usuario en un <see cref="InvalidOperationException"/> con mensaje claro, para
    /// cubrir condiciones de carrera que el pre-chequeo no detecta.
    /// </summary>
    private async Task GuardarCapturandoDuplicado(string nombreUsuario)
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                $"Ya existe un administrador con el nombre de usuario \"{nombreUsuario}\".");
        }
    }
}
