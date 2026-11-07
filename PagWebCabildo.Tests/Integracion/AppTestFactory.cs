using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Tests.Integracion;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> reutilizable para las pruebas de
/// integración de extremo a extremo (tareas 14.1 y 14.2). Levanta la aplicación real
/// (<c>Program</c>) pero sustituye la base de datos SQLite basada en archivo
/// (<c>app.db</c>) por una base SQLite en memoria privada por fábrica, de modo que las
/// pruebas nunca tocan el almacén de la aplicación.
///
/// <para>
/// La conexión en memoria se mantiene abierta durante toda la vida de la fábrica: así
/// el esquema y los datos sembrados persisten entre las múltiples peticiones HTTP de
/// una prueba. El arranque de la aplicación aplica las migraciones sobre esa conexión
/// (<c>db.Database.Migrate()</c> en <c>Program.cs</c>), lo que crea el esquema y siembra
/// los registros singleton y el administrador inicial ("admin" / "Admin123*",
/// <see cref="Rol.AdministradorGeneral"/>) definidos en las migraciones de <c>HasData</c>.
/// Esta fábrica añade además un Secretario conocido ("editor" / "Editor123*") y un elemento
/// de contenido de ejemplo (una <see cref="Autoridad"/>) útil para las pruebas de borrado.
/// </para>
/// </summary>
public sealed class AppTestFactory : WebApplicationFactory<Program>
{
    /// <summary>Credenciales del AdministradorGeneral sembrado por las migraciones.</summary>
    public const string AdminUsuario = "admin";
    public const string AdminContraseña = "Admin123*";

    /// <summary>
    /// Credenciales del Secretario (antiguo "Editor") sembrado por esta fábrica.
    /// Los nombres de constante se conservan para no romper las pruebas de
    /// integración existentes que las referencian.
    /// </summary>
    public const string EditorUsuario = "editor";
    public const string EditorContraseña = "Editor123*";

    /// <summary>
    /// Conexión SQLite en memoria mantenida abierta durante la vida de la fábrica.
    /// Mientras permanezca abierta, la base de datos conserva su contenido entre
    /// peticiones y contextos.
    /// </summary>
    private readonly SqliteConnection _conexion = new("DataSource=:memory:");

    /// <summary>Id de la autoridad de ejemplo sembrada (para pruebas de borrado).</summary>
    public int AutoridadEjemploId { get; private set; }

    public AppTestFactory()
    {
        _conexion.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(servicios =>
        {
            // Retirar el registro de DbContext (y su configuración de opciones) de la
            // aplicación, que apunta a app.db vía UseSqlite(cadena de conexión).
            servicios.RemoveAll<DbContextOptions<AppDbContext>>();
            servicios.RemoveAll<DbContextOptions>();
            var descriptorContext = servicios
                .FirstOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (descriptorContext is not null)
            {
                servicios.Remove(descriptorContext);
            }

            // Registrar el DbContext sobre la conexión en memoria mantenida abierta.
            servicios.AddDbContext<AppDbContext>(opt => opt.UseSqlite(_conexion));

            // Construir el proveedor, aplicar las migraciones (crea el esquema y
            // siembra el admin inicial) y añadir los datos de prueba.
            using var proveedor = servicios.BuildServiceProvider();
            using var scope = proveedor.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();
            AutoridadEjemploId = SembrarDatosDePrueba(db);
        });
    }

    /// <summary>
    /// Siembra un Secretario conocido y un elemento de contenido de ejemplo sobre el que
    /// probar el rechazo de borrado por parte de un Secretario. Devuelve el Id de la
    /// autoridad de ejemplo. Es idempotente: no duplica datos si ya existen.
    /// </summary>
    private static int SembrarDatosDePrueba(AppDbContext db)
    {
        var hasher = new PasswordHasher<Administrador>();

        if (!db.Administradores.Any(a => a.NombreUsuario == EditorUsuario))
        {
            var editor = new Administrador
            {
                Nombre = "Secretario de Prueba",
                NombreUsuario = EditorUsuario,
                Rol = Rol.Secretario,
                Activo = true
            };
            editor.HashContraseña = hasher.HashPassword(editor, EditorContraseña);
            db.Administradores.Add(editor);
            db.SaveChanges();
        }

        var autoridad = db.Autoridades.FirstOrDefault(a => a.Nombre == "Autoridad de Prueba");
        if (autoridad is null)
        {
            autoridad = new Autoridad
            {
                Nombre = "Autoridad de Prueba",
                Cargo = "Gobernador",
                TipoAutoridad = "Autoridad",
                Orden = 1
            };
            db.Autoridades.Add(autoridad);
            db.SaveChanges();
        }

        return autoridad.Id;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _conexion.Dispose();
        }

        base.Dispose(disposing);
    }
}
