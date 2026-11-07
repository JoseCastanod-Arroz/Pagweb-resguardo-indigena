# Implementation Plan: Sitio Web Resguardo Indígena Rincón Bodega

## Overview

Este plan convierte el diseño del sitio web del Resguardo Indígena Rincón Bodega en una serie de tareas de implementación incrementales para ASP.NET Core MVC (net10.0). Cada tarea se apoya en las anteriores y termina integrando el código en el flujo existente, sin dejar código huérfano.

El orden general es: configuración del proyecto y paquetes → modelos de datos, `DbContext`, migraciones y *seeding* → capa de servicios → infraestructura de autenticación/autorización → controladores y vistas públicas → Area Admin (controladores y vistas) → manejo de archivos/medios → validación → pruebas (unitarias, basadas en propiedades e integración).

Las tareas se agrupan por módulo/capa y se asignan al sitio público (Requisitos 1–17) y al Panel_Admin (Requisitos 18–43). Las pruebas basadas en propiedades implementan las 12 Correctness Properties del diseño con un mínimo de 100 iteraciones cada una.

## Tasks

- [x] 1. Configurar el proyecto, paquetes y estructura base
  - [x] 1.1 Agregar paquetes y estructura de carpetas base
    - Añadir al proyecto `PagWebCabildo/PagWebCabildo` los paquetes `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Design` y `Microsoft.AspNetCore.Authentication.Cookies` (incluido en el framework)
    - Crear la estructura de carpetas `Data/`, `Models/Entidades/`, `Services/`, `Areas/Admin/Controllers`, `Areas/Admin/Views`
    - Crear las subcarpetas de medios `wwwroot/uploads/eventos`, `wwwroot/uploads/galeria/portadas`, `wwwroot/uploads/galeria/elementos`, `wwwroot/uploads/documentos` (con archivos `.gitkeep`)
    - _Requisitos: Architecture (arquitectura en capas), File/Media Handling_

  - [x] 1.2 Configurar la cadena de conexión SQLite
    - Añadir la cadena de conexión a `app.db` en `appsettings.json` y `appsettings.Development.json`
    - _Requisitos: Architecture (persistencia con EF Core + SQLite)_

- [x] 2. Definir los modelos de datos y el DbContext
  - [x] 2.1 Crear las enumeraciones y entidades del dominio
    - Crear las enumeraciones `Rol` (AdministradorGeneral, Editor), `EstadoComunicado` (Borrador, Publicado) y `TipoElemento` (Foto, Video)
    - Crear las entidades `Administrador`, `PresentacionResguardo`, `Autoridad`, `Evento`, `Comunicado`, `TemaGaleria`, `ElementoGaleria`, `CategoriaDocumento`, `Documento`, `InformacionContacto` y `Mensaje` según el modelo de datos del diseño
    - _Requisitos: 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 21, 22, 23_

  - [x] 2.2 Crear el AppDbContext con DbSets y configuración
    - Definir `AppDbContext` con los `DbSet` de todas las entidades
    - Configurar en `OnModelCreating` el índice único de `Administrador.NombreUsuario` y las relaciones `TemaGaleria`→`ElementoGaleria` y `CategoriaDocumento`→`Documento`
    - _Requisitos: Data Models (DbContext)_

  - [x]* 2.3 Escribir pruebas unitarias del mapeo del DbContext
    - Verificar que las entidades persisten y se releen correctamente usando una base SQLite en memoria
    - _Requisitos: Data Models_

- [x] 3. Configurar migraciones y seeding inicial
  - [x] 3.1 Registrar el DbContext y crear la migración inicial
    - Registrar `AddDbContext<AppDbContext>(opt => opt.UseSqlite(...))` en `Program.cs`
    - Crear la migración inicial de EF Core y aplicar la base de datos al arranque
    - _Requisitos: Architecture (Program.cs), Data Models_

  - [x] 3.2 Sembrar registros singleton y administrador inicial
    - Sembrar en `OnModelCreating` un registro único de `PresentacionResguardo` e `InformacionContacto`
    - Sembrar un `Administrador` inicial con rol `AdministradorGeneral` y contraseña *hasheada* con `PasswordHasher`
    - _Requisitos: 3, 15, 18, 24, 40, Data Models (seeding)_

- [x] 4. Implementar servicios de validación y archivos (transversales)
  - [x] 4.1 Implementar IValidacionContactoService
    - Implementar `EsMedioContactoValido(string medio)` que acepta correo (`nombre@dominio.extensión`) o teléfono (7–15 dígitos)
    - _Requisitos: 16.2, 16.5_

  - [x]* 4.2 Escribir prueba basada en propiedades para la validación de contacto
    - **Feature: sitio-web-resguardo, Property 8: El medio de contacto se acepta si y solo si es correo o teléfono válidos**
    - **Validates: Requirements 16.2, 16.5**
    - Mínimo 100 iteraciones; generar correos y teléfonos válidos e inválidos, incluyendo límites de longitud
    - _Requisitos: 16.2, 16.5_

  - [x] 4.3 Implementar IArchivoService
    - Implementar `Guardar(IFormFile, subcarpeta)` con nombre único (`Guid` + extensión) y validación de extensiones permitidas (imágenes, videos, documentos)
    - Implementar `Eliminar(rutaRelativa)` y `Abrir(rutaRelativa)` apoyándose en `IWebHostEnvironment`
    - _Requisitos: 14, 27, 33, 35, 38, File/Media Handling_

  - [x]* 4.4 Escribir pruebas unitarias de IArchivoService
    - Probar guardado con extensión válida, rechazo de extensión no permitida y eliminación de archivo físico
    - _Requisitos: File/Media Handling_

- [x] 5. Implementar la capa de servicios de contenido
  - [x] 5.1 Implementar IPresentacionService e IContactoService
    - Implementar `ObtenerPresentacion`/`ActualizarPresentacion` y `ObtenerInformacion`/`ActualizarInformacion` sobre los registros singleton
    - _Requisitos: 3, 15, 24, 40_

  - [x] 5.2 Implementar IAutoridadService con reordenamiento
    - Implementar CRUD y `ReordenarAutoridades`; el listado devuelve autoridades ordenadas de forma ascendente por `Orden`
    - _Requisitos: 4.1, 4.2, 25.1, 25.2, 25.3, 25.4_

  - [x]* 5.3 Escribir prueba basada en propiedades del orden de autoridades
    - **Feature: sitio-web-resguardo, Property 5: Las autoridades se muestran en el orden definido**
    - **Validates: Requirements 4.2, 25.3**
    - Mínimo 100 iteraciones
    - _Requisitos: 4.2, 25.3_

  - [x] 5.4 Implementar IEventoService con ordenamiento por proximidad
    - Implementar CRUD, `ListarOrdenadosPorProximidad` (fecha más próxima primero), marcar cancelado y asociar imagen
    - _Requisitos: 5.1, 5.2, 6, 7, 26, 27, 28.1, 29_

  - [x]* 5.5 Escribir prueba basada en propiedades del orden de eventos
    - **Feature: sitio-web-resguardo, Property 1: Los eventos se ordenan por proximidad de fecha**
    - **Validates: Requirements 5.2**
    - Mínimo 100 iteraciones; incluir fechas repetidas
    - _Requisitos: 5.2_

  - [x] 5.6 Implementar IComunicadoService con reglas de borrador y publicación
    - Implementar CRUD, `ListarPublicadosRecientesPrimero`, `GuardarBorrador` (exige título y contenido) y `Publicar` (solo desde borrador)
    - Garantizar que los borradores no se exponen por listado ni por acceso directo
    - _Requisitos: 8.1, 8.2, 9, 30, 31.1, 32.1, 32.2, 32.3, 32.4, 32.5, 32.6_

  - [x]* 5.7 Escribir prueba basada en propiedades del orden de comunicados publicados
    - **Feature: sitio-web-resguardo, Property 2: Los comunicados publicados se ordenan del más reciente al más antiguo**
    - **Validates: Requirements 8.2**
    - Mínimo 100 iteraciones; incluir fechas repetidas
    - _Requisitos: 8.2_

  - [x]* 5.8 Escribir prueba basada en propiedades de ocultamiento de borradores
    - **Feature: sitio-web-resguardo, Property 3: Los borradores nunca aparecen en vistas públicas**
    - **Validates: Requirements 8.1, 32.3**
    - Mínimo 100 iteraciones; generar conjuntos mixtos de borradores y publicados
    - _Requisitos: 8.1, 32.3_

  - [x]* 5.9 Escribir prueba basada en propiedades de transición de publicación
    - **Feature: sitio-web-resguardo, Property 11: Publicar solo procede desde el estado borrador**
    - **Validates: Requirements 32.4, 32.5, 32.6**
    - Mínimo 100 iteraciones
    - _Requisitos: 32.4, 32.5, 32.6_

  - [x] 5.10 Implementar IGaleriaService
    - Implementar CRUD de temas y elementos; asociar elementos a su tema
    - _Requisitos: 10, 11, 12, 33, 34.1, 35, 36.1_

  - [x] 5.11 Implementar IDocumentoService con agrupación por categoría
    - Implementar CRUD de categorías y documentos y `ListarAgrupadosPorCategoria`
    - _Requisitos: 13.1, 37.1, 37.2, 38, 39.1_

  - [x]* 5.12 Escribir prueba basada en propiedades de agrupación de documentos
    - **Feature: sitio-web-resguardo, Property 12: Cada documento se agrupa bajo su propia categoría**
    - **Validates: Requirements 13.1**
    - Mínimo 100 iteraciones
    - _Requisitos: 13.1_

  - [x] 5.13 Implementar IMensajeService con validación y ordenamiento
    - Implementar `RegistrarMensaje` (valida campos obligatorios, longitudes y medio de contacto), `ListarRecientesPrimero`, `Ver`, `MarcarAtendido` y `Eliminar`
    - _Requisitos: 16.3, 16.4, 17.1, 41.1, 41.2, 42, 43.1_

  - [x]* 5.14 Escribir prueba basada en propiedades del orden de mensajes
    - **Feature: sitio-web-resguardo, Property 4: Los mensajes se ordenan del más reciente al más antiguo**
    - **Validates: Requirements 41.2**
    - Mínimo 100 iteraciones; incluir fechas de recepción repetidas
    - _Requisitos: 41.2_

  - [x]* 5.15 Escribir prueba basada en propiedades de rechazo de formulario incompleto
    - **Feature: sitio-web-resguardo, Property 9: Un formulario con campos obligatorios vacíos se rechaza sin registrar mensaje**
    - **Validates: Requirements 16.4**
    - Mínimo 100 iteraciones; generar campos vacíos o compuestos solo por espacios
    - _Requisitos: 16.4_

  - [x]* 5.16 Escribir prueba basada en propiedades de registro de formulario válido
    - **Feature: sitio-web-resguardo, Property 10: Un formulario válido registra el mensaje de forma recuperable**
    - **Validates: Requirements 16.3, 17.1**
    - Mínimo 100 iteraciones; generar modelos válidos dentro de los límites de longitud
    - _Requisitos: 16.3, 17.1_

  - [x] 5.17 Implementar IUsuarioService e IAutenticacionService
    - Implementar CRUD de administradores con *hashing* de contraseña (`PasswordHasher`) en `IUsuarioService`
    - Implementar `ValidarCredenciales` en `IAutenticacionService` (verifica cuenta activa y hash)
    - _Requisitos: 18.1, 18.2, 21.1, 22.1, 22.2, 23.1_

  - [x]* 5.18 Escribir pruebas unitarias de servicios con casos límite
    - Probar guardar/releer presentación e información de contacto, marcar evento cancelado, marcar mensaje atendido y mensajes de error de borrador
    - _Requisitos: 24, 29, 32.2, 40, 42_

- [x] 6. Checkpoint - Verificar servicios y propiedades
  - Compilar la solución y asegurar que todas las pruebas de servicios y de propiedades pasan; preguntar al usuario si surgen dudas.

- [x] 7. Implementar la infraestructura de autenticación y autorización
  - [x] 7.1 Configurar autenticación por cookies y políticas de rol
    - Configurar `AddAuthentication(Cookie).AddCookie(...)` con `LoginPath = "/Admin/Account/Login"`
    - Definir las políticas `"GestionCuentas"` y `"EliminarContenido"` que exigen el rol `Administrador_General`
    - Añadir el middleware en orden: `UseStaticFiles` → `UseRouting` → `UseAuthentication` → `UseAuthorization` y registrar las rutas de Area
    - _Requisitos: 18.1, 18.3, 23.2, 23.3, Architecture (Program.cs)_

- [x] 8. Implementar el sitio público (controladores y vistas)
  - [x] 8.1 Actualizar el layout público y la página de inicio
    - Ajustar `Views/Shared/_Layout.cshtml` con el menú de navegación a todas las secciones públicas
    - Implementar `HomeController.Index` con presentación, accesos a secciones y últimos eventos y comunicados, y su vista
    - _Requisitos: 1.1, 1.2, 1.3, 1.4, 2.1, 2.2, 2.3_

  - [x] 8.2 Implementar Historia y Organización
    - Implementar `HistoriaController.Index` (texto de presentación) y `OrganizacionController.Index` (autoridades ordenadas), con sus vistas
    - _Requisitos: 3.1, 4.1, 4.2_

  - [x] 8.3 Implementar Eventos públicos
    - Implementar `EventosController.Index` (lista ordenada por proximidad) y `Detalle(id)` (título, fecha, hora, lugar, descripción, imagen opcional, indicador de cancelado), con sus vistas
    - _Requisitos: 5.1, 5.2, 6.1, 6.2, 7.1_

  - [x] 8.4 Implementar Comunicados públicos
    - Implementar `ComunicadosController.Index` (solo publicados, recientes primero) y `Detalle(id)` (oculta borradores, incluido acceso directo), con sus vistas
    - _Requisitos: 8.1, 8.2, 9.1, 32.3_

  - [x] 8.5 Implementar Galería pública
    - Implementar `GaleriaController.Index` (temas + portada), `Tema(id)` (fotos y videos) y `Foto(id)` (foto ampliada con descripción), con sus vistas
    - _Requisitos: 10.1, 10.2, 11.1, 12.1_

  - [x] 8.6 Implementar Documentos públicos y descarga
    - Implementar `DocumentosController.Index` (agrupados por categoría con enlaces) y `Descargar(id)` usando `IArchivoService.Abrir` con `FileStreamResult`
    - _Requisitos: 13.1, 14.1_

  - [x] 8.7 Implementar Contacto público y formulario
    - Implementar `ContactoController.Index` (información + formulario) y `Enviar(modelo)` [HttpPost] con Data Annotations y validación del medio de contacto; mostrar confirmación o conservar datos y errores
    - _Requisitos: 15.1, 16.1, 16.2, 16.3, 16.4, 16.5, 17.1_

- [x] 9. Checkpoint - Verificar sitio público
  - Compilar y ejecutar las pruebas; asegurar que las vistas públicas renderizan y el formulario valida correctamente; preguntar al usuario si surgen dudas.

- [x] 10. Implementar la base del Area Admin (layout, login y dashboard)
  - [x] 10.1 Crear el layout del Area Admin y el inicio de sesión
    - Crear `Areas/Admin/Views/Shared/_AdminLayout.cshtml` con el menú del panel
    - Implementar `AccountController` con `Login` [GET/POST] (`[AllowAnonymous]`) emitiendo cookie con claims de rol, y `Logout`
    - _Requisitos: 18.1, 18.2, 18.3, 19.1, 20.1_

  - [x] 10.2 Implementar el panel principal (Dashboard)
    - Implementar `DashboardController.Index` (`[Authorize]`) con accesos a cada módulo de gestión, y su vista
    - _Requisitos: 20.1_

- [x] 11. Implementar la gestión de cuentas de administrador
  - [x] 11.1 Implementar UsuariosController con política GestionCuentas
    - Implementar `Index`, `Crear`, `Editar`, `Desactivar` y `Eliminar` bajo `[Authorize(Policy="GestionCuentas")]`, con sus vistas
    - Mostrar mensaje de acción no permitida cuando el rol es Editor
    - _Requisitos: 21.1, 21.2, 22.1, 22.2, 22.3, 23.1, 23.2, 23.5_

  - [x]* 11.2 Escribir prueba basada en propiedades de gestión de cuentas por Editor
    - **Feature: sitio-web-resguardo, Property 7: Un Editor nunca puede gestionar cuentas de administrador**
    - **Validates: Requirements 21.2, 22.3, 23.5**
    - Mínimo 100 iteraciones; cubrir crear, editar, desactivar y eliminar
    - _Requisitos: 21.2, 22.3, 23.5_

- [x] 12. Implementar los módulos de gestión de contenido del Area Admin
  - [x] 12.1 Implementar Paginas (Historia) y Contacto admin
    - Implementar `PaginasController.Historia` [GET/POST] y `ContactoController.Editar` [GET/POST] (`[Authorize]`), con sus vistas
    - _Requisitos: 24.1, 40.1_

  - [x] 12.2 Implementar Autoridades admin
    - Implementar `Index`, `Crear`, `Editar`, `Ordenar` y `Eliminar` (`EliminarContenido`), con sus vistas
    - _Requisitos: 25.1, 25.2, 25.3, 25.4, 25.5_

  - [x] 12.3 Implementar Eventos admin con imagen
    - Implementar `Index`, `Crear`, `Editar`, `MarcarCancelado` y `Eliminar` (`EliminarContenido`), integrando `IArchivoService` para la imagen, con sus vistas
    - _Requisitos: 26.1, 27.1, 28.1, 28.2, 28.3, 29.1_

  - [x] 12.4 Implementar Comunicados admin con borrador y publicación
    - Implementar `Index`, `Crear`, `Editar`, `GuardarBorrador`, `Publicar` y `Eliminar` (`EliminarContenido`), con validación de borrador y transición, y sus vistas
    - _Requisitos: 30.1, 31.1, 31.2, 31.3, 32.1, 32.2, 32.4, 32.5, 32.6_

  - [x] 12.5 Implementar Galería admin con carga de medios
    - Implementar `CrearTema`, `EditarTema`, `EliminarTema`, `SubirElemento`, `EditarElemento` y `EliminarElemento`, integrando `IArchivoService` para portadas, fotos y videos, con sus vistas
    - _Requisitos: 33.1, 34.1, 34.2, 34.3, 35.1, 36.1, 36.2, 36.3_

  - [x] 12.6 Implementar Documentos admin con carga de archivos
    - Implementar `Categorias`, `CrearCategoria`, `EditarCategoria`, `EliminarCategoria`, `Subir`, `Editar` y `Eliminar`, integrando `IArchivoService`, con sus vistas
    - _Requisitos: 37.1, 37.2, 37.3, 37.4, 38.1, 39.1, 39.2, 39.3_

  - [x] 12.7 Implementar Mensajes admin
    - Implementar `Index` (recientes primero), `Ver(id)`, `MarcarAtendido` y `Eliminar` (`EliminarContenido`), con sus vistas
    - _Requisitos: 41.1, 41.2, 42.1, 42.2, 43.1, 43.2_

  - [x]* 12.8 Escribir prueba basada en propiedades de eliminación de contenido por Editor
    - **Feature: sitio-web-resguardo, Property 6: Un Editor nunca puede eliminar contenido**
    - **Validates: Requirements 23.4, 23.6, 25.5, 28.3, 31.3, 34.3, 36.3, 39.3, 43.2**
    - Mínimo 100 iteraciones; cubrir todos los tipos de contenido
    - _Requisitos: 23.4, 23.6, 25.5, 28.3, 31.3, 34.3, 36.3, 39.3, 43.2_

- [x] 13. Checkpoint - Verificar Area Admin
  - Compilar y ejecutar las pruebas; asegurar que los módulos de gestión y la autorización por rol funcionan; preguntar al usuario si surgen dudas.

- [x] 14. Escribir pruebas de integración de extremo a extremo
  - [x]* 14.1 Escribir pruebas de integración de autenticación y autorización
    - Con `WebApplicationFactory<Program>` y base sembrada: login válido/ inválido y redirección, acceso no autenticado al Area Admin redirige a login, y un Editor recibe rechazo al eliminar contenido o gestionar cuentas
    - _Requisitos: 18.1, 18.2, 18.3, 23.5, 23.6_

  - [x]* 14.2 Escribir pruebas de integración de render y descarga de documentos
    - Verificar render de inicio y vistas de detalle, y que la descarga entrega el archivo correcto con su nombre y content-type
    - _Requisitos: 1.1, 6.1, 9.1, 11.1, 12.1, 14.1, 15.1_

- [x] 15. Checkpoint final - Asegurar que todas las pruebas pasan
  - Compilar la solución completa y ejecutar todas las pruebas (unitarias, de propiedades e integración); preguntar al usuario si surgen dudas.

## Notes

- Las tareas marcadas con `*` son opcionales (pruebas) y pueden omitirse para un MVP más rápido.
- Cada tarea referencia requisitos específicos para trazabilidad.
- Los checkpoints aseguran validación incremental.
- Las pruebas basadas en propiedades validan las 12 Correctness Properties del diseño con un mínimo de 100 iteraciones usando CsCheck o FsCheck.
- Las pruebas unitarias validan ejemplos concretos y casos límite; las de integración validan controladores, autorización y descarga.

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["2.1"] },
    { "id": 2, "tasks": ["2.2", "4.1", "4.3"] },
    { "id": 3, "tasks": ["2.3", "3.1", "4.2", "4.4"] },
    { "id": 4, "tasks": ["3.2", "5.1", "5.2", "5.4", "5.6", "5.10", "5.11", "5.13", "5.17"] },
    { "id": 5, "tasks": ["5.3", "5.5", "5.7", "5.8", "5.9", "5.12", "5.14", "5.15", "5.16", "5.18", "7.1"] },
    { "id": 6, "tasks": ["8.1", "8.2", "8.3", "8.4", "8.5", "8.6", "8.7", "10.1"] },
    { "id": 7, "tasks": ["10.2", "11.1", "12.1", "12.2", "12.3", "12.4", "12.5", "12.6", "12.7"] },
    { "id": 8, "tasks": ["11.2", "12.8", "14.1", "14.2"] }
  ]
}
```
