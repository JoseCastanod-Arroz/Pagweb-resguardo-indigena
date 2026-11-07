using System;
using System.Linq;
using System.Threading.Tasks;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Pruebas unitarias basadas en ejemplos (no en propiedades) que complementan a las
/// pruebas basadas en propiedades de la capa de servicios de contenido. Cubren casos
/// concretos y límites de los servicios singleton y de marcado de estado:
///
/// <list type="bullet">
/// <item>Guardar y releer el texto de presentación (Requisito 24).</item>
/// <item>Guardar y releer la información de contacto (Requisito 40).</item>
/// <item>Marcar un evento como cancelado y releerlo (Requisito 29).</item>
/// <item>Marcar un mensaje como atendido y releerlo (Requisito 42).</item>
/// <item>Mensajes de error al guardar un borrador incompleto y guardado válido
/// (Requisito 32.2).</item>
/// </list>
///
/// Cada prueba usa una base SQLite en memoria recién creada
/// (<see cref="SqliteEnMemoria"/>) y construye los servicios con contextos obtenidos
/// de <see cref="SqliteEnMemoria.CrearContexto"/>, usando un contexto para escribir y
/// otro independiente para releer, de modo que las aserciones observen datos
/// efectivamente persistidos.
///
/// Validates: Requirements 24, 29, 32.2, 40, 42
/// </summary>
public sealed class ServiciosCasosLimiteTests
{
    // -----------------------------------------------------------------------
    // Requisito 24: Presentación (singleton) — guardar y releer.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PresentacionService_ActualizarYObtener_DevuelveTextoActualizado()
    {
        using var bd = new SqliteEnMemoria();

        const string nuevoTexto = "Historia del Resguardo Indígena Rincón Bodega.";

        // Actualizar sobre un contexto de escritura.
        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new PresentacionService(ctxEscritura);
            var actualizada = await servicio.ActualizarPresentacion(nuevoTexto);
            Assert.Equal(nuevoTexto, actualizada.Texto);
        }

        // Releer sobre un contexto independiente para confirmar la persistencia.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new PresentacionService(ctxLectura);
            var leida = await servicio.ObtenerPresentacion();
            Assert.Equal(nuevoTexto, leida.Texto);

            // Se mantiene el carácter singleton: sigue existiendo un único registro.
            Assert.Equal(1, ctxLectura.Presentaciones.Count());
        }
    }

    [Fact]
    public async Task PresentacionService_ActualizarConNulo_GuardaCadenaVacia()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new PresentacionService(ctxEscritura);
            await servicio.ActualizarPresentacion(null!);
        }

        using var ctxLectura = bd.CrearContexto();
        var servicioLectura = new PresentacionService(ctxLectura);
        var leida = await servicioLectura.ObtenerPresentacion();
        Assert.Equal(string.Empty, leida.Texto);
    }

    // -----------------------------------------------------------------------
    // Requisito 40: Información de contacto (singleton) — guardar y releer.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ContactoService_ActualizarYObtener_DevuelveValoresActualizados()
    {
        using var bd = new SqliteEnMemoria();

        const string direccion = "Calle 10 # 5-20, Rincón Bodega";
        const string telefono = "3123456789";
        const string correo = "contacto@resguardo.org";
        const string otros = "Atención de lunes a viernes.";

        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new ContactoService(ctxEscritura);
            var actualizada = await servicio.ActualizarInformacion(direccion, telefono, correo, otros);
            Assert.Equal(direccion, actualizada.Direccion);
            Assert.Equal(telefono, actualizada.Telefono);
            Assert.Equal(correo, actualizada.CorreoElectronico);
            Assert.Equal(otros, actualizada.OtrosDatos);
        }

        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ContactoService(ctxLectura);
            var leida = await servicio.ObtenerInformacion();
            Assert.Equal(direccion, leida.Direccion);
            Assert.Equal(telefono, leida.Telefono);
            Assert.Equal(correo, leida.CorreoElectronico);
            Assert.Equal(otros, leida.OtrosDatos);

            // Se mantiene el carácter singleton: un único registro.
            Assert.Equal(1, ctxLectura.InformacionesContacto.Count());
        }
    }

    [Fact]
    public async Task ContactoService_ActualizarConNulos_GuardaCadenasVacias()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new ContactoService(ctxEscritura);
            await servicio.ActualizarInformacion(null!, null!, null!, null!);
        }

        using var ctxLectura = bd.CrearContexto();
        var servicioLectura = new ContactoService(ctxLectura);
        var leida = await servicioLectura.ObtenerInformacion();
        Assert.Equal(string.Empty, leida.Direccion);
        Assert.Equal(string.Empty, leida.Telefono);
        Assert.Equal(string.Empty, leida.CorreoElectronico);
        Assert.Equal(string.Empty, leida.OtrosDatos);
    }

    // -----------------------------------------------------------------------
    // Requisito 29: Marcar evento como cancelado.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EventoService_MarcarCancelado_PersisteCanceladoVerdadero()
    {
        using var bd = new SqliteEnMemoria();

        int eventoId;

        // Crear un evento no cancelado.
        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new EventoService(ctxEscritura);
            var creado = await servicio.Crear(new Evento
            {
                Titulo = "Asamblea comunitaria",
                Fecha = new DateOnly(2025, 6, 15),
                Hora = new TimeOnly(9, 0),
                Lugar = "Casa del cabildo",
                Descripcion = "Reunión general.",
                Cancelado = false
            });
            eventoId = creado.Id;
            Assert.False(creado.Cancelado);
        }

        // Marcar cancelado.
        using (var ctxCancelar = bd.CrearContexto())
        {
            var servicio = new EventoService(ctxCancelar);
            var cancelado = await servicio.MarcarCancelado(eventoId);
            Assert.NotNull(cancelado);
            Assert.True(cancelado!.Cancelado);
        }

        // Releer con contexto independiente y verificar persistencia.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new EventoService(ctxLectura);
            var leido = await servicio.Obtener(eventoId);
            Assert.NotNull(leido);
            Assert.True(leido!.Cancelado);
        }
    }

    [Fact]
    public async Task EventoService_MarcarCancelado_ConIdInexistente_DevuelveNull()
    {
        using var bd = new SqliteEnMemoria();

        using var ctx = bd.CrearContexto();
        var servicio = new EventoService(ctx);

        var resultado = await servicio.MarcarCancelado(9999);
        Assert.Null(resultado);
    }

    // -----------------------------------------------------------------------
    // Requisito 42: Marcar mensaje como atendido.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task MensajeService_MarcarAtendido_PersisteAtendidoVerdadero()
    {
        using var bd = new SqliteEnMemoria();
        var validacion = new ValidacionContactoService();

        int mensajeId;

        // Sembrar un mensaje no atendido directamente en la base.
        using (var ctxSemilla = bd.CrearContexto())
        {
            var mensaje = new Mensaje
            {
                Nombre = "Visitante",
                MedioContacto = "3123456789",
                Asunto = "Consulta",
                Contenido = "Deseo información sobre la comunidad.",
                FechaRecepcion = new DateTime(2025, 3, 1, 10, 0, 0),
                Atendido = false
            };
            ctxSemilla.Mensajes.Add(mensaje);
            await ctxSemilla.SaveChangesAsync();
            mensajeId = mensaje.Id;
        }

        // Marcar atendido.
        using (var ctxMarcar = bd.CrearContexto())
        {
            var servicio = new MensajeService(ctxMarcar, validacion);
            var atendido = await servicio.MarcarAtendido(mensajeId);
            Assert.NotNull(atendido);
            Assert.True(atendido!.Atendido);
        }

        // Releer con contexto independiente.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new MensajeService(ctxLectura, validacion);
            var leido = await servicio.Ver(mensajeId);
            Assert.NotNull(leido);
            Assert.True(leido!.Atendido);
        }
    }

    [Fact]
    public async Task MensajeService_MarcarAtendido_ConIdInexistente_DevuelveNull()
    {
        using var bd = new SqliteEnMemoria();
        var validacion = new ValidacionContactoService();

        using var ctx = bd.CrearContexto();
        var servicio = new MensajeService(ctx, validacion);

        var resultado = await servicio.MarcarAtendido(9999);
        Assert.Null(resultado);
    }

    // -----------------------------------------------------------------------
    // Requisito 32.2: Validación de borrador de comunicado.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ComunicadoService_GuardarBorrador_TituloVacio_LanzaYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();

        using var ctx = bd.CrearContexto();
        var servicio = new ComunicadoService(ctx);

        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.GuardarBorrador(null, "   ", DateTime.Now, "Contenido válido."));

        // El mensaje debe indicar el campo faltante (Título).
        Assert.Contains("Título", excepcion.Message);

        // No se persistió ningún comunicado.
        Assert.Equal(0, ctx.Comunicados.Count());
    }

    [Fact]
    public async Task ComunicadoService_GuardarBorrador_ContenidoVacio_LanzaYNoPersiste()
    {
        using var bd = new SqliteEnMemoria();

        using var ctx = bd.CrearContexto();
        var servicio = new ComunicadoService(ctx);

        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.GuardarBorrador(null, "Título válido", DateTime.Now, "   "));

        Assert.Contains("Contenido", excepcion.Message);
        Assert.Equal(0, ctx.Comunicados.Count());
    }

    [Fact]
    public async Task ComunicadoService_GuardarBorrador_TituloYContenidoVacios_MensajeEnumeraAmbos()
    {
        using var bd = new SqliteEnMemoria();

        using var ctx = bd.CrearContexto();
        var servicio = new ComunicadoService(ctx);

        var excepcion = await Assert.ThrowsAsync<ArgumentException>(() =>
            servicio.GuardarBorrador(null, "", DateTime.Now, ""));

        // El mensaje debe enumerar ambos campos obligatorios faltantes.
        Assert.Contains("Título", excepcion.Message);
        Assert.Contains("Contenido", excepcion.Message);
        Assert.Equal(0, ctx.Comunicados.Count());
    }

    [Fact]
    public async Task ComunicadoService_GuardarBorrador_Valido_PersisteComoBorrador()
    {
        using var bd = new SqliteEnMemoria();

        const string titulo = "Convocatoria a asamblea";
        const string contenido = "Se convoca a la comunidad a la asamblea general.";
        var fecha = new DateTime(2025, 5, 20);

        int borradorId;

        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new ComunicadoService(ctxEscritura);
            var borrador = await servicio.GuardarBorrador(null, titulo, fecha, contenido);

            Assert.Equal(EstadoComunicado.Borrador, borrador.Estado);
            Assert.Equal(titulo, borrador.Titulo);
            Assert.Equal(contenido, borrador.Contenido);
            borradorId = borrador.Id;
        }

        // Releer con contexto independiente: existe y está en estado Borrador.
        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new ComunicadoService(ctxLectura);
            var leido = await servicio.Obtener(borradorId);

            Assert.NotNull(leido);
            Assert.Equal(EstadoComunicado.Borrador, leido!.Estado);
            Assert.Equal(titulo, leido.Titulo);
            Assert.Equal(contenido, leido.Contenido);
            Assert.Equal(fecha, leido.Fecha);
            Assert.Equal(1, ctxLectura.Comunicados.Count());
        }
    }
}
