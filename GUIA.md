# Guía del sitio web — Resguardo Indígena Rincón Bodega

Esta guía describe las funcionalidades del sitio para **visitantes** (público general)
y para **administradores** (gestión del contenido). El sistema es una aplicación web
ASP.NET Core MVC con dos zonas:

- **Sitio público**: abierto a cualquier visitante, sin iniciar sesión.
- **Panel de administración (Area Admin)**: protegido por inicio de sesión, donde se
  gestiona todo el contenido.

> **Acceso rápido al panel:** en la página de inicio hay un botón **"Ingresar como
> administrador"** y, en la barra de navegación, un enlace **"Ingresar"**. También
> puedes ir directo a `/Admin/Account/Login`.
>
> **Credenciales por defecto:** usuario `admin` / contraseña `Admin123*`.
> Cámbialas antes de usar el sitio en producción.

---

## 1. Funcionalidades para el visitante (sitio público)

El visitante no necesita cuenta. Desde el menú de navegación superior puede llegar a
todas las secciones.

| Sección | Ruta | Qué ofrece |
|---------|------|------------|
| **Inicio** | `/` | Presentación del resguardo, accesos a las secciones y los últimos eventos y comunicados. |
| **Historia** | `/Historia` | Texto de historia y presentación del resguardo. |
| **Organización** | `/Organizacion` | Autoridades, cargos y comités, mostrados en su orden definido. |
| **Eventos** | `/Eventos` | Lista de eventos ordenados por proximidad de fecha. |
| Detalle de evento | `/Eventos/Detalle/{id}` | Título, fecha, hora, lugar, descripción, imagen (si tiene) e indicador de "cancelado". |
| **Comunicados** | `/Comunicados` | Solo comunicados **publicados**, del más reciente al más antiguo. |
| Detalle de comunicado | `/Comunicados/Detalle/{id}` | Contenido completo del comunicado publicado. |
| **Galería** | `/Galeria` | Temas de la galería con su portada. |
| Tema de galería | `/Galeria/Tema/{id}` | Fotos y videos del tema. |
| Foto ampliada | `/Galeria/Foto/{id}` | Imagen en grande con su descripción. |
| **Documentos** | `/Documentos` | Documentos agrupados por categoría, con enlace de descarga. |
| Descargar documento | `/Documentos/Descargar/{id}` | Entrega el archivo con su nombre y tipo correctos. |
| **Contacto** | `/Contacto` | Información de contacto del cabildo y formulario para enviar un mensaje. |

### El formulario de contacto
En **Contacto** cualquier visitante puede enviar un mensaje. Reglas de validación:

- **Campos obligatorios:** nombre, medio de contacto, asunto y mensaje. Si falta alguno
  (o solo tiene espacios), el envío se rechaza y se conserva lo escrito.
- **Medio de contacto válido:** un correo (`nombre@dominio.ext`) o un teléfono de 7 a 15 dígitos.
- **Límites de longitud:** nombre hasta 100, asunto hasta 150, mensaje hasta 2000 caracteres.
- Si todo es correcto, se muestra una confirmación y el mensaje queda registrado para
  que un administrador lo revise.

### Qué NO ve el visitante
- Los comunicados en estado **borrador** (ni en el listado ni por enlace directo).
- Ninguna pantalla de administración (lo redirige al inicio de sesión).

---

## 2. Acceso de administrador

1. Entra a **`/Admin/Account/Login`** (o usa el botón/enlace "Ingresar" del sitio).
2. Escribe usuario y contraseña.
3. Si las credenciales son válidas y la cuenta está **activa**, entras al **Panel principal**.
4. Si son inválidas o la cuenta está **desactivada**, se muestra "Credenciales incorrectas"
   y no se inicia sesión.
5. Para salir, usa **Cerrar sesión** en el menú del panel.

### Roles
Hay dos roles, con permisos distintos:

| Capacidad | Administrador General | Editor |
|-----------|:---------------------:|:------:|
| Ver el panel y todos los módulos | Sí | Sí |
| Crear y editar contenido | Sí | Sí |
| **Eliminar** contenido | Sí | **No** |
| **Gestionar cuentas** de administrador | Sí | **No** |

Cuando un **Editor** intenta eliminar contenido o gestionar cuentas, el sistema lo
rechaza y muestra una pantalla de **"acción no permitida para su rol"**.

---

## 3. Funcionalidades para el administrador (Panel de administración)

Tras iniciar sesión llegas al **Panel principal** (`/Admin/Dashboard`), con accesos a
cada módulo.

### 3.1 Panel principal
- **Ruta:** `/Admin/Dashboard`
- Tarjetas/enlaces a todos los módulos de gestión.

### 3.2 Usuarios (solo Administrador General)
- **Ruta:** `/Admin/Usuarios`
- Crear, editar, **desactivar** y **eliminar** cuentas de administrador.
- Al crear/editar se define nombre, nombre de usuario (único), contraseña y rol.
- Las contraseñas se guardan cifradas (hash); nunca en texto plano.
- Desactivar una cuenta impide que inicie sesión, sin borrarla.

### 3.3 Historia (Páginas)
- **Ruta:** `/Admin/Paginas/Historia`
- Editar el texto de historia/presentación que se muestra en el sitio público.

### 3.4 Autoridades
- **Ruta:** `/Admin/Autoridades`
- Crear, editar y **eliminar** autoridades, cargos y comités.
- **Reordenar**: define el orden en que aparecen en la sección pública de Organización.

### 3.5 Eventos
- **Ruta:** `/Admin/Eventos`
- Crear y editar eventos (título, fecha, hora, lugar, descripción).
- **Imagen opcional**: se sube un archivo de imagen que se guarda y asocia al evento.
- **Marcar como cancelado**: el evento sigue visible pero con el indicador de cancelado.
- **Eliminar** evento (y su imagen).

### 3.6 Comunicados
- **Ruta:** `/Admin/Comunicados`
- Crear y editar comunicados.
- **Guardar como borrador**: exige título y contenido; el borrador NO se ve en el sitio público.
- **Publicar**: solo procede desde el estado borrador; al publicar aparece en el sitio público.
- **Eliminar** comunicado.

### 3.7 Galería
- **Ruta:** `/Admin/Galeria`
- **Temas**: crear, editar y eliminar; cada tema puede tener una imagen de **portada**.
- **Elementos**: subir **fotos y videos** a un tema, editarlos y eliminarlos.
- Las portadas, fotos y videos se guardan como archivos y se asocian a su tema.

### 3.8 Documentos
- **Ruta:** `/Admin/Documentos`
- **Categorías**: crear, editar y eliminar categorías de documentos.
- **Documentos**: subir un archivo con título, descripción y categoría; editar (incluido
  reemplazar el archivo) y eliminar.
- En el sitio público los documentos aparecen agrupados por su categoría con enlace de descarga.

### 3.9 Contacto
- **Ruta:** `/Admin/Contacto/Editar`
- Editar la información de contacto (dirección, teléfono, correo, otros datos) que se
  muestra en la sección pública de Contacto.

### 3.10 Mensajes
- **Ruta:** `/Admin/Mensajes`
- Ver la lista de mensajes recibidos por el formulario de contacto, del más reciente al más antiguo.
- Abrir el **detalle** de un mensaje (`/Admin/Mensajes/Ver/{id}`).
- **Marcar como atendido**.
- **Eliminar** mensaje.

---

## 4. Tipos de archivo permitidos (subidas)

| Uso | Extensiones aceptadas |
|-----|-----------------------|
| Imágenes (eventos, portadas, fotos) | `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` |
| Videos (galería) | `.mp4`, `.webm` |
| Documentos | `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.odt`, `.txt` |

Un archivo con una extensión no permitida se rechaza con un mensaje de error.

---

## 5. Cómo ejecutar el sitio

Desde la carpeta del proyecto:

```bash
cd PagWebCabildo
dotnet run
```

Luego abre en el navegador la dirección que indique la consola (por ejemplo
`http://localhost:5209/`). La base de datos SQLite (`app.db`) y el administrador inicial
se crean automáticamente al primer arranque.

---

## 6. Notas de seguridad

- Cambia la contraseña por defecto (`Admin123*`) cuanto antes.
- El acceso al panel está protegido por inicio de sesión; las acciones sensibles
  (eliminar contenido, gestionar cuentas) están reservadas al Administrador General.
- Este es un boceto funcional: no incluye políticas avanzadas de contraseña, HTTPS
  forzado en producción, expiración de sesión ni recuperación de contraseña.
