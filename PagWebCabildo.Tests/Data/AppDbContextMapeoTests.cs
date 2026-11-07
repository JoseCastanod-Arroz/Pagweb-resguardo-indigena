using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Data;

/// <summary>
/// Pruebas unitarias del mapeo del <c>AppDbContext</c>. Verifican que cada entidad
/// del dominio persiste y se relee correctamente usando una base SQLite en memoria,
/// incluidos los tipos especiales (DateOnly/TimeOnly/DateTime), los campos enumerados
/// y las relaciones de clave foránea.
///
/// Validates: Requirements Data Models
/// </summary>
public class AppDbContextMapeoTests
{
    [Fact]
    public void Administrador_PersisteYRelee_ConRolYEstado()
    {
        using var bd = new SqliteEnMemoria();
        int idInsertado;

        using (var ctx = bd.CrearContexto())
        {
            var nuevo = new Administrador
            {
                Nombre = "Ana Gestora",
                NombreUsuario = "ana",
                HashContraseña = "hash-xyz",
                Rol = Rol.AdministradorGeneral,
                Activo = true
            };
            ctx.Administradores.Add(nuevo);
            ctx.SaveChanges();
            idInsertado = nuevo.Id;
        }

        using (var ctx = bd.CrearContexto())
        {
            var admin = ctx.Administradores.Single(a => a.Id == idInsertado);
            Assert.Equal("Ana Gestora", admin.Nombre);
            Assert.Equal("ana", admin.NombreUsuario);
            Assert.Equal("hash-xyz", admin.HashContraseña);
            Assert.Equal(Rol.AdministradorGeneral, admin.Rol);
            Assert.True(admin.Activo);
        }
    }

    [Fact]
    public void Administrador_NombreUsuarioDuplicado_LanzaPorIndiceUnico()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctx = bd.CrearContexto())
        {
            ctx.Administradores.Add(new Administrador { NombreUsuario = "repetido", Nombre = "Uno" });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            ctx.Administradores.Add(new Administrador { NombreUsuario = "repetido", Nombre = "Dos" });
            Assert.Throws<DbUpdateException>(() => ctx.SaveChanges());
        }
    }

    [Fact]
    public void PresentacionResguardo_PersisteYRelee()
    {
        using var bd = new SqliteEnMemoria();
        int idInsertado;

        using (var ctx = bd.CrearContexto())
        {
            var nueva = new PresentacionResguardo { Texto = "Historia del resguardo." };
            ctx.Presentaciones.Add(nueva);
            ctx.SaveChanges();
            idInsertado = nueva.Id;
        }

        using (var ctx = bd.CrearContexto())
        {
            var p = ctx.Presentaciones.Single(x => x.Id == idInsertado);
            Assert.Equal("Historia del resguardo.", p.Texto);
        }
    }

    [Fact]
    public void Autoridad_PersisteYRelee_ConOrden()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctx = bd.CrearContexto())
        {
            ctx.Autoridades.Add(new Autoridad
            {
                Nombre = "Juan Pérez",
                Cargo = "Gobernador",
                TipoAutoridad = "Autoridad",
                Orden = 3
            });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var a = ctx.Autoridades.Single();
            Assert.Equal("Juan Pérez", a.Nombre);
            Assert.Equal("Gobernador", a.Cargo);
            Assert.Equal("Autoridad", a.TipoAutoridad);
            Assert.Equal(3, a.Orden);
        }
    }

    [Fact]
    public void Evento_PersisteYRelee_ConDateOnlyTimeOnlyYCancelado()
    {
        using var bd = new SqliteEnMemoria();
        var fecha = new DateOnly(2025, 6, 15);
        var hora = new TimeOnly(14, 30, 0);

        using (var ctx = bd.CrearContexto())
        {
            ctx.Eventos.Add(new Evento
            {
                Titulo = "Asamblea",
                Fecha = fecha,
                Hora = hora,
                Lugar = "Casa Comunal",
                Descripcion = "Reunión general.",
                ImagenRuta = "uploads/eventos/foto.jpg",
                Cancelado = true
            });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var e = ctx.Eventos.Single();
            Assert.Equal("Asamblea", e.Titulo);
            Assert.Equal(fecha, e.Fecha);
            Assert.Equal(hora, e.Hora);
            Assert.Equal("Casa Comunal", e.Lugar);
            Assert.Equal("Reunión general.", e.Descripcion);
            Assert.Equal("uploads/eventos/foto.jpg", e.ImagenRuta);
            Assert.True(e.Cancelado);
        }
    }

    [Fact]
    public void Evento_SinImagen_ReleeImagenRutaNula()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctx = bd.CrearContexto())
        {
            ctx.Eventos.Add(new Evento
            {
                Titulo = "Minga",
                Fecha = new DateOnly(2025, 1, 1),
                Hora = new TimeOnly(8, 0),
                Lugar = "Río",
                Descripcion = "Limpieza.",
                ImagenRuta = null
            });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var e = ctx.Eventos.Single();
            Assert.Null(e.ImagenRuta);
        }
    }

    [Fact]
    public void Comunicado_PersisteYRelee_ConDateTimeYEstado()
    {
        using var bd = new SqliteEnMemoria();
        var fecha = new DateTime(2025, 3, 10, 9, 45, 0, DateTimeKind.Unspecified);

        using (var ctx = bd.CrearContexto())
        {
            ctx.Comunicados.Add(new Comunicado
            {
                Titulo = "Convocatoria",
                Fecha = fecha,
                Contenido = "Se convoca a la comunidad.",
                Estado = EstadoComunicado.Publicado
            });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var c = ctx.Comunicados.Single();
            Assert.Equal("Convocatoria", c.Titulo);
            Assert.Equal(fecha, c.Fecha);
            Assert.Equal("Se convoca a la comunidad.", c.Contenido);
            Assert.Equal(EstadoComunicado.Publicado, c.Estado);
        }
    }

    [Fact]
    public void InformacionContacto_PersisteYRelee()
    {
        using var bd = new SqliteEnMemoria();
        int idInsertado;

        using (var ctx = bd.CrearContexto())
        {
            var nueva = new InformacionContacto
            {
                Direccion = "Vereda Rincón Bodega",
                Telefono = "3001234567",
                CorreoElectronico = "contacto@resguardo.org",
                OtrosDatos = "Horario: L-V"
            };
            ctx.InformacionesContacto.Add(nueva);
            ctx.SaveChanges();
            idInsertado = nueva.Id;
        }

        using (var ctx = bd.CrearContexto())
        {
            var i = ctx.InformacionesContacto.Single(x => x.Id == idInsertado);
            Assert.Equal("Vereda Rincón Bodega", i.Direccion);
            Assert.Equal("3001234567", i.Telefono);
            Assert.Equal("contacto@resguardo.org", i.CorreoElectronico);
            Assert.Equal("Horario: L-V", i.OtrosDatos);
        }
    }

    [Fact]
    public void Mensaje_PersisteYRelee_ConContenidoFechaYAtendido()
    {
        using var bd = new SqliteEnMemoria();
        var recepcion = new DateTime(2025, 2, 20, 16, 5, 30, DateTimeKind.Unspecified);

        using (var ctx = bd.CrearContexto())
        {
            ctx.Mensajes.Add(new Mensaje
            {
                Nombre = "Visitante",
                MedioContacto = "visitante@correo.com",
                Asunto = "Consulta",
                Contenido = "Quisiera más información.",
                FechaRecepcion = recepcion,
                Atendido = true
            });
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var m = ctx.Mensajes.Single();
            Assert.Equal("Visitante", m.Nombre);
            Assert.Equal("visitante@correo.com", m.MedioContacto);
            Assert.Equal("Consulta", m.Asunto);
            Assert.Equal("Quisiera más información.", m.Contenido);
            Assert.Equal(recepcion, m.FechaRecepcion);
            Assert.True(m.Atendido);
        }
    }

    [Fact]
    public void TemaGaleria_ConElementos_PersisteYReleeRelacion()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctx = bd.CrearContexto())
        {
            var tema = new TemaGaleria
            {
                Nombre = "Fiestas",
                Descripcion = "Celebraciones.",
                PortadaRuta = "uploads/galeria/portadas/p.jpg",
                Elementos =
                {
                    new ElementoGaleria { Tipo = TipoElemento.Foto, ArchivoRuta = "f1.jpg", Descripcion = "Foto 1" },
                    new ElementoGaleria { Tipo = TipoElemento.Video, ArchivoRuta = "v1.mp4", Descripcion = "Video 1" }
                }
            };
            ctx.TemasGaleria.Add(tema);
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var tema = ctx.TemasGaleria
                .Include(t => t.Elementos)
                .Single();

            Assert.Equal("Fiestas", tema.Nombre);
            Assert.Equal("uploads/galeria/portadas/p.jpg", tema.PortadaRuta);
            Assert.Equal(2, tema.Elementos.Count);

            var foto = tema.Elementos.Single(e => e.Tipo == TipoElemento.Foto);
            Assert.Equal("f1.jpg", foto.ArchivoRuta);
            Assert.Equal(tema.Id, foto.TemaGaleriaId);

            var video = tema.Elementos.Single(e => e.Tipo == TipoElemento.Video);
            Assert.Equal("v1.mp4", video.ArchivoRuta);
            Assert.Equal(tema.Id, video.TemaGaleriaId);
        }
    }

    [Fact]
    public void TemaGaleria_EliminarEnCascada_EliminaSusElementos()
    {
        using var bd = new SqliteEnMemoria();
        int temaId;

        using (var ctx = bd.CrearContexto())
        {
            var tema = new TemaGaleria
            {
                Nombre = "Temporal",
                Elementos = { new ElementoGaleria { Tipo = TipoElemento.Foto, ArchivoRuta = "a.jpg" } }
            };
            ctx.TemasGaleria.Add(tema);
            ctx.SaveChanges();
            temaId = tema.Id;
        }

        using (var ctx = bd.CrearContexto())
        {
            var tema = ctx.TemasGaleria.Include(t => t.Elementos).Single(t => t.Id == temaId);
            ctx.TemasGaleria.Remove(tema);
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            Assert.Empty(ctx.TemasGaleria);
            Assert.Empty(ctx.ElementosGaleria);
        }
    }

    [Fact]
    public void CategoriaDocumento_ConDocumentos_PersisteYReleeRelacion()
    {
        using var bd = new SqliteEnMemoria();

        using (var ctx = bd.CrearContexto())
        {
            var categoria = new CategoriaDocumento
            {
                Nombre = "Reglamentos",
                Documentos =
                {
                    new Documento { Titulo = "Reglamento interno", Descripcion = "PDF", ArchivoRuta = "r1.pdf" },
                    new Documento { Titulo = "Estatutos", Descripcion = "PDF", ArchivoRuta = "r2.pdf" }
                }
            };
            ctx.CategoriasDocumento.Add(categoria);
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            var categoria = ctx.CategoriasDocumento
                .Include(c => c.Documentos)
                .Single();

            Assert.Equal("Reglamentos", categoria.Nombre);
            Assert.Equal(2, categoria.Documentos.Count);

            var doc = categoria.Documentos.Single(d => d.Titulo == "Reglamento interno");
            Assert.Equal("r1.pdf", doc.ArchivoRuta);
            Assert.Equal(categoria.Id, doc.CategoriaDocumentoId);
        }
    }

    [Fact]
    public void CategoriaDocumento_EliminarEnCascada_EliminaSusDocumentos()
    {
        using var bd = new SqliteEnMemoria();
        int categoriaId;

        using (var ctx = bd.CrearContexto())
        {
            var categoria = new CategoriaDocumento
            {
                Nombre = "Temporal",
                Documentos = { new Documento { Titulo = "Doc", ArchivoRuta = "d.pdf" } }
            };
            ctx.CategoriasDocumento.Add(categoria);
            ctx.SaveChanges();
            categoriaId = categoria.Id;
        }

        using (var ctx = bd.CrearContexto())
        {
            var categoria = ctx.CategoriasDocumento.Include(c => c.Documentos).Single(c => c.Id == categoriaId);
            ctx.CategoriasDocumento.Remove(categoria);
            ctx.SaveChanges();
        }

        using (var ctx = bd.CrearContexto())
        {
            Assert.Empty(ctx.CategoriasDocumento);
            Assert.Empty(ctx.Documentos);
        }
    }
}
