using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de gestión de las cuentas de administrador (Requisitos 21, 22 y 23).
/// Encapsula el CRUD de <see cref="Administrador"/> y aplica el <i>hashing</i> de
/// contraseñas con el componente estándar, de modo que las contraseñas en texto
/// plano nunca se almacenan.
/// </summary>
/// <remarks>
/// La autorización por rol (quién puede gestionar cuentas) se aplica en la capa de
/// controladores/políticas, no dentro de este servicio.
/// </remarks>
public interface IUsuarioService
{
    /// <summary>
    /// Lista todas las cuentas de administrador.
    /// </summary>
    /// <returns>Las cuentas de administrador registradas.</returns>
    Task<IReadOnlyList<Administrador>> Listar();

    /// <summary>
    /// Obtiene una cuenta de administrador por su identificador.
    /// </summary>
    /// <param name="id">Identificador del administrador.</param>
    /// <returns>El administrador encontrado, o <c>null</c> si no existe.</returns>
    Task<Administrador?> Obtener(int id);

    /// <summary>
    /// Crea una nueva cuenta de administrador con la contraseña <i>hasheada</i> y
    /// <see cref="Administrador.Activo"/> en <c>true</c> (Requisito 21.1).
    /// </summary>
    /// <param name="nombre">Nombre de la persona titular de la cuenta.</param>
    /// <param name="nombreUsuario">Nombre de usuario único para iniciar sesión.</param>
    /// <param name="contraseña">Contraseña en texto plano a <i>hashear</i>.</param>
    /// <param name="rol">Rol asignado a la cuenta.</param>
    /// <returns>El administrador creado, con su identificador asignado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si ya existe una cuenta con el mismo <paramref name="nombreUsuario"/>.
    /// </exception>
    Task<Administrador> Crear(string nombre, string nombreUsuario, string contraseña, Rol rol);

    /// <summary>
    /// Edita una cuenta de administrador existente (Requisito 22.1). Si se suministra
    /// una nueva <paramref name="contraseña"/> se vuelve a <i>hashear</i>; si es
    /// <c>null</c> o vacía se conserva el hash actual.
    /// </summary>
    /// <param name="id">Identificador del administrador a editar.</param>
    /// <param name="nombre">Nuevo nombre de la persona titular.</param>
    /// <param name="nombreUsuario">Nuevo nombre de usuario único.</param>
    /// <param name="rol">Nuevo rol de la cuenta.</param>
    /// <param name="contraseña">
    /// Nueva contraseña en texto plano, o <c>null</c>/vacía para conservar la actual.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Si no existe la cuenta, o si el <paramref name="nombreUsuario"/> lo usa otra cuenta.
    /// </exception>
    Task Editar(int id, string nombre, string nombreUsuario, Rol rol, string? contraseña = null);

    /// <summary>
    /// Desactiva una cuenta de administrador estableciendo
    /// <see cref="Administrador.Activo"/> en <c>false</c>, lo que impide su inicio de
    /// sesión (Requisito 22.2).
    /// </summary>
    /// <param name="id">Identificador del administrador a desactivar.</param>
    /// <exception cref="InvalidOperationException">Si no existe la cuenta.</exception>
    Task Desactivar(int id);

    /// <summary>
    /// Cambia el estado de actividad de una cuenta de administrador. Desactivar
    /// (<paramref name="activo"/> en <c>false</c>) impide el inicio de sesión
    /// (Requisito 22.2).
    /// </summary>
    /// <param name="id">Identificador del administrador.</param>
    /// <param name="activo">Nuevo estado de actividad.</param>
    /// <exception cref="InvalidOperationException">Si no existe la cuenta.</exception>
    Task CambiarEstado(int id, bool activo);

    /// <summary>
    /// Elimina una cuenta de administrador por su identificador (Requisito 23.1).
    /// </summary>
    /// <param name="id">Identificador del administrador a eliminar.</param>
    Task Eliminar(int id);
}
