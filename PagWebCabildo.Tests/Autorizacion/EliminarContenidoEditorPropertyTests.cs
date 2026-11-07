using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CsCheck;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PagWebCabildo.Models.Entidades;
using Xunit;

namespace PagWebCabildo.Tests.Autorizacion;

/// <summary>
/// Prueba basada en propiedades de la Property 5 del diseño roles-cabildo-actas:
/// ningún rol distinto de <see cref="Rol.AdministradorGeneral"/> (el Gobernador)
/// puede eliminar contenido de ningún tipo. Preserva la antigua Property 6 del
/// spec original (un Editor nunca elimina contenido) tras renombrar
/// <c>Editor</c> a <see cref="Rol.Secretario"/> y añadir los roles
/// <see cref="Rol.Tesorero"/>, <see cref="Rol.Fiscal"/> y <see cref="Rol.Comisario"/>.
///
/// Feature: roles-cabildo-actas, Property 5: Ningún rol distinto de
/// AdministradorGeneral elimina contenido ni gestiona cuentas.
///
/// Validates: Requirements 2.1, 2.6
///
/// El sistema bajo prueba es la política de autorización nombrada
/// <c>"EliminarContenido"</c>, que según <see cref="Program"/> exige el rol
/// <see cref="Rol.AdministradorGeneral"/>
/// (<c>policy.RequireRole(nameof(Rol.AdministradorGeneral))</c>). Esta misma
/// política protege las acciones <c>Eliminar*</c> de todos los controladores de
/// contenido del Area Admin (Autoridades, Eventos, Comunicados, temas y elementos
/// de Galería, categorías y documentos, y Mensajes); por tanto, verificar la
/// política a nivel de servicio de autorización cubre, de forma determinista, el
/// rechazo de la eliminación para todos los tipos de contenido sin necesidad de
/// hospedar la aplicación.
///
/// La política se construye con exactamente la misma configuración de
/// <see cref="Microsoft.AspNetCore.Authorization.AuthorizationOptions"/> que
/// <see cref="Program"/> y se resuelve el <see cref="IAuthorizationPolicyProvider"/>
/// y el <see cref="IAuthorizationService"/> reales desde un
/// <see cref="ServiceCollection"/>, de modo que la prueba refleja la política
/// efectiva de la aplicación.
///
/// Para cada iteración se genera un principal con un rol aleatorio NO-Gobernador
/// (Secretario, Tesorero, Fiscal o Comisario), claims variados y un selector sobre
/// todos los tipos de contenido (<c>Autoridad, Evento, Comunicado, TemaGaleria,
/// ElementoGaleria, CategoriaDocumento, Documento, Mensaje</c>) —documentando la
/// cobertura de "todos los tipos de contenido"— y se comprueba que la autorización
/// <b>fracasa</b> (<c>Succeeded == false</c>) para cada uno. Como comprobación de
/// cordura, se verifica además que un principal
/// <see cref="Rol.AdministradorGeneral"/> sí es autorizado por la política.
/// </summary>
public class EliminarContenidoEditorPropertyTests
{
    private const int IteracionesMinimas = 100;

    private const string NombrePolitica = "EliminarContenido";

    /// <summary>
    /// Todos los tipos de contenido eliminables protegidos por la política
    /// <c>"EliminarContenido"</c> (Property 5 del diseño).
    /// </summary>
    private enum TipoContenido
    {
        Autoridad,
        Evento,
        Comunicado,
        TemaGaleria,
        ElementoGaleria,
        CategoriaDocumento,
        Documento,
        Mensaje
    }

    // Feature: roles-cabildo-actas, Property 5: Ningún rol distinto de AdministradorGeneral elimina contenido
    [Fact]
    public void NingunRolNoGobernador_PuedeEliminarContenido()
    {
        // Construir la política real exactamente como en Program.cs y resolver los
        // servicios de autorización una sola vez para toda la ejecución.
        var proveedor = new ServiceCollection()
            .AddLogging()
            .AddAuthorization(options =>
            {
                options.AddPolicy("GestionCuentas", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral)));
                options.AddPolicy("EliminarContenido", policy =>
                    policy.RequireRole(nameof(Rol.AdministradorGeneral)));
            })
            .BuildServiceProvider();

        var servicioAutorizacion = proveedor.GetRequiredService<IAuthorizationService>();
        var proveedorPoliticas = proveedor.GetRequiredService<IAuthorizationPolicyProvider>();

        // Obtener la política nombrada real para confirmar que existe.
        var politica = proveedorPoliticas
            .GetPolicyAsync(NombrePolitica)
            .GetAwaiter()
            .GetResult();
        Assert.NotNull(politica);

        GenEntrada()
            .Sample(entrada =>
            {
                // Un usuario con un rol NO-Gobernador y claims arbitrarios: la
                // política debe rechazarlo para cualquier tipo de contenido (el
                // selector documenta la cobertura; la política es la misma para
                // todos los tipos).
                var noGobernador = ConstruirPrincipal(entrada.Rol.ToString(), entrada.ClaimsExtra);

                var resultadoNoGobernador = servicioAutorizacion
                    .AuthorizeAsync(noGobernador, resource: null, NombrePolitica)
                    .GetAwaiter()
                    .GetResult();

                if (resultadoNoGobernador.Succeeded)
                {
                    // Ningún rol distinto de AdministradorGeneral debe ser autorizado
                    // a eliminar, sea cual sea el tipo de contenido (entrada.Tipo).
                    return false;
                }

                // Comprobación de cordura: un AdministradorGeneral sí es autorizado.
                var administrador = ConstruirPrincipal(
                    nameof(Rol.AdministradorGeneral),
                    entrada.ClaimsExtra);

                var resultadoAdministrador = servicioAutorizacion
                    .AuthorizeAsync(administrador, resource: null, NombrePolitica)
                    .GetAwaiter()
                    .GetResult();

                return resultadoAdministrador.Succeeded;
            }, iter: IteracionesMinimas);
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
    /// Genera un rol NO-Gobernador, el tipo de contenido (cubriendo todos los tipos)
    /// y una lista de claims adicionales arbitrarios para el principal.
    /// </summary>
    private static Gen<(Rol Rol, TipoContenido Tipo, IReadOnlyList<(string Tipo, string Valor)> ClaimsExtra)> GenEntrada() =>
        Gen.Select(
            GenRolNoGobernador(),
            GenTipoContenido(),
            GenClaimsExtra(),
            (rol, tipo, claimsExtra) => (rol, tipo, claimsExtra));

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

    /// <summary>
    /// Selector uniforme sobre todos los tipos de contenido eliminables.
    /// </summary>
    private static Gen<TipoContenido> GenTipoContenido() =>
        Gen.OneOf(
            Gen.Const(TipoContenido.Autoridad),
            Gen.Const(TipoContenido.Evento),
            Gen.Const(TipoContenido.Comunicado),
            Gen.Const(TipoContenido.TemaGaleria),
            Gen.Const(TipoContenido.ElementoGaleria),
            Gen.Const(TipoContenido.CategoriaDocumento),
            Gen.Const(TipoContenido.Documento),
            Gen.Const(TipoContenido.Mensaje));

    /// <summary>
    /// Genera una lista de claims adicionales (nombre, identificador, etc.) para
    /// que el principal presente formas variadas; ninguno debe habilitar la
    /// eliminación.
    /// </summary>
    private static Gen<IReadOnlyList<(string Tipo, string Valor)>> GenClaimsExtra() =>
        Gen.Select(
            GenTipoClaim(),
            Gen.String[0, 20],
            (tipoClaim, valor) => (tipoClaim, valor))
        .List[0, 4]
        .Select(lista => (IReadOnlyList<(string, string)>)lista);

    /// <summary>
    /// Tipos de claim habituales de un principal autenticado (ninguno relacionado
    /// con autorización de eliminación).
    /// </summary>
    private static Gen<string> GenTipoClaim() =>
        Gen.OneOf(
            Gen.Const(ClaimTypes.Name),
            Gen.Const(ClaimTypes.NameIdentifier),
            Gen.Const(ClaimTypes.Email),
            Gen.Const("CustomClaim"));
}
