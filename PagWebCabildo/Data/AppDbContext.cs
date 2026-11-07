using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Data;

/// <summary>
/// Contexto de EF Core del sitio del resguardo. Define los <see cref="DbSet{TEntity}"/>
/// de todas las entidades del dominio y la configuración del modelo.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Administrador> Administradores => Set<Administrador>();
    public DbSet<PresentacionResguardo> Presentaciones => Set<PresentacionResguardo>();
    public DbSet<Autoridad> Autoridades => Set<Autoridad>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Comunicado> Comunicados => Set<Comunicado>();
    public DbSet<TemaGaleria> TemasGaleria => Set<TemaGaleria>();
    public DbSet<ElementoGaleria> ElementosGaleria => Set<ElementoGaleria>();
    public DbSet<CategoriaDocumento> CategoriasDocumento => Set<CategoriaDocumento>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<InformacionContacto> InformacionesContacto => Set<InformacionContacto>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<Acta> Actas => Set<Acta>();
    public DbSet<RegistroActividad> RegistrosActividad => Set<RegistroActividad>();
    public DbSet<PublicacionForo> PublicacionesForo => Set<PublicacionForo>();
    public DbSet<ComentarioForo> ComentariosForo => Set<ComentarioForo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // El nombre de usuario del administrador es único.
        modelBuilder.Entity<Administrador>()
            .HasIndex(a => a.NombreUsuario)
            .IsUnique();

        // Relación uno-a-muchos: un TemaGaleria tiene muchos ElementoGaleria.
        modelBuilder.Entity<ElementoGaleria>()
            .HasOne(e => e.TemaGaleria)
            .WithMany(t => t.Elementos)
            .HasForeignKey(e => e.TemaGaleriaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación uno-a-muchos: una CategoriaDocumento tiene muchos Documento.
        modelBuilder.Entity<Documento>()
            .HasOne(d => d.CategoriaDocumento)
            .WithMany(c => c.Documentos)
            .HasForeignKey(d => d.CategoriaDocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación muchos-a-uno: un Acta fue registrada por un Administrador.
        // El Administrador no tiene colección de navegación hacia Acta y se usa
        // DeleteBehavior.Restrict para que al eliminar un administrador no se
        // borren en cascada las actas, que son registros permanentes.
        modelBuilder.Entity<Acta>()
            .HasOne(a => a.RegistradoPor)
            .WithMany()
            .HasForeignKey(a => a.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        // El número consecutivo del acta es único.
        modelBuilder.Entity<Acta>()
            .HasIndex(a => a.Consecutivo)
            .IsUnique();

        // Relación uno-a-muchos: una PublicacionForo tiene muchos ComentarioForo.
        // Al eliminar una publicación se borran en cascada sus comentarios.
        modelBuilder.Entity<ComentarioForo>()
            .HasOne(c => c.PublicacionForo)
            .WithMany(p => p.Comentarios)
            .HasForeignKey(c => c.PublicacionForoId)
            .OnDelete(DeleteBehavior.Cascade);

        SembrarDatosIniciales(modelBuilder);
    }

    /// <summary>
    /// Siembra los registros singleton (<see cref="PresentacionResguardo"/> e
    /// <see cref="InformacionContacto"/>) y el administrador inicial.
    /// </summary>
    /// <remarks>
    /// El hash de la contraseña está precalculado como literal constante para que la
    /// migración de *seeding* sea determinista. <see cref="PasswordHasher{TUser}"/> genera
    /// una sal aleatoria en cada llamada, por lo que invocarlo aquí produciría un hash
    /// distinto en cada compilación y migraciones espurias. El literal corresponde a la
    /// contraseña por defecto "Admin123*" para el usuario "admin".
    /// </remarks>
    private static void SembrarDatosIniciales(ModelBuilder modelBuilder)
    {
        // Registro singleton de la presentación/historia del resguardo.
        modelBuilder.Entity<PresentacionResguardo>().HasData(
            new PresentacionResguardo
            {
                Id = 1,
                Texto =
                    "El Resguardo Indígena Rincón Bodega es una comunidad del pueblo Pijao, " +
                    "ubicada en el municipio de Natagaima, departamento del Tolima (Colombia). " +
                    "Fue reconocido mediante la Resolución N.° 0055 del 18 de diciembre de 2000 " +
                    "del INCORA (NIT. 900087502-4).\n\n" +
                    "Nuestra comunidad y nuestro cabildo ejercen su autonomía al amparo de la Ley 89 " +
                    "de 1890 y de los artículos 246 y 330 de la Constitución Política de 1991, " +
                    "ratificados por la Sentencia C-139 de 1996 de la Honorable Corte Constitucional, " +
                    "como un sistema propio que regula nuestra tradición, usos, costumbres y cultura.\n\n" +
                    "El Resguardo trabaja por defender los derechos e intereses de la población " +
                    "indígena Pijao del Tolima, promover el conocimiento y la investigación de nuestra " +
                    "realidad conforme al uso, la costumbre y la cultura de la etnia, fortalecer la " +
                    "unión con otras organizaciones populares en busca del bien común, y recuperar y " +
                    "fortalecer nuestros usos, costumbres e identidad cultural."
            });

        // Registro singleton de la información de contacto.
        modelBuilder.Entity<InformacionContacto>().HasData(
            new InformacionContacto
            {
                Id = 1,
                Direccion = "Resguardo Indígena Rincón Bodega, municipio de Natagaima, Tolima (Colombia)",
                Telefono = "0000000",
                CorreoElectronico = "contacto@resguardorinconbodega.org",
                OtrosDatos =
                    "Comunidad Pijao. NIT. 900087502-4. " +
                    "Reconocido por Resolución N.° 0055 del 18 de diciembre de 2000 (INCORA). " +
                    "Nota: actualice el teléfono y el correo oficiales desde el Panel de Administración."
            });

        // Autoridades y cargos del cabildo según el Reglamento Interno (Capítulo Cuarto).
        // El reglamento solo registra el nombre del Gobernador y la Gobernadora Suplente
        // (firmas del documento); el resto de cargos queda "Por definir" para completarse
        // en cada elección anual desde el Panel de Administración.
        modelBuilder.Entity<Autoridad>().HasData(
            new Autoridad
            {
                Id = 1, Nombre = "Hugo Loaiza", Cargo = "Gobernador", TipoAutoridad = "Cabildo", Orden = 1,
                Funcion = "El Gobernador es la máxima autoridad del cabildo y el representante legal del resguardo ante la " +
                          "comunidad, el Estado y otras organizaciones. Dirige la vida institucional del resguardo: preside las " +
                          "reuniones del cabildo y las asambleas de la comunidad, orienta la ejecución de los proyectos y, junto " +
                          "con el Tesorero, administra los recursos comunes. Vela por el uso adecuado de las parcelas y por el " +
                          "cumplimiento de los trabajos comunitarios, aprueba las iniciativas que benefician a la comunidad y se " +
                          "asegura de que las decisiones tomadas en asamblea se lleven a la práctica. También firma las " +
                          "resoluciones, sentencias y documentos oficiales que el cabildo expide en ejercicio de la justicia propia."
            },
            new Autoridad
            {
                Id = 2, Nombre = "Graciela Capera Cumaco", Cargo = "Gobernadora Suplente", TipoAutoridad = "Cabildo", Orden = 2,
                Funcion = "La Gobernadora Suplente acompaña al Gobernador en el ejercicio de su cargo y garantiza la continuidad " +
                          "del gobierno propio. Cuando el Gobernador se encuentra ausente, incapacitado o así lo autoriza, asume " +
                          "plenamente sus mismas funciones y responsabilidades, de modo que el resguardo nunca quede sin dirección " +
                          "ni representación."
            },
            new Autoridad
            {
                Id = 3, Nombre = "Por definir", Cargo = "Secretario(a)", TipoAutoridad = "Cabildo", Orden = 3,
                Funcion = "El Secretario es responsable de la memoria escrita del resguardo. Asiste a las reuniones del cabildo y " +
                          "redacta las actas que dejan constancia de lo acordado, prepara los documentos y la correspondencia que " +
                          "se emiten a nombre de la comunidad y mantiene organizado el archivo. Además, informa al Gobernador " +
                          "sobre las peticiones, denuncias y sugerencias que llegan de los comuneros, y firma, junto con él, la " +
                          "documentación oficial. Su labor asegura que las decisiones queden registradas y puedan consultarse."
            },
            new Autoridad
            {
                Id = 4, Nombre = "Por definir", Cargo = "Tesorero(a)", TipoAutoridad = "Cabildo", Orden = 4,
                Funcion = "El Tesorero administra los recursos económicos del resguardo con transparencia. Lleva el registro " +
                          "ordenado de los ingresos y gastos, elabora los informes y balances, y gestiona el dinero de la " +
                          "comunidad: consigna y retira fondos y realiza los contratos de compra y venta de bienes y servicios " +
                          "necesarios. Rinde cuentas de manera periódica y mantiene la contabilidad abierta a la revisión de la " +
                          "comunidad, de forma que todos puedan conocer en qué se invierten los recursos comunes."
            },
            new Autoridad
            {
                Id = 5, Nombre = "Por definir", Cargo = "Fiscal", TipoAutoridad = "Cabildo", Orden = 5,
                Funcion = "El Fiscal cumple una función de control y vigilancia dentro del resguardo. Supervisa que las acciones " +
                          "de los integrantes del cabildo y de la comunidad se ajusten al reglamento interno y a la ley propia, " +
                          "y revisa los libros de contabilidad, la cuenta bancaria y las actas de la Secretaría. Cuando detecta " +
                          "manejos indebidos o actuaciones irregulares, los denuncia ante la Asamblea para que se tomen las " +
                          "medidas correspondientes. Su papel protege el buen uso de los recursos y la confianza de la comunidad."
            },
            new Autoridad
            {
                Id = 6, Nombre = "Por definir", Cargo = "Alcalde Mayor", TipoAutoridad = "Cabildo", Orden = 6,
                Funcion = "El Alcalde Mayor apoya directamente al Gobernador en las tareas de campo y en la convivencia diaria " +
                          "del resguardo. Lo acompaña en las inspecciones del territorio y en la solución de conflictos, atiende a " +
                          "las personas y entidades externas que visitan la comunidad, y coordina y encabeza los trabajos " +
                          "comunitarios. Como jefe de trabajo, organiza las labores colectivas y procura que se realicen de manera " +
                          "ordenada y en beneficio de todos."
            },
            new Autoridad
            {
                Id = 7, Nombre = "Por definir", Cargo = "Comisario(a)", TipoAutoridad = "Cabildo", Orden = 7,
                Funcion = "El Comisario se encarga de convocar y comunicar a la comunidad. Por indicación previa y escrita del " +
                          "Gobernador, cita a los comuneros y a los miembros del resguardo a las reuniones ordinarias y " +
                          "extraordinarias, y se asegura de que la información llegue a tiempo. Cumple, además, las demás tareas de " +
                          "coordinación que el Gobernador le asigne conforme al reglamento interno, facilitando la participación de " +
                          "todos en la vida del resguardo."
            },
            new Autoridad
            {
                Id = 8, Nombre = "Por definir", Cargo = "Alguacil", TipoAutoridad = "Cabildo", Orden = 8,
                Funcion = "El Alguacil vela por el orden, la seguridad y la convivencia pacífica de la comunidad y por el cuidado " +
                          "de sus bienes. Recorre y revisa periódicamente los linderos del resguardo para proteger el territorio, " +
                          "convoca a reuniones cuando advierte irregularidades y da trámite a las quejas y denuncias que presentan " +
                          "los miembros del resguardo. Su labor contribuye a mantener la armonía interna y el respeto por las " +
                          "normas propias."
            });

        // Eventos de ejemplo de la comunidad (contenido de muestra, editable desde
        // el Panel de Administración). Fechas fijas para que el seeding sea determinista.
        modelBuilder.Entity<Evento>().HasData(
            new Evento
            {
                Id = 1,
                Titulo = "Minga comunitaria de siembra",
                Fecha = new DateOnly(2026, 3, 14),
                Hora = new TimeOnly(7, 0),
                Lugar = "Parcela comunitaria del resguardo",
                Descripcion = "Jornada de trabajo colectivo para la siembra de maíz, yuca y plátano en la parcela " +
                              "comunitaria. Compartimos semillas, saberes y un almuerzo preparado entre todas las familias. " +
                              "Trae tu herramienta y muchas ganas de compartir.",
                Cancelado = false
            },
            new Evento
            {
                Id = 2,
                Titulo = "Noche de relatos de los mayores",
                Fecha = new DateOnly(2026, 4, 25),
                Hora = new TimeOnly(18, 30),
                Lugar = "Casa del cabildo",
                Descripcion = "Un encuentro tranquilo alrededor del fogón para escuchar a los mayores contar historias, " +
                              "mitos y memorias del pueblo Pijao. Habrá chicha y aromática para acompañar la conversación. " +
                              "Espacio abierto a niños, jóvenes y adultos.",
                Cancelado = false
            },
            new Evento
            {
                Id = 3,
                Titulo = "Festival del maíz y la gastronomía propia",
                Fecha = new DateOnly(2026, 6, 20),
                Hora = new TimeOnly(10, 0),
                Lugar = "Plaza central de Rincón Bodega",
                Descripcion = "Celebración de nuestra cocina tradicional: tamales, insulso, bollos y bebidas de maíz. " +
                              "Música, danza y muestras de artesanía durante todo el día. Una jornada para compartir en " +
                              "familia y con los visitantes.",
                Cancelado = false
            },
            new Evento
            {
                Id = 4,
                Titulo = "Caminata por los linderos del territorio",
                Fecha = new DateOnly(2026, 8, 9),
                Hora = new TimeOnly(6, 30),
                Lugar = "Punto de encuentro: casa del cabildo",
                Descripcion = "Recorrido guiado por los linderos del resguardo para reconocer el territorio, los caminos " +
                              "y los sitios de importancia cultural. Actividad al aire libre, a ritmo pausado. " +
                              "Lleva agua, sombrero y calzado cómodo.",
                Cancelado = false
            },
            new Evento
            {
                Id = 5,
                Titulo = "Taller de tejido y artesanía tradicional",
                Fecha = new DateOnly(2026, 9, 12),
                Hora = new TimeOnly(14, 0),
                Lugar = "Casa del cabildo",
                Descripcion = "Tarde dedicada a aprender y compartir técnicas de tejido y elaboración de artesanías propias. " +
                              "Guiado por artesanas de la comunidad. Los materiales los pone el resguardo; solo trae tu " +
                              "creatividad.",
                Cancelado = false
            });

        // Comunicados de ejemplo (publicados). Contenido de muestra, editable desde
        // el Panel de Administración. Fechas fijas para un seeding determinista.
        modelBuilder.Entity<Comunicado>().HasData(
            new Comunicado
            {
                Id = 1,
                Titulo = "Convocatoria a Asamblea General de la comunidad",
                Fecha = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Unspecified),
                Contenido = "El cabildo convoca a todos los comuneros a la Asamblea General que se realizará en la casa del " +
                            "cabildo. En ella se presentarán los informes del año, se revisarán los proyectos en curso y se " +
                            "tomarán decisiones importantes para la comunidad. La participación de todas las familias es " +
                            "fundamental para el gobierno propio del resguardo.",
                Estado = EstadoComunicado.Publicado
            },
            new Comunicado
            {
                Id = 2,
                Titulo = "Jornada de limpieza y cuidado del territorio",
                Fecha = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Contenido = "Invitamos a la comunidad a una jornada de limpieza de los caminos, las fuentes de agua y los " +
                            "espacios comunes del resguardo. Cuidar nuestro territorio es una responsabilidad compartida. " +
                            "Habrá refrigerio para quienes participen.",
                Estado = EstadoComunicado.Publicado
            },
            new Comunicado
            {
                Id = 3,
                Titulo = "Inscripciones abiertas para el taller de lengua y cultura Pijao",
                Fecha = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Unspecified),
                Contenido = "Se abren las inscripciones para el taller dedicado a fortalecer la lengua, los saberes y las " +
                            "tradiciones del pueblo Pijao. Está dirigido a niños, jóvenes y adultos de la comunidad. " +
                            "Acércate a la casa del cabildo para registrar tu participación.",
                Estado = EstadoComunicado.Publicado
            },
            new Comunicado
            {
                Id = 4,
                Titulo = "Agradecimiento por la minga de siembra",
                Fecha = new DateTime(2026, 3, 16, 0, 0, 0, DateTimeKind.Unspecified),
                Contenido = "El cabildo agradece a todas las familias que participaron en la minga comunitaria de siembra. " +
                            "Gracias al trabajo colectivo logramos sembrar la parcela comunitaria en un solo día. " +
                            "Este es el espíritu que mantiene vivo nuestro resguardo.",
                Estado = EstadoComunicado.Publicado
            });

        // Tema de galería de ejemplo con fotos (contenido de muestra, editable y
        // reemplazable desde el Panel de Administración). Las imágenes viven en
        // wwwroot/uploads/galeria.
        modelBuilder.Entity<TemaGaleria>().HasData(
            new TemaGaleria
            {
                Id = 1,
                Nombre = "Territorio y comunidad",
                Descripcion = "Paisajes, vida cotidiana y encuentros de la comunidad del Resguardo Indígena Rincón Bodega.",
                PortadaRuta = "uploads/galeria/portadas/muestra-portada.jpeg"
            });

        modelBuilder.Entity<ElementoGaleria>().HasData(
            new ElementoGaleria
            {
                Id = 1, TemaGaleriaId = 1, Tipo = TipoElemento.Foto,
                ArchivoRuta = "uploads/galeria/elementos/muestra-1.jpeg",
                Descripcion = "Paisaje del territorio del resguardo."
            },
            new ElementoGaleria
            {
                Id = 2, TemaGaleriaId = 1, Tipo = TipoElemento.Foto,
                ArchivoRuta = "uploads/galeria/elementos/muestra-2.jpeg",
                Descripcion = "Vida cotidiana en la comunidad."
            },
            new ElementoGaleria
            {
                Id = 3, TemaGaleriaId = 1, Tipo = TipoElemento.Foto,
                ArchivoRuta = "uploads/galeria/elementos/muestra-3.png",
                Descripcion = "Tradición y cultura Pijao."
            },
            new ElementoGaleria
            {
                Id = 4, TemaGaleriaId = 1, Tipo = TipoElemento.Foto,
                ArchivoRuta = "uploads/galeria/elementos/muestra-4.jpeg",
                Descripcion = "Naturaleza alrededor de Rincón Bodega."
            });

        // Categorías y documentos de PRUEBA (contenido de muestra). Los archivos
        // físicos viven en wwwroot/uploads/documentos. Reemplazar desde el Panel
        // de Administración por documentos reales.
        modelBuilder.Entity<CategoriaDocumento>().HasData(
            new CategoriaDocumento { Id = 1, Nombre = "Categoría de prueba A" },
            new CategoriaDocumento { Id = 2, Nombre = "Categoría de prueba B" });

        modelBuilder.Entity<Documento>().HasData(
            new Documento
            {
                Id = 1, CategoriaDocumentoId = 1,
                Titulo = "Documento de prueba 1",
                Descripcion = "Este es un documento de PRUEBA. Reemplázalo por un archivo real.",
                ArchivoRuta = "uploads/documentos/documento-prueba-1.txt"
            },
            new Documento
            {
                Id = 2, CategoriaDocumentoId = 1,
                Titulo = "Documento de prueba 2",
                Descripcion = "Este es un documento de PRUEBA. Reemplázalo por un archivo real.",
                ArchivoRuta = "uploads/documentos/documento-prueba-2.txt"
            },
            new Documento
            {
                Id = 3, CategoriaDocumentoId = 2,
                Titulo = "Documento de prueba 3",
                Descripcion = "Este es un documento de PRUEBA. Reemplázalo por un archivo real.",
                ArchivoRuta = "uploads/documentos/documento-prueba-3.txt"
            },
            new Documento
            {
                Id = 4, CategoriaDocumentoId = 2,
                Titulo = "Documento de prueba 4",
                Descripcion = "Este es un documento de PRUEBA. Reemplázalo por un archivo real.",
                ArchivoRuta = "uploads/documentos/documento-prueba-4.txt"
            });

        // Administrador inicial con rol AdministradorGeneral.
        // Contraseña por defecto: "Admin123*" (hash precalculado, ver remarks).
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                Id = 1,
                Nombre = "Administrador General",
                NombreUsuario = "admin",
                HashContraseña =
                    "AQAAAAIAAYagAAAAEMIqadTtIKxWfSEBUsfxIq+G1rs4BdZjHbPLGpQvm8oCW4EpdD3HpC7z6/a6oiFBEQ==",
                Rol = Rol.AdministradorGeneral,
                Activo = true
            });
    }
}
