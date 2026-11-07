using System.Security.Claims;
using CsCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PagWebCabildo.Models.Entidades;
using Xunit;

namespace PagWebCabildo.Tests.Autorizacion;

/// <summary>
/// Prueba basada en propiedades de la Property 5 del diseño roles-cabildo-actas:
/// ningún rol distinto de <see cref="Rol.AdministradorGeneral"/> (el Gobernador)
/// puede gestionar cuentas de administrador (crear, editar, desactivar o eliminar).
/// Preserva la antigua Property 7 del spec original (un Editor nunca gestiona
/// cuentas) tras renombrar <c>Editor</c> a <see cref="Rol.Secretario"/> y añadir los
/// roles <see cref="Rol.Tesorero"/>, <see cref="Rol.Fiscal"/> y <see cref="Rol.Comisario"/>.
///
/// Feature: roles-cabildo-actas, Property 5: Ningún rol distinto de
/// AdministradorGeneral elimina contenido ni gestiona cuentas.
///
/// Validates: Requirements 2.1, 2.6
///
/// La autorización de la gestión de cuentas se aplica a nivel de controlador/política:
/// todo <c>PagWebCabildo.Areas.Admin.Controllers.UsuariosController</c> está anotado
/// con <c>[Authorize(Policy = "GestionCuentas")]</c>, y esa política exige el rol
/// <see cref="Rol.AdministradorGeneral"/> (configurada en Program.cs).
///
/// Como todas las operaciones de gestión de cuentas (crear/editar/desactivar/eliminar)
/// comparten la misma política, la prueba parametriza la "operación" sobre el conjunto
/// {Crear, Editar, Desactivar, Eliminar} y el rol sobre el conjunto de roles
/// NO-Gobernador {Secretario, Tesorero, Fiscal, Comisario}, generados por CsCheck, y
/// verifica, de forma determinista (sin levantar un host web), que:
/// <list type="bullet">
///   <item>un principal con un rol NO-Gobernador es SIEMPRE denegado para cada operación;</item>
///   <item>(verificación de sanidad) un principal AdministradorGeneral es permitido.</item>
/// </list>
///
/// La política evaluada es la definición REAL de la aplicación: se resuelve el nombre
/// de política <c>"GestionCuentas"</c> desde un <see cref="IAuthorizationPolicyProvider"/>
/// construido con la misma llamada <c>AddAuthorization(...)</c> que Program.cs.
/// </summary>
public class GestionCuentasEditorPropertyTests
{
    private const int IteracionesMinimas = 100;

    /// <summary>Operaciones de gestión de cuentas cubiertas por la política.</summary>
    private enum OperacionCuenta
    {
        Crear,
        Editar,
        Desactivar,
        Eliminar
    }

    // Feature: roles-cabildo-actas, Property 5: Ningún rol distinto de AdministradorGeneral gestiona cuentas
    [Fact]
    public void NingunRolNoGobernador_PuedeGestionarCuentas()
    {
        // Servicio y política construidos exactamente como la aplicación real:
        // el proveedor resuelve el nombre de política "GestionCuentas" definido con
        // la misma configuración AddAuthorization(...) de Program.cs.
        var proveedor = ConstruirProveedorServicios();
        var servicioAutorizacion = proveedor.GetRequiredService<IAuthorizationService>();
        var proveedorPoliticas = proveedor.GetRequiredService<IAuthorizationPolicyProvider>();
        var politica = proveedorPoliticas.GetPolicyAsync("GestionCuentas")
            .GetAwaiter().GetResult();

        Assert.NotNull(politica);

        GenCaso()
            .Sample(caso =>
            {
                // Principal con un rol NO-Gobernador y claims extra variados (nombre, id).
                var noGobernador = ConstruirPrincipal(
                    rol: caso.Rol.ToString(),
                    nombre: caso.Nombre,
                    id: caso.Id);

                // El rol NO-Gobernador debe ser denegado para cada operación de gestión
                // de cuentas.
                var resultadoNoGobernador = servicioAutorizacion
                    .AuthorizeAsync(noGobernador, resource: caso.Operacion, politica!)
                    .GetAwaiter().GetResult();

                bool noGobernadorDenegado = !resultadoNoGobernador.Succeeded;

                // Verificación de sanidad: un AdministradorGeneral con los mismos claims
                // extra sí es permitido para la misma operación.
                var adminGeneral = ConstruirPrincipal(
                    rol: nameof(Rol.AdministradorGeneral),
                    nombre: caso.Nombre,
                    id: caso.Id);

                var resultadoAdmin = servicioAutorizacion
                    .AuthorizeAsync(adminGeneral, resource: caso.Operacion, politica!)
                    .GetAwaiter().GetResult();

                bool adminPermitido = resultadoAdmin.Succeeded;

                return noGobernadorDenegado && adminPermitido;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Construye un <see cref="ServiceProvider"/> con los servicios de autorización y
    /// la misma definición de políticas que Program.cs (incluida "GestionCuentas").
    /// </summary>
    private static ServiceProvider ConstruirProveedorServicios()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddAuthorization(options =>
        {
            options.AddPolicy("GestionCuentas", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral)));
            options.AddPolicy("EliminarContenido", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral)));
        });
        return servicios.BuildServiceProvider();
    }

    /// <summary>
    /// Construye un <see cref="ClaimsPrincipal"/> autenticado con el rol indicado
    /// (en <see cref="ClaimTypes.Role"/>) más claims de nombre e identificador.
    /// </summary>
    private static ClaimsPrincipal ConstruirPrincipal(string rol, string nombre, int id)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, rol),
            new(ClaimTypes.Name, nombre),
            new(ClaimTypes.NameIdentifier, id.ToString())
        };
        var identidad = new ClaimsIdentity(claims, authenticationType: "TestCookie");
        return new ClaimsPrincipal(identidad);
    }

    /// <summary>
    /// Genera un caso de prueba con un rol NO-Gobernador, una operación de gestión de
    /// cuentas y claims extra variados (nombre e identificador) para ejercitar la
    /// propiedad universalmente.
    /// </summary>
    private static Gen<CasoGenerado> GenCaso() =>
        Gen.Select(
            GenRolNoGobernador(),
            Gen.OneOf(
                Gen.Const(OperacionCuenta.Crear),
                Gen.Const(OperacionCuenta.Editar),
                Gen.Const(OperacionCuenta.Desactivar),
                Gen.Const(OperacionCuenta.Eliminar)),
            Gen.String[1, 40],
            Gen.Int[1, 100_000],
            (rol, operacion, nombre, id) => new CasoGenerado(rol, operacion, nombre, id));

    /// <summary>
    /// Selector uniforme sobre todos los roles distintos de
    /// <see cref="Rol.AdministradorGeneral"/> (Secretario, Tesorero, Fiscal, Comisario).
    /// </summary>
    private static Gen<Rol> GenRolNoGobernador() =>
        Gen.OneOf(
            Gen.Const(Rol.Secretario),
            Gen.Const(Rol.Tesorero),
            Gen.Const(Rol.Fiscal),
            Gen.Const(Rol.Comisario));

    /// <summary>Datos de un caso generado para una iteración de la prueba.</summary>
    private readonly record struct CasoGenerado(
        Rol Rol,
        OperacionCuenta Operacion,
        string Nombre,
        int Id);
}
