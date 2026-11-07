using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Pruebas unitarias basadas en ejemplos (no en propiedades) de
/// <see cref="ActaService"/> (implementación de <see cref="IActaService"/>).
/// Cubren las validaciones de creación/edición del acta, la coordinación del
/// adjunto a través de <see cref="IArchivoService"/> y el orden del listado
/// (más reciente primero):
///
/// <list type="bullet">
/// <item>Crear con datos y archivo válidos persiste el acta, fija la ruta del
/// adjunto y la fecha de registro, y asigna un Id (Requisitos 3.1, 3.4).</item>
/// <item>Crear valida consecutivo positivo y único, título obligatorio y la
/// presencia del archivo (Requisito 3.2).</item>
/// <item>Editar reemplaza o conserva el adjunto según corresponda y devuelve
/// null para un Id inexistente (Requisito 4.1).</item>
/// <item>Listar devuelve las actas ordenadas por fecha de asamblea descendente
/// con desempate por Id descendente (Requisito 4.1 / 3.1).</item>
/// </list>
///
/// Cada prueba usa una base SQLite en memoria recién creada
/// (<see cref="SqliteEnMemoria"/>) con contextos independientes para escribir y
/// releer, de modo que las aserciones observen datos efectivamente persistidos.
/// Se emplea un doble de prueba de <see cref="IArchivoService"/>
/// (<see cref="ArchivoServiceFalso"/>) que no toca el disco.
///
/// Validates: Requirements 3.2, 4.1, 5.1
/// </summary>
public sealed class ActaServiceTests
{
    // -----------------------------------------------------------------------
    // Requisito 3.1 / 3.4: Crear un acta válida persiste y completa metadatos.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Crear_ConDatosYArchivoValidos_PersisteAsignaIdRutaYFechaRegistro()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();

        // Sembrar un administrador válido para satisfacer la FK RegistradoPorId
        // (relación ON DELETE RESTRICT hacia Administrador).
        int adminId = await SembrarAdministrador(bd);

        int actaId;
        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxEscritura, archivos);
            var archivo = CrearArchivo("acta-01.pdf", "contenido del acta");

            var creada = await servicio.Crear(new Acta
            {
                Consecutivo = 1,
                FechaAsamblea = new DateOnly(2025, 3, 10),
                Tipo = TipoAsamblea.Ordinaria,
                Titulo = "Acta de asamblea ordinaria",
                Descripcion = "Decisiones de la comunidad.",
                RegistradoPorId = adminId
            }, archivo);

            // Se asignó un Id, la ruta del adjunto (del doble) y la fecha de registro.
            Assert.True(creada.Id > 0);
            Assert.StartsWith("uploads/actas/", creada.ArchivoRuta);
            Assert.NotEqual(default, creada.FechaRegistro);
            actaId = creada.Id;
        }

        // Releer con contexto independiente y confirmar la persistencia.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxLectura, archivos);
            var leida = await servicio.Obtener(actaId);
            Assert.NotNull(leida);
            Assert.Equal(1, leida!.Consecutivo);
            Assert.Equal(new DateOnly(2025, 3, 10), leida.FechaAsamblea);
            Assert.Equal("Acta de asamblea ordinaria", leida.Titulo);
            Assert.StartsWith("uploads/actas/", leida.ArchivoRuta);
            Assert.Equal(1, ctxLectura.Actas.Count());
        }

        // El doble de archivos registró exactamente un guardado.
        Assert.Single(archivos.RutasGuardadas);
    }

    // -----------------------------------------------------------------------
    // Requisito 3.2: Validaciones de creación.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Crear_ConsecutivoNoPositivo_LanzaArgumentExceptionYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new ActaService(ctx, archivos);
        var archivo = CrearArchivo("acta.pdf", "contenido");

        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.Crear(ActaBase(adminId, consecutivo: 0), archivo));

        Assert.Contains("consecutivo", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, ctx.Actas.Count());
        // No debió guardarse ningún archivo si la validación falla antes.
        Assert.Empty(archivos.RutasGuardadas);
    }

    [Fact]
    public async Task Crear_TituloEnBlanco_LanzaArgumentExceptionYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new ActaService(ctx, archivos);
        var archivo = CrearArchivo("acta.pdf", "contenido");

        var acta = ActaBase(adminId);
        acta.Titulo = "   ";

        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.Crear(acta, archivo));

        Assert.Contains("título", excepcion.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, ctx.Actas.Count());
    }

    [Fact]
    public async Task Crear_ConsecutivoDuplicado_LanzaArgumentExceptionYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        // Primera acta con consecutivo 5.
        using (var ctxPrimera = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxPrimera, archivos);
            await servicio.Crear(ActaBase(adminId, consecutivo: 5), CrearArchivo("a5.pdf", "x"));
        }

        // Segunda acta con el mismo consecutivo debe rechazarse.
        using (var ctxSegunda = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxSegunda, archivos);
            var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
                servicio.Crear(ActaBase(adminId, consecutivo: 5), CrearArchivo("a5-bis.pdf", "y")));

            Assert.Contains("5", excepcion.Message);
            // Sigue existiendo una sola acta con ese consecutivo.
            Assert.Equal(1, ctxSegunda.Actas.Count(a => a.Consecutivo == 5));
        }
    }

    [Fact]
    public async Task Crear_SinArchivo_LanzaArgumentExceptionYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new ActaService(ctx, archivos);

        // Archivo nulo.
        await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.Crear(ActaBase(adminId), archivo: null!));

        // Archivo vacío (longitud 0).
        var archivoVacio = CrearArchivo("vacio.pdf", string.Empty);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.Crear(ActaBase(adminId), archivoVacio));

        Assert.Equal(0, ctx.Actas.Count());
    }

    // -----------------------------------------------------------------------
    // Requisito 4.1: Editar — reemplazo y conservación del adjunto; Id inexistente.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Editar_ConNuevoArchivo_ReemplazaRutaYEliminaAnterior()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        int actaId;
        string rutaOriginal;

        // Crear el acta con su adjunto inicial.
        using (var ctxCrear = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxCrear, archivos);
            var creada = await servicio.Crear(ActaBase(adminId, consecutivo: 7), CrearArchivo("original.pdf", "v1"));
            actaId = creada.Id;
            rutaOriginal = creada.ArchivoRuta;
        }

        // Editar con un archivo nuevo: debe cambiar la ruta y eliminar la anterior.
        using (var ctxEditar = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxEditar, archivos);
            var editada = await servicio.Editar(new Acta
            {
                Id = actaId,
                Consecutivo = 7,
                FechaAsamblea = new DateOnly(2025, 4, 1),
                Tipo = TipoAsamblea.Extraordinaria,
                Titulo = "Acta editada",
                Descripcion = "Nueva descripción.",
                RegistradoPorId = adminId
            }, CrearArchivo("nuevo.pdf", "v2"));

            Assert.NotNull(editada);
            Assert.NotEqual(rutaOriginal, editada!.ArchivoRuta);
            Assert.StartsWith("uploads/actas/", editada.ArchivoRuta);
        }

        // El doble registró la eliminación del adjunto anterior.
        Assert.Contains(rutaOriginal, archivos.RutasEliminadas);

        // El nuevo adjunto quedó persistido.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxLectura, archivos);
            var leida = await servicio.Obtener(actaId);
            Assert.NotNull(leida);
            Assert.NotEqual(rutaOriginal, leida!.ArchivoRuta);
            Assert.Equal("Acta editada", leida.Titulo);
            Assert.Equal(TipoAsamblea.Extraordinaria, leida.Tipo);
        }
    }

    [Fact]
    public async Task Editar_SinArchivo_ConservaRutaExistente()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        int actaId;
        string rutaOriginal;

        using (var ctxCrear = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxCrear, archivos);
            var creada = await servicio.Crear(ActaBase(adminId, consecutivo: 9), CrearArchivo("doc.pdf", "v1"));
            actaId = creada.Id;
            rutaOriginal = creada.ArchivoRuta;
        }

        // Editar sin archivo (null): debe conservarse la ruta original.
        using (var ctxEditar = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxEditar, archivos);
            var editada = await servicio.Editar(new Acta
            {
                Id = actaId,
                Consecutivo = 9,
                FechaAsamblea = new DateOnly(2025, 5, 2),
                Tipo = TipoAsamblea.Ordinaria,
                Titulo = "Solo datos, sin cambiar adjunto",
                Descripcion = "Descripción.",
                RegistradoPorId = adminId
            }, archivo: null);

            Assert.NotNull(editada);
            Assert.Equal(rutaOriginal, editada!.ArchivoRuta);
        }

        // No se eliminó ningún archivo al conservar el adjunto.
        Assert.Empty(archivos.RutasEliminadas);

        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxLectura, archivos);
            var leida = await servicio.Obtener(actaId);
            Assert.NotNull(leida);
            Assert.Equal(rutaOriginal, leida!.ArchivoRuta);
            Assert.Equal("Solo datos, sin cambiar adjunto", leida.Titulo);
        }
    }

    [Fact]
    public async Task Editar_ConIdInexistente_DevuelveNull()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        using var ctx = bd.CrearContexto();
        var servicio = new ActaService(ctx, archivos);

        var resultado = await servicio.Editar(new Acta
        {
            Id = 9999,
            Consecutivo = 1,
            FechaAsamblea = new DateOnly(2025, 1, 1),
            Tipo = TipoAsamblea.Ordinaria,
            Titulo = "No existe",
            RegistradoPorId = adminId
        }, archivo: null);

        Assert.Null(resultado);
    }

    // -----------------------------------------------------------------------
    // Requisito 4.1 / 3.1: Listar en orden "más reciente primero".
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Listar_DevuelveActasOrdenadasPorFechaAsambleaDescYLuegoIdDesc()
    {
        using var bd = new SqliteEnMemoria();
        var archivos = new ArchivoServiceFalso();
        int adminId = await SembrarAdministrador(bd);

        // Sembrar varias actas: dos comparten fecha para ejercitar el desempate
        // por Id descendente.
        using (var ctxCrear = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxCrear, archivos);
            await servicio.Crear(ActaBase(adminId, consecutivo: 1, fecha: new DateOnly(2025, 1, 10)), CrearArchivo("1.pdf", "a"));
            await servicio.Crear(ActaBase(adminId, consecutivo: 2, fecha: new DateOnly(2025, 6, 20)), CrearArchivo("2.pdf", "b"));
            await servicio.Crear(ActaBase(adminId, consecutivo: 3, fecha: new DateOnly(2025, 6, 20)), CrearArchivo("3.pdf", "c"));
            await servicio.Crear(ActaBase(adminId, consecutivo: 4, fecha: new DateOnly(2025, 3, 5)), CrearArchivo("4.pdf", "d"));
        }

        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ActaService(ctxLectura, archivos);
            var listado = await servicio.Listar();

            // Orden esperado por consecutivo: 3 y 2 (misma fecha 2025-06-20, Id desc),
            // luego 4 (2025-03-05) y por último 1 (2025-01-10).
            var fechas = listado.Select(a => a.FechaAsamblea).ToList();
            Assert.True(EstaOrdenadaDescendente(fechas), "Las actas deben estar ordenadas por fecha de asamblea descendente.");

            // Las dos actas con la misma fecha deben ir de Id mayor a menor.
            var mismasFecha = listado.Where(a => a.FechaAsamblea == new DateOnly(2025, 6, 20)).ToList();
            Assert.Equal(2, mismasFecha.Count);
            Assert.True(mismasFecha[0].Id > mismasFecha[1].Id, "Ante igual fecha, el desempate es por Id descendente.");

            // La primera del listado es una de las de fecha más reciente (2025-06-20).
            Assert.Equal(new DateOnly(2025, 6, 20), listado[0].FechaAsamblea);
            // La última es la de fecha más antigua (2025-01-10).
            Assert.Equal(new DateOnly(2025, 1, 10), listado[^1].FechaAsamblea);
        }
    }

    // -----------------------------------------------------------------------
    // Utilidades de armado.
    // -----------------------------------------------------------------------

    /// <summary>Siembra un administrador válido y devuelve su Id asignado por EF.</summary>
    private static async Task<int> SembrarAdministrador(SqliteEnMemoria bd)
    {
        using var ctx = bd.CrearContexto();
        var admin = new Administrador
        {
            Nombre = "Secretaria del Cabildo",
            NombreUsuario = "secretaria",
            HashContraseña = "hash",
            Rol = Rol.Secretario,
            Activo = true
        };
        ctx.Administradores.Add(admin);
        await ctx.SaveChangesAsync();
        return admin.Id;
    }

    /// <summary>Construye un acta válida de base, parametrizando consecutivo y fecha.</summary>
    private static Acta ActaBase(int adminId, int consecutivo = 1, DateOnly? fecha = null) => new()
    {
        Consecutivo = consecutivo,
        FechaAsamblea = fecha ?? new DateOnly(2025, 2, 15),
        Tipo = TipoAsamblea.Ordinaria,
        Titulo = $"Acta {consecutivo}",
        Descripcion = "Descripción del acta.",
        RegistradoPorId = adminId
    };

    /// <summary>Construye un <see cref="IFormFile"/> en memoria con el nombre y contenido dados.</summary>
    private static IFormFile CrearArchivo(string nombre, string contenido)
    {
        var bytes = Encoding.UTF8.GetBytes(contenido);
        var flujo = new MemoryStream(bytes);
        return new FormFile(flujo, 0, flujo.Length, "archivo", nombre);
    }

    /// <summary>Indica si una secuencia de fechas está en orden descendente (no estricto).</summary>
    private static bool EstaOrdenadaDescendente(System.Collections.Generic.IReadOnlyList<DateOnly> fechas)
    {
        for (int i = 1; i < fechas.Count; i++)
        {
            if (fechas[i] > fechas[i - 1])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Doble de prueba de <see cref="IArchivoService"/> que no toca el disco.
    /// <see cref="Guardar"/> devuelve una ruta relativa determinista bajo
    /// <c>uploads/actas</c>; <see cref="Eliminar"/> registra la ruta y devuelve
    /// <c>true</c>; <see cref="Abrir"/> devuelve un flujo en memoria de relleno.
    /// Expone las rutas guardadas y eliminadas para su verificación.
    /// </summary>
    private sealed class ArchivoServiceFalso : IArchivoService
    {
        public System.Collections.Generic.List<string> RutasGuardadas { get; } = new();
        public System.Collections.Generic.List<string> RutasEliminadas { get; } = new();

        public Task<string> Guardar(IFormFile archivo, string subcarpeta)
        {
            var extension = Path.GetExtension(archivo.FileName);
            var ruta = $"uploads/{subcarpeta}/test-{Guid.NewGuid():N}{extension}";
            RutasGuardadas.Add(ruta);
            return Task.FromResult(ruta);
        }

        public bool Eliminar(string rutaRelativa)
        {
            RutasEliminadas.Add(rutaRelativa);
            return true;
        }

        public ArchivoAbierto Abrir(string rutaRelativa)
        {
            var flujo = new MemoryStream(Encoding.UTF8.GetBytes("contenido-de-prueba"));
            var nombre = Path.GetFileName(rutaRelativa);
            return new ArchivoAbierto(flujo, "application/pdf", nombre);
        }
    }
}
