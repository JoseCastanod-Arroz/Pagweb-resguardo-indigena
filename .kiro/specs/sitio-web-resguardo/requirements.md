# Requirements Document

## Introduction

Este documento especifica los requisitos funcionales del sitio web del Resguardo Indígena Rincón Bodega. El sistema se compone de un sitio público, donde cualquier visitante puede consultar información del resguardo (historia, organización, eventos, comunicados, galería, documentos y contacto) y enviar mensajes al cabildo, y un panel de administración, donde los administradores autorizados gestionan el contenido del sitio y las cuentas de administrador.

El desarrollo utiliza ASP.NET Core MVC (net10.0) sobre el proyecto inicial ubicado en `PagWebCabildo/PagWebCabildo`. Esta es la primera versión del documento y corresponde a un boceto funcional: define el comportamiento esperado de las funciones del sistema sin abordar aspectos excluidos del alcance (ver sección "Alcance y No-Objetivos").

El idioma del sitio es español. Las palabras en lengua Pijao se ingresan como texto plano dentro de los contenidos.

## Alcance y No-Objetivos

Las siguientes áreas se excluyen explícitamente de esta versión y NO generan requisitos. Se registran como restricciones y no-objetivos:

- **Seguridad**: HTTPS, cifrado, protección frente a ataques, políticas de contraseñas y manejo de cookies.
- **Atributos no funcionales**: rendimiento, accesibilidad, compatibilidad, copias de seguridad y disponibilidad.
- **Diseño visual**: CSS, colores, tipografía y diseño responsivo (se realizarán manualmente en una etapa posterior).
- **Zona privada**: no existe zona privada para miembros de la comunidad; todo el contenido es público.
- **Multilenguaje**: el sitio opera únicamente en español.
- **Funciones adicionales**: recuperación de contraseña, registro de cambios (historial de auditoría), notificaciones y programación de publicaciones.
- **Despliegue**: hospedaje, dominio y capacitación.

## Supuestos

- Todo el contenido del sitio es público; no hay zona privada en esta versión.
- El cabildo provee y autoriza el contenido que se publica.
- Los administradores inician sesión con nombre de usuario y contraseña.
- El idioma del sitio es español; las palabras en lengua Pijao se ingresan como texto plano.

## Glossary

- **Sistema**: La aplicación web completa del Resguardo Indígena Rincón Bodega.
- **Sitio_Publico**: Conjunto de páginas accesibles a cualquier visitante sin autenticación.
- **Panel_Admin**: Conjunto de pantallas accesibles únicamente a administradores autenticados.
- **Visitante**: Persona que accede al sitio público sin autenticación para consultar contenido y enviar mensajes.
- **Administrador_General**: Usuario autenticado con control total; gestiona contenido y cuentas de administrador, y elimina contenido.
- **Editor**: Usuario autenticado que crea y edita contenido, pero no gestiona cuentas de administrador ni elimina contenido.
- **Administrador**: Cualquier usuario autenticado del Panel_Admin (Administrador_General o Editor).
- **Evento**: Actividad del resguardo con título, fecha, hora, lugar, descripción e imagen opcional.
- **Comunicado**: Aviso o noticia con título, fecha y contenido.
- **Tema_Galeria**: Agrupación de fotos y videos identificada por nombre, descripción e imagen de portada.
- **Elemento_Galeria**: Foto o video que pertenece a un Tema_Galeria.
- **Documento**: Archivo descargable con título, descripción y categoría.
- **Categoria_Documento**: Agrupación de documentos y trámites.
- **Autoridad**: Autoridad, cargo o comité que forma parte de la organización del resguardo.
- **Mensaje**: Comunicación enviada por un Visitante mediante el formulario de contacto.
- **Informacion_Contacto**: Datos de contacto del cabildo mostrados en el sitio público.
- **Presentacion_Resguardo**: Texto de la historia y presentación del resguardo.

---

## Requirements

### Módulo: Sitio Público

### Requisito 1: Página de inicio (RF-P01) [Alta]

**Historia de Usuario:** Como Visitante, quiero una página de inicio que presente el resguardo, me dé acceso a cada sección y muestre los últimos eventos y comunicados, para orientarme y acceder rápidamente al contenido relevante.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la página de inicio, THE Sitio_Publico SHALL mostrar una presentación del resguardo.
2. WHEN un Visitante accede a la página de inicio, THE Sitio_Publico SHALL mostrar accesos a cada sección pública.
3. WHEN un Visitante accede a la página de inicio, THE Sitio_Publico SHALL mostrar los últimos eventos registrados.
4. WHEN un Visitante accede a la página de inicio, THE Sitio_Publico SHALL mostrar los últimos comunicados registrados.

### Requisito 2: Menú de navegación (RF-P02) [Alta]

**Historia de Usuario:** Como Visitante, quiero un menú de navegación disponible en todas las páginas, para acceder a cualquier sección pública desde donde me encuentre.

#### Criterios de Aceptación

1. THE Sitio_Publico SHALL mostrar un menú de navegación con acceso a todas las secciones públicas.
2. WHEN un Visitante se encuentra en cualquier página pública, THE Sitio_Publico SHALL mostrar el menú de navegación en esa página.
3. WHEN un Visitante selecciona una sección en el menú de navegación, THE Sitio_Publico SHALL mostrar la página de la sección seleccionada.

### Requisito 3: Historia y presentación del resguardo (RF-P03) [Alta]

**Historia de Usuario:** Como Visitante, quiero una página con la historia y presentación del resguardo, para conocer su origen e identidad.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de historia, THE Sitio_Publico SHALL mostrar el texto de la Presentacion_Resguardo.

### Requisito 4: Organización del resguardo (RF-P04) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver la organización del resguardo, para conocer sus autoridades, cargos y comités.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de organización, THE Sitio_Publico SHALL mostrar las Autoridades, cargos y comités del resguardo.
2. THE Sitio_Publico SHALL mostrar las Autoridades en el orden definido en el Panel_Admin.

### Requisito 5: Listado de eventos (RF-P05) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver la lista de eventos con los próximos primero, para identificar fácilmente las actividades que vienen.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de eventos, THE Sitio_Publico SHALL mostrar la lista de Eventos.
2. WHEN un Visitante accede a la sección de eventos, THE Sitio_Publico SHALL ordenar los Eventos mostrando primero los de fecha más próxima.

### Requisito 6: Detalle de un evento (RF-P06) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver el detalle de un evento, para conocer toda su información.

#### Criterios de Aceptación

1. WHEN un Visitante selecciona un Evento, THE Sitio_Publico SHALL mostrar el título, la fecha, la hora, el lugar y la descripción del Evento.
2. WHERE el Evento tiene una imagen asociada, THE Sitio_Publico SHALL mostrar la imagen del Evento.

### Requisito 7: Indicación de evento cancelado (RF-P07) [Media]

**Historia de Usuario:** Como Visitante, quiero saber si un evento está cancelado, para no asistir a una actividad que no se realizará.

#### Criterios de Aceptación

1. WHERE un Evento está marcado como cancelado, THE Sitio_Publico SHALL indicar que el Evento está cancelado.

### Requisito 8: Listado de comunicados (RF-P08) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver la lista de comunicados del más reciente al más antiguo, para enterarme primero de lo más actual.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de comunicados, THE Sitio_Publico SHALL mostrar la lista de Comunicados publicados.
2. WHEN un Visitante accede a la sección de comunicados, THE Sitio_Publico SHALL ordenar los Comunicados del más reciente al más antiguo.

### Requisito 9: Detalle de un comunicado (RF-P09) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver el detalle de un comunicado, para leer su contenido completo.

#### Criterios de Aceptación

1. WHEN un Visitante selecciona un Comunicado, THE Sitio_Publico SHALL mostrar el título, la fecha y el contenido del Comunicado.

### Requisito 10: Listado de temas de galería (RF-P10) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver los temas de la galería con su imagen de portada, para elegir qué conjunto de fotos y videos explorar.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de galería, THE Sitio_Publico SHALL mostrar la lista de Temas_Galeria.
2. WHEN un Visitante accede a la sección de galería, THE Sitio_Publico SHALL mostrar la imagen de portada de cada Tema_Galeria.

### Requisito 11: Contenido de un tema de galería (RF-P11) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver las fotos y videos de un tema de galería, para conocer el material asociado a ese tema.

#### Criterios de Aceptación

1. WHEN un Visitante selecciona un Tema_Galeria, THE Sitio_Publico SHALL mostrar las fotos y los videos que pertenecen a ese Tema_Galeria.

### Requisito 12: Vista ampliada de foto (RF-P12) [Media]

**Historia de Usuario:** Como Visitante, quiero ver una foto ampliada con su descripción, para apreciar el detalle y su contexto.

#### Criterios de Aceptación

1. WHEN un Visitante selecciona una foto de un Tema_Galeria, THE Sitio_Publico SHALL mostrar la foto ampliada junto con su descripción.

### Requisito 13: Listado de documentos y trámites por categoría (RF-P13) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver los documentos y trámites agrupados por categoría, para encontrar fácilmente lo que necesito.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de documentos, THE Sitio_Publico SHALL mostrar los Documentos agrupados por Categoria_Documento.

### Requisito 14: Descarga de documento (RF-P14) [Alta]

**Historia de Usuario:** Como Visitante, quiero descargar un documento, para consultarlo o utilizarlo fuera del sitio.

#### Criterios de Aceptación

1. WHEN un Visitante selecciona descargar un Documento, THE Sitio_Publico SHALL entregar el archivo del Documento.

### Requisito 15: Información de contacto del cabildo (RF-P15) [Alta]

**Historia de Usuario:** Como Visitante, quiero ver la información de contacto del cabildo, para comunicarme con el resguardo.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de contacto, THE Sitio_Publico SHALL mostrar la Informacion_Contacto del cabildo.

### Requisito 16: Formulario de contacto (RF-P16) [Alta]

**Historia de Usuario:** Como Visitante, quiero un formulario de contacto, para enviar un mensaje al cabildo.

#### Criterios de Aceptación

1. WHEN un Visitante accede a la sección de contacto, THE Sitio_Publico SHALL mostrar un formulario con los campos nombre (obligatorio, de 1 a 100 caracteres), medio de contacto (obligatorio), asunto (obligatorio, de 1 a 150 caracteres) y mensaje (obligatorio, de 1 a 2000 caracteres).
2. THE Sitio_Publico SHALL aceptar como medio de contacto un correo electrónico con formato nombre@dominio.extensión o un número de teléfono de 7 a 15 dígitos.
3. WHEN un Visitante envía el formulario con los cuatro campos obligatorios completos y dentro de los límites de longitud definidos, THE Sitio_Publico SHALL registrar el Mensaje.
4. IF un Visitante envía el formulario con uno o más campos obligatorios vacíos, THEN THE Sitio_Publico SHALL rechazar el envío, conservar los datos ya ingresados e indicar mediante un mensaje cuáles campos obligatorios faltan.
5. IF un Visitante envía el formulario con un medio de contacto que no cumple el formato de correo electrónico ni el de número de teléfono, THEN THE Sitio_Publico SHALL rechazar el envío, conservar los datos ya ingresados e indicar mediante un mensaje que el medio de contacto no tiene un formato válido.

### Requisito 17: Confirmación de mensaje enviado (RF-P17) [Alta]

**Historia de Usuario:** Como Visitante, quiero recibir una confirmación cuando envío mi mensaje, para saber que se registró correctamente.

#### Criterios de Aceptación

1. WHEN un Mensaje se registra correctamente, THE Sitio_Publico SHALL mostrar al Visitante una confirmación de que el Mensaje fue enviado.

---

### Módulo: Panel de Administración

### Requisito 18: Inicio de sesión de administrador (RF-A01) [Alta]

**Historia de Usuario:** Como Administrador, quiero iniciar sesión con usuario y contraseña, para acceder al panel de administración.

#### Criterios de Aceptación

1. WHEN un Administrador envía un nombre de usuario y una contraseña válidos, THE Panel_Admin SHALL crear una sesión autenticada y redirigir al Administrador a la pantalla principal del Panel_Admin.
2. IF un Administrador envía un nombre de usuario o una contraseña inválidos, THEN THE Panel_Admin SHALL rechazar el acceso, no crear ninguna sesión, mantener al usuario en la pantalla de inicio de sesión y mostrar un mensaje que indique que las credenciales son incorrectas.
3. IF un usuario no autenticado intenta acceder a una pantalla del Panel_Admin, THEN THE Panel_Admin SHALL redirigir al inicio de sesión sin exponer el contenido de la pantalla solicitada.

### Requisito 19: Cierre de sesión (RF-A02) [Alta]

**Historia de Usuario:** Como Administrador, quiero cerrar sesión, para finalizar mi acceso al panel de administración.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado selecciona cerrar sesión, THE Panel_Admin SHALL finalizar la sesión del Administrador.

### Requisito 20: Panel principal (RF-A03) [Alta]

**Historia de Usuario:** Como Administrador, quiero un panel principal con acceso a cada módulo de gestión, para navegar al módulo que necesito administrar.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado accede al Panel_Admin, THE Panel_Admin SHALL mostrar un panel principal con acceso a cada módulo de gestión.

### Requisito 21: Creación de administradores (RF-A04) [Alta]

**Historia de Usuario:** Como Administrador_General, quiero crear cuentas de administrador, para dar acceso al panel a nuevas personas autorizadas.

#### Criterios de Aceptación

1. WHEN un Administrador_General registra un administrador con nombre, nombre de usuario, contraseña y rol, THE Panel_Admin SHALL crear la cuenta de Administrador.
2. IF un Editor intenta crear una cuenta de administrador, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 22: Edición y desactivación de administradores (RF-A05) [Media]

**Historia de Usuario:** Como Administrador_General, quiero editar y desactivar cuentas de administrador, para mantener actualizado el acceso al panel.

#### Criterios de Aceptación

1. WHEN un Administrador_General edita una cuenta de Administrador, THE Panel_Admin SHALL guardar los cambios de la cuenta.
2. WHEN un Administrador_General desactiva una cuenta de Administrador, THE Panel_Admin SHALL impedir el inicio de sesión de esa cuenta.
3. IF un Editor intenta editar o desactivar una cuenta de administrador, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 23: Roles de administrador (RF-A06) [Media]

**Historia de Usuario:** Como Administrador_General, quiero dos roles con permisos diferenciados, para controlar quién gestiona usuarios y quién elimina contenido.

#### Criterios de Aceptación

1. THE Panel_Admin SHALL asignar a cada cuenta de administrador exactamente uno de los dos roles Administrador_General o Editor.
2. WHERE el rol es Administrador_General, THE Panel_Admin SHALL permitir la gestión de cuentas de administrador (crear, editar y eliminar cuentas de administrador).
3. WHERE el rol es Administrador_General, THE Panel_Admin SHALL permitir la eliminación de contenido.
4. WHERE el rol es Editor, THE Panel_Admin SHALL permitir únicamente la creación y edición de contenido, sin acceso a la gestión de cuentas de administrador ni a la eliminación de contenido.
5. IF un Editor intenta gestionar cuentas de administrador, THEN THE Panel_Admin SHALL rechazar la operación, no realizar ningún cambio en las cuentas y mostrar un mensaje que indique que la acción no está permitida para su rol.
6. IF un Editor intenta eliminar contenido, THEN THE Panel_Admin SHALL rechazar la operación, conservar el contenido sin cambios y mostrar un mensaje que indique que la acción no está permitida para su rol.

### Requisito 24: Edición de historia y presentación (RF-A07) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar el texto de la historia y presentación del resguardo, para mantener actualizada esa información.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado guarda cambios en el texto de la Presentacion_Resguardo, THE Panel_Admin SHALL actualizar el texto de la Presentacion_Resguardo.

### Requisito 25: Gestión de organización (RF-A08) [Alta]

**Historia de Usuario:** Como Administrador, quiero crear, editar, ordenar y eliminar autoridades, cargos y comités, para mantener actualizada la organización del resguardo.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado crea una Autoridad, THE Panel_Admin SHALL registrar la Autoridad.
2. WHEN un Administrador autenticado edita una Autoridad, THE Panel_Admin SHALL guardar los cambios de la Autoridad.
3. WHEN un Administrador autenticado define el orden de las Autoridades, THE Panel_Admin SHALL guardar el orden definido.
4. WHEN un Administrador_General elimina una Autoridad, THE Panel_Admin SHALL eliminar la Autoridad.
5. IF un Editor intenta eliminar una Autoridad, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 26: Creación de eventos (RF-A09) [Alta]

**Historia de Usuario:** Como Administrador, quiero crear un evento con su información, para publicarlo en el sitio público.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado crea un Evento con título, fecha, hora, lugar y descripción, THE Panel_Admin SHALL registrar el Evento.

### Requisito 27: Imagen de evento (RF-A10) [Alta]

**Historia de Usuario:** Como Administrador, quiero adjuntar una imagen a un evento, para enriquecer su presentación.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado adjunta una imagen a un Evento, THE Panel_Admin SHALL asociar la imagen al Evento.

### Requisito 28: Edición y eliminación de eventos (RF-A11) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar y eliminar eventos, para corregir o retirar actividades del sitio.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado edita un Evento, THE Panel_Admin SHALL guardar los cambios del Evento.
2. WHEN un Administrador_General elimina un Evento, THE Panel_Admin SHALL eliminar el Evento.
3. IF un Editor intenta eliminar un Evento, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 29: Marcar evento como cancelado (RF-A12) [Media]

**Historia de Usuario:** Como Administrador, quiero marcar un evento como cancelado, para informar a los visitantes que no se realizará.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado marca un Evento como cancelado, THE Panel_Admin SHALL registrar el Evento como cancelado.

### Requisito 30: Creación de comunicados (RF-A13) [Alta]

**Historia de Usuario:** Como Administrador, quiero crear un comunicado con su información, para publicar avisos y noticias.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado crea un Comunicado con título, fecha y contenido, THE Panel_Admin SHALL registrar el Comunicado.

### Requisito 31: Edición y eliminación de comunicados (RF-A14) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar y eliminar comunicados, para corregir o retirar avisos del sitio.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado edita un Comunicado, THE Panel_Admin SHALL guardar los cambios del Comunicado.
2. WHEN un Administrador_General elimina un Comunicado, THE Panel_Admin SHALL eliminar el Comunicado.
3. IF un Editor intenta eliminar un Comunicado, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 32: Comunicado como borrador (RF-A15) [Media]

**Historia de Usuario:** Como Administrador, quiero guardar un comunicado como borrador y publicarlo después, para prepararlo antes de hacerlo visible.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado guarda un Comunicado como borrador, THE Panel_Admin SHALL registrar el Comunicado con estado de borrador y conservar su contenido para edición posterior.
2. IF un Administrador autenticado intenta guardar como borrador un Comunicado sin título o sin contenido, THEN THE Panel_Admin SHALL rechazar la operación, conservar los datos ingresados y mostrar un mensaje de error que indique los campos obligatorios faltantes.
3. WHILE un Comunicado está en estado de borrador, THE Sitio_Publico SHALL ocultar el Comunicado de todas las vistas públicas, incluyendo listados y acceso por enlace directo.
4. WHEN un Administrador autenticado publica un Comunicado en estado de borrador, THE Panel_Admin SHALL cambiar el estado del Comunicado a publicado.
5. WHEN un Comunicado cambia a estado publicado, THE Sitio_Publico SHALL mostrar el Comunicado en las vistas públicas.
6. IF un Administrador autenticado intenta publicar un Comunicado que no está en estado de borrador, THEN THE Panel_Admin SHALL rechazar la operación, mantener el estado actual del Comunicado y mostrar un mensaje de error que indique que el Comunicado no puede publicarse desde su estado actual.

### Requisito 33: Creación de temas de galería (RF-A16) [Alta]

**Historia de Usuario:** Como Administrador, quiero crear un tema de galería con nombre, descripción y portada, para organizar las fotos y videos.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado crea un Tema_Galeria con nombre, descripción e imagen de portada, THE Panel_Admin SHALL registrar el Tema_Galeria.

### Requisito 34: Edición y eliminación de temas de galería (RF-A17) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar y eliminar temas de galería, para mantener organizada la galería.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado edita un Tema_Galeria, THE Panel_Admin SHALL guardar los cambios del Tema_Galeria.
2. WHEN un Administrador_General elimina un Tema_Galeria, THE Panel_Admin SHALL eliminar el Tema_Galeria.
3. IF un Editor intenta eliminar un Tema_Galeria, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 35: Carga de fotos y videos (RF-A18) [Alta]

**Historia de Usuario:** Como Administrador, quiero subir fotos y videos a un tema, para alimentar la galería con material del resguardo.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado sube una foto o un video a un Tema_Galeria, THE Panel_Admin SHALL asociar el Elemento_Galeria al Tema_Galeria.

### Requisito 36: Edición y eliminación de elementos de galería (RF-A19) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar la descripción de un elemento y eliminarlo, para mantener actualizado el contenido de la galería.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado edita la descripción de un Elemento_Galeria, THE Panel_Admin SHALL guardar la descripción del Elemento_Galeria.
2. WHEN un Administrador_General elimina un Elemento_Galeria, THE Panel_Admin SHALL eliminar el Elemento_Galeria.
3. IF un Editor intenta eliminar un Elemento_Galeria, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 37: Gestión de categorías de documentos (RF-A20) [Alta]

**Historia de Usuario:** Como Administrador, quiero crear, editar y eliminar categorías de documentos, para organizar los documentos y trámites.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado crea una Categoria_Documento, THE Panel_Admin SHALL registrar la Categoria_Documento.
2. WHEN un Administrador autenticado edita una Categoria_Documento, THE Panel_Admin SHALL guardar los cambios de la Categoria_Documento.
3. WHEN un Administrador_General elimina una Categoria_Documento, THE Panel_Admin SHALL eliminar la Categoria_Documento.
4. IF un Editor intenta eliminar una Categoria_Documento, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 38: Carga de documentos (RF-A21) [Alta]

**Historia de Usuario:** Como Administrador, quiero subir un documento con título, descripción y categoría, para ponerlo a disposición de los visitantes.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado sube un Documento con título, descripción y Categoria_Documento, THE Panel_Admin SHALL registrar el Documento.

### Requisito 39: Edición y eliminación de documentos (RF-A22) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar y eliminar documentos, para corregir o retirar material del sitio.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado edita un Documento, THE Panel_Admin SHALL guardar los cambios del Documento.
2. WHEN un Administrador_General elimina un Documento, THE Panel_Admin SHALL eliminar el Documento.
3. IF un Editor intenta eliminar un Documento, THEN THE Panel_Admin SHALL rechazar la operación.

### Requisito 40: Edición de información de contacto (RF-A23) [Alta]

**Historia de Usuario:** Como Administrador, quiero editar la información de contacto del cabildo, para mantenerla actualizada en el sitio público.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado guarda cambios en la Informacion_Contacto, THE Panel_Admin SHALL actualizar la Informacion_Contacto.

### Requisito 41: Listado de mensajes recibidos (RF-A24) [Alta]

**Historia de Usuario:** Como Administrador, quiero ver la lista de mensajes recibidos del más reciente al más antiguo, para atender primero lo más actual.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado accede al módulo de mensajes, THE Panel_Admin SHALL mostrar la lista de Mensajes recibidos.
2. WHEN un Administrador autenticado accede al módulo de mensajes, THE Panel_Admin SHALL ordenar los Mensajes del más reciente al más antiguo.

### Requisito 42: Ver mensaje y marcar como atendido (RF-A25) [Media]

**Historia de Usuario:** Como Administrador, quiero ver un mensaje y marcarlo como atendido, para llevar el seguimiento de las comunicaciones recibidas.

#### Criterios de Aceptación

1. WHEN un Administrador autenticado selecciona un Mensaje, THE Panel_Admin SHALL mostrar el contenido del Mensaje.
2. WHEN un Administrador autenticado marca un Mensaje como atendido, THE Panel_Admin SHALL registrar el Mensaje como atendido.

### Requisito 43: Eliminación de mensajes (RF-A26) [Media]

**Historia de Usuario:** Como Administrador, quiero eliminar mensajes, para retirar comunicaciones que ya no se necesitan.

#### Criterios de Aceptación

1. WHEN un Administrador_General elimina un Mensaje, THE Panel_Admin SHALL eliminar el Mensaje.
2. IF un Editor intenta eliminar un Mensaje, THEN THE Panel_Admin SHALL rechazar la operación.
