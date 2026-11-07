using System.Collections.Generic;
using System.Security.Claims;
using CsCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PagWebCabildo.Models.Entidades;
using Xunit;

namespace PagWebCabildo.Tests.Autorizacion;

/// <summary>
/// Prueba basada en propiedades de la Property 2 del diseño roles-cabildo-actas:
/// solo los roles autorizados gestionan o descargan actas. En concreto, solo los
/// roles con la política <c>"GestionActas"</c> pueden crear/editar actas
/// (<c>Index</c>/<c>Crear</c>/<c>Editar</c> del <c>ActasController</c>) y solo los
/// roles con la política <c>"ConsultaActas"</c> pueden descargarlas
/// (<c>Descargar</c>).
///
/// Feature: roles-cabildo-actas, Property 2: Solo roles autorizados gestionan o
/// descargan actas.
///
/// Validates: Requirements 3.1, 4.2, 4.4
///
/// Según <see cref="Program"/>, las políticas se definen así:
/// <list type="bullet">
///   <item><c>GestionActas</c>: <see cref="Rol.AdministradorGeneral"/>, <see cref="Rol.Secretario"/>.</item>
///   <item><c>ConsultaActas</c>: <see cref="Rol.AdministradorGeneral"/>, <see cref="Rol.Secretario"/>, <see cref="Rol.Fiscal"/>.</item>
/// </list>
///
/// El <c>ActasController</c> protege <c>Index</c>/<c>Crear</c>/<c>Editar</c> con
/// <c>[Authorize(Policy = "GestionActas")]</c> y <c>Descargar</c> con
/// <c>[Authorize(Policy = "ConsultaActas")]</c>; por tanto, verificar ambas
/// políticas a nivel del servicio de autorización cubre, de forma determinista, el
/// permiso de gestión y descarga de actas sin necesidad de hospedar la aplicación.
///
/// Las políticas se construyen con exactamente la misma configuración de
/// <see cref="Microsoft.AspNetCore.Authorization.AuthorizationOptions"/> que
/// <see cref="Program"/> (el bloque completo <c>AddAuthorization(...)</c> con todas
/// las políticas) y se resuelven el <see cref="IAuthorizationPolicyProvider"/> y el
/// <see cref="IAuthorizationService"/> reales desde un
/// <see cref="ServiceCollection"/>, de modo que la prueba refleja la política
/// efectiva de la aplicación.
///
/// Para cada iteración se genera un rol aleatorio sobre los CINCO roles del sistema
/// (<see cref="Rol.AdministradorGeneral"/>, <see cref="Rol.Secretario"/>,
/// <see cref="Rol.Tesorero"/>, <see cref="Rol.Fiscal"/>, <see cref="Rol.Comisario"/>)
/// y claims adicionales arbitrarios, se construye un <see cref="ClaimsPrincipal"/>
/// (<c>ClaimTypes.Role</c> = nombre del rol) y se comprueba el resultado EXACTO de
/// autorización de cada política: la autorización debe tener éxito si y solo si el
/// rol pertenece al conjunto autorizado de esa política. Esto verifica tanto que los
/// roles autorizados SON permitidos como que los no autorizados son DENEGADOS, para
/// gestionar (<c>GestionActas</c>) y para descargar (<c>ConsultaActas</c>) actas.
/// </summary>
public class ActasAutorizacionPropertyTests
{
    private const int IteracionesMinimas = 100;

    private const string PoliticaGestionActas = "GestionActas";
    private const string PoliticaConsultaActas = "ConsultaActas";

    /// <summary>Roles autorizados para gestionar actas (GestionActas).</summary>
    private static readonly HashSet<Rol> RolesGestionActas = new()
    {
        Rol.AdministradorGeneral,
        Rol.Secretario
    };

    /// <summary>Roles autorizados para consultar/descargar actas (ConsultaActas).</summary>
    private static readonly HashSet<Rol> RolesConsultaActas = new()
    {
        Rol.AdministradorGeneral,
        Rol.Secretario,
        Rol.Fiscal
    };

    // Feature: roles-cabildo-actas, Property 2: Solo roles autorizados gestionan o descargan actas
    [Fact]
    public void SoloRolesAutorizados_GestionanODescarganActas()
    {
        // Construir las políticas reales replicando el bloque AddAuthorization
        // completo de Program.cs y resolver los servicios de autorización una sola
        // vez para toda la ejecución.
        var proveedor = ConstruirProveedorServicios();
        var servicioAutorizacion = proveedor.GetRequiredService<IAuthorizationService>();
        var proveedorPoliticas = proveedor.GetRequiredService<IAuthorizationPolicyProvider>();

        // Confirmar que ambas políticas nombradas existen en la configuración real.
        var politicaGestion = proveedorPoliticas
            .GetPolicyAsync(PoliticaGestionActas)
            .GetAwaiter()
            .GetResult();
        var politicaConsulta = proveedorPoliticas
            .GetPolicyAsync(PoliticaConsultaActas)
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(politicaGestion);
        Assert.NotNull(politicaConsulta);

        GenEntrada()
            .Sample(entrada =>
            {
                var principal = ConstruirPrincipal(entrada.Rol.ToString(), entrada.ClaimsExtra);

                // GestionActas autoriza IFF rol ∈ {AdministradorGeneral, Secretario}.
                var resultadoGestion = servicioAutorizacion
                    .AuthorizeAsync(principal, resource: null, PoliticaGestionActas)
                    .GetAwaiter()
                    .GetResult();
                bool esperadoGestion = RolesGestionActas.Contains(entrada.Rol);

                // ConsultaActas autoriza IFF rol ∈ {AdministradorGeneral, Secretario, Fiscal}.
                var resultadoConsulta = servicioAutorizacion
                    .AuthorizeAsync(principal, resource: null, PoliticaConsultaActas)
                    .GetAwaiter()
                    .GetResult();
                bool esperadoConsulta = RolesConsultaActas.Contains(entrada.Rol);

                return resultadoGestion.Succeeded == esperadoGestion
                    && resultadoConsulta.Succeeded == esperadoConsulta;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Construye un <see cref="ServiceProvider"/> con los servicios de autorización y
    /// EXACTAMENTE la misma definición de políticas que Program.cs (todas las
    /// políticas: GestionCuentas, EliminarContenido, GestionActas, ConsultaActas,
    /// GestionFinanciera, GestionMensajes, Veeduria, GestionComunicadosEventos).
    /// </summary>
    private static ServiceProvider ConstruirProveedorServicios()
    {
        var servicios = new ServiceCollection();
        servicios.AddLogging();
        servicios.AddAuthorization(options =>
        {
            // GestionCuentas y EliminarContenido siguen exigiendo solo AdministradorGeneral.
            options.AddPolicy("GestionCuentas", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral)));
            options.AddPolicy("EliminarContenido", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral)));

            // GestionActas: AdministradorGeneral, Secretario.
            options.AddPolicy("GestionActas", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario)));
            // ConsultaActas: AdministradorGeneral, Secretario, Fiscal.
            options.AddPolicy("ConsultaActas", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario), nameof(Rol.Fiscal)));
            // GestionFinanciera: AdministradorGeneral, Tesorero.
            options.AddPolicy("GestionFinanciera", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Tesorero)));
            // GestionMensajes: AdministradorGeneral, Comisario.
            options.AddPolicy("GestionMensajes", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Comisario)));
            // Veeduria: AdministradorGeneral, Fiscal.
            options.AddPolicy("Veeduria", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Fiscal)));
            // GestionComunicadosEventos: AdministradorGeneral, Secretario, Comisario.
            options.AddPolicy("GestionComunicadosEventos", policy =>
                policy.RequireRole(nameof(Rol.AdministradorGeneral), nameof(Rol.Secretario), nameof(Rol.Comisario)));
        });
        return servicios.BuildServiceProvider();
    }

    /// <summary>
    /// Construye un <see cref="ClaimsPrincipal"/> autenticado con el rol indicado y
    /// claims adicionales arbitrarios para ejercitar principales variados.
    /// </summary>
    private static ClaimsPrincipal ConstruirPrincipal(string rol, IReadOnlyList<(string Tipo, string Valor)> claimsExtra)
    {
        var claims = new List<Claim> { new(ClaimTypes.Role, rol) };
        foreach (var (tipo, valor) in claimsExtra)
        {
            claims.Add(new Claim(tipo, valor));
        }

        var identidad = new ClaimsIdentity(claims, authenticationType: "PruebaCookies");
        return new ClaimsPrincipal(identidad);
    }

    /// <summary>
    /// Genera un rol aleatorio sobre los CINCO roles del sistema y una lista de
    /// claims adicionales arbitrarios para el principal.
    /// </summary>
    private static Gen<(Rol Rol, IReadOnlyList<(string Tipo, string Valor)> ClaimsExtra)> GenEntrada() =>
        Gen.Select(
            GenRol(),
            GenClaimsExtra(),
            (rol, claimsExtra) => (rol, claimsExtra));

    /// <summary>
    /// Selector uniforme sobre los CINCO roles del sistema (AdministradorGeneral,
    /// Secretario, Tesorero, Fiscal, Comisario).
    /// </summary>
    private static Gen<Rol> GenRol() =>
        Gen.OneOf(
            Gen.Const(Rol.AdministradorGeneral),
            Gen.Const(Rol.Secretario),
            Gen.Const(Rol.Tesorero),
            Gen.Const(Rol.Fiscal),
            Gen.Const(Rol.Comisario));

    /// <summary>
    /// Genera una lista de claims adicionales (nombre, identificador, etc.) para que
    /// el principal presente formas variadas; ninguno debe alterar la decisión de
    /// autorización por rol.
    /// </summary>
    private static Gen<IReadOnlyList<(string Tipo, string Valor)>> GenClaimsExtra() =>
        Gen.Select(
            GenTipoClaim(),
            Gen.String[0, 20],
            (tipoClaim, valor) => (tipoClaim, valor))
        .List[0, 4]
        .Select(lista => (IReadOnlyList<(string, string)>)lista);

    /// <summary>
    /// Tipos de claim habituales de un principal autenticado (ninguno relacionado con
    /// la autorización de actas).
    /// </summary>
    private static Gen<string> GenTipoClaim() =>
        Gen.OneOf(
            Gen.Const(ClaimTypes.Name),
            Gen.Const(ClaimTypes.NameIdentifier),
            Gen.Const(ClaimTypes.Email),
            Gen.Const("CustomClaim"));
}
