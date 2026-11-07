using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Persistencia con EF Core + SQLite.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticación por cookies para el Panel_Admin (Requisito 18.1).
// El esquema de cookies redirige a la pantalla de login del área Admin cuando el
// usuario no está autenticado y a una pantalla de acceso denegado cuando lo está
// pero carece del rol exigido por la política.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";
        options.AccessDeniedPath = "/Admin/Account/AccesoDenegado";
    });

// Políticas de autorización basadas en rol (Requisitos 18.3, 23.2, 23.3).
// Ambas políticas exigen el rol "AdministradorGeneral". El valor del claim de rol
// sigue la convención Rol.ToString() (ver nameof(Rol.AdministradorGeneral)): la
// acción de login (tarea 10.1) debe emitir ClaimTypes.Role = administrador.Rol.ToString()
// para que estas políticas resuelvan correctamente.
builder.Services.AddAuthorization(options =>
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
    // GestionContenidoGeneral: contenido editorial de Contacto. Lo pueden gestionar
    // todos los roles EXCEPTO el Tesorero, cuyo ámbito se limita a los documentos
    // financieros y la lectura del resto.
    options.AddPolicy("GestionContenidoGeneral", policy =>
        policy.RequireRole(
            nameof(Rol.AdministradorGeneral),
            nameof(Rol.Secretario),
            nameof(Rol.Fiscal),
            nameof(Rol.Comisario)));
    // GestionContenidoEditorial: Autoridades y Galería. Como GestionContenidoGeneral
    // pero excluyendo también al Comisario, cuyo ámbito se centra en mensajes de
    // contacto, foro y convocatorias. Queda para AdministradorGeneral, Secretario y Fiscal.
    options.AddPolicy("GestionContenidoEditorial", policy =>
        policy.RequireRole(
            nameof(Rol.AdministradorGeneral),
            nameof(Rol.Secretario),
            nameof(Rol.Fiscal)));
    // GestionHistoria: edición del texto de presentación e historia. Para
    // AdministradorGeneral y Secretario (no la editan Fiscal, Tesorero ni Comisario).
    options.AddPolicy("GestionHistoria", policy =>
        policy.RequireRole(
            nameof(Rol.AdministradorGeneral),
            nameof(Rol.Secretario)));
    // GestionDocumentos: módulo de documentos. Todos los roles autenticados EXCEPTO
    // el Comisario (cuyo ámbito no incluye la gestión documental general).
    options.AddPolicy("GestionDocumentos", policy =>
        policy.RequireRole(
            nameof(Rol.AdministradorGeneral),
            nameof(Rol.Secretario),
            nameof(Rol.Tesorero),
            nameof(Rol.Fiscal)));
    // ModerarForo: moderación del foro comunitario (recetas e historias enviadas
    // por visitantes anónimos y sus comentarios). Lo gestionan el Administrador
    // General y el Comisario, que ya atiende el contenido entrante de la comunidad
    // (mensajes de contacto).
    options.AddPolicy("ModerarForo", policy =>
        policy.RequireRole(
            nameof(Rol.AdministradorGeneral),
            nameof(Rol.Comisario)));
});

// Registro de servicios de aplicación para inyección de dependencias (scoped).
// Comparten el ciclo de vida del AppDbContext (también scoped) por petición HTTP.
builder.Services.AddScoped<IPresentacionService, PresentacionService>();
builder.Services.AddScoped<IContactoService, ContactoService>();
builder.Services.AddScoped<IAutoridadService, AutoridadService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IComunicadoService, ComunicadoService>();
builder.Services.AddScoped<IGaleriaService, GaleriaService>();
builder.Services.AddScoped<IDocumentoService, DocumentoService>();
builder.Services.AddScoped<IMensajeService, MensajeService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAutenticacionService, AutenticacionService>();
builder.Services.AddScoped<IValidacionContactoService, ValidacionContactoService>();
builder.Services.AddScoped<IArchivoService, ArchivoService>();
builder.Services.AddScoped<IRegistroActividadService, RegistroActividadService>();
builder.Services.AddScoped<IActaService, ActaService>();
builder.Services.AddScoped<IForoService, ForoService>();

var app = builder.Build();

// Aplicar las migraciones pendientes y crear la base de datos al arranque.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Las actas son privadas (Requisito 4.3): su adjunto nunca debe servirse como
// archivo estático público. Se bloquea cualquier solicitud directa a
// /uploads/actas/** antes del middleware de archivos estáticos; la descarga
// legítima pasa solo por la acción protegida /Admin/Actas/Descargar.
app.Use(async (context, next) =>
{
    var ruta = context.Request.Path;
    // Las actas (Requisito 4.3) y los documentos de categorías privadas nunca se
    // sirven como archivos estáticos: su descarga pasa solo por acciones
    // controladas (Admin/Actas/Descargar y Admin/Documentos/DescargarPrivado).
    if (ruta.StartsWithSegments("/uploads/actas", StringComparison.OrdinalIgnoreCase) ||
        ruta.StartsWithSegments("/uploads/documentos-privados", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

// MapStaticAssets sirve los activos conocidos en tiempo de compilación.
// UseStaticFiles se añade para servir también los archivos subidos en tiempo de
// ejecución bajo wwwroot/uploads (Requisito Architecture - Program.cs).
app.MapStaticAssets();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rutas de Area (Panel_Admin) además de la ruta por defecto del sitio público.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

// Expone la clase Program generada por las instrucciones de nivel superior para que
// las pruebas de integración puedan referenciar `Program` con
// WebApplicationFactory<Program>. No altera el comportamiento de la aplicación.
public partial class Program { }
