# Design Document

## Overview

Esta feature evoluciona el Panel de Administración para representar los **cargos del cabildo con uso digital** como roles del sistema, cada uno con permisos propios, e introduce un **módulo privado de Actas**, un **registro de actividad** (veeduría) y el **marcado financiero de documentos** (tesorería).

El diseño se apoya en la arquitectura existente (ASP.NET Core MVC, EF Core + SQLite, autenticación por cookies y autorización por políticas) y busca el menor impacto posible, extendiendo el enum `Rol`, las políticas de `Program.cs` y añadiendo entidades/controladores nuevos sin romper los módulos actuales.

## Architecture

Se mantiene la arquitectura en capas actual:
- **Entidades** (`Models/Entidades`): se amplía `Rol`, se añaden `Acta`, `TipoAsamblea` y `RegistroActividad`, y se extiende `CategoriaDocumento` con el marcado financiero.
- **DbContext** (`Data/AppDbContext`): nuevos `DbSet`, relaciones y migraciones.
- **Servicios** (`Services`): `IActaService`, `IRegistroActividadService`; extensión de `IDocumentoService`.
- **Controladores Area Admin**: `ActasController`, `RegistroActividadController`; ajustes en `UsuariosController` y `DocumentosController`.
- **Autorización**: nuevas políticas en `Program.cs`.
- **Almacenamiento de archivos**: las actas se guardan en `wwwroot/uploads/actas` mediante el `IArchivoService` existente.

### Modelo de Roles

Se reemplaza el enum `Rol` de dos valores por uno de cinco, conservando `AdministradorGeneral` como el rol del Gobernador para compatibilidad.

```
enum Rol {
    AdministradorGeneral = 0,  // Gobernador
    Secretario = 1,            // reemplaza el antiguo "Editor"
    Tesorero = 2,
    Fiscal = 3,
    Comisario = 4
}
```

Decisión de compatibilidad: el antiguo valor `Editor = 1` se renombra a `Secretario = 1`. Las cuentas existentes con valor 1 pasan a ser Secretario, lo que preserva su capacidad de gestionar contenidos. No requiere reasignación numérica de datos.

### Matriz de permisos

| Acción / Política            | Gobernador | Secretario | Tesorero | Fiscal | Comisario |
|------------------------------|:---------:|:----------:|:--------:|:------:|:---------:|
| Gestionar cuentas            | ✔ | ✘ | ✘ | ✘ | ✘ |
| Eliminar contenido           | ✔ | ✘ | ✘ | ✘ | ✘ |
| Editar presentación/autoridades/contacto | ✔ | ✘ | ✘ | ✘ | ✘ |
| Comunicados (crear/editar)   | ✔ | ✔ | ✘ | ✘ | ✔ |
| Eventos (crear/editar)       | ✔ | ✔ | ✘ | ✘ | ✔ |
| Galería (gestionar)          | ✔ | ✔ | ✘ | ✘ | ✘ |
| Documentos (gestionar)       | ✔ | ✔ | ✘ | ✘ | ✘ |
| Documentos financieros       | ✔ | ✘ | ✔ | ✘ | ✘ |
| Actas (crear/editar)         | ✔ | ✔ | ✘ | ✘ | ✘ |
| Actas (consultar/descargar)  | ✔ | ✔ | ✘ | ✔ | ✘ |
| Mensajes de contacto         | ✔ | ✘ | ✘ | ✘ | ✔ |
| Registro de actividad (ver)  | ✔ | ✘ | ✘ | ✔ | ✘ |

## Components and Interfaces

### Autorización (Program.cs)
Políticas nuevas, además de `GestionCuentas` y `EliminarContenido` (que siguen exigiendo `AdministradorGeneral`):
- `GestionActas`: AdministradorGeneral, Secretario.
- `ConsultaActas`: AdministradorGeneral, Secretario, Fiscal.
- `GestionFinanciera`: AdministradorGeneral, Tesorero.
- `GestionMensajes`: AdministradorGeneral, Comisario.
- `Veeduria`: AdministradorGeneral, Fiscal.
- `GestionComunicadosEventos`: AdministradorGeneral, Secretario, Comisario.

## Data Models

### Entidad `Acta`
```
Acta {
    int Id
    int Consecutivo
    DateOnly FechaAsamblea
    TipoAsamblea Tipo          // Ordinaria | Extraordinaria
    string Titulo
    string Descripcion
    string ArchivoRuta         // uploads/actas
    int RegistradoPorId        // FK Administrador
    DateTime FechaRegistro
}
enum TipoAsamblea { Ordinaria = 0, Extraordinaria = 1 }
```

### Entidad `RegistroActividad`
```
RegistroActividad {
    int Id
    int AdministradorId
    string Accion              // Crear | Editar | Eliminar | Publicar
    string Modulo              // Comunicado | Evento | Acta | ...
    string Detalle
    DateTime FechaHora
}
```

### Documentos financieros
- Se añade `bool EsFinanciero` a `CategoriaDocumento` (marcado a nivel de categoría).
- El Tesorero gestiona documentos cuya categoría tiene `EsFinanciero = true`.

## Services and Controllers

### Servicios
- `IActaService`: `Listar()`, `Obtener(id)`, `Crear(acta, archivo)`, `Editar(acta, archivo?)`, `AbrirArchivo(id)`.
- `IRegistroActividadService`: `Registrar(administradorId, accion, modulo, detalle)`, `Listar()`.
- `IDocumentoService`: se extiende para filtrar por categorías financieras.

### Controladores
- `ActasController` (Area Admin): `Index`/`Crear`/`Editar` bajo `GestionActas`; `Descargar` bajo `ConsultaActas`.
- `RegistroActividadController` (Area Admin): `Index` bajo `Veeduria`.

## Error Handling

- Carga de actas: validar extensión permitida (PDF/DOC/DOCX/ODT/TXT) vía `IArchivoService`; si falla, se vuelve al formulario con mensaje.
- Consecutivo de acta: validar presencia; se recomienda unicidad (índice) y, ante duplicado, mostrar error de validación.
- Descarga de acta inexistente o sin archivo físico: devolver 404.
- Acceso sin rol: redirección a `AccesoDenegado` (comportamiento existente de la política de cookies).
- Registro de actividad: su escritura no debe interrumpir la operación principal; si falla el registro, se registra el error pero la acción de contenido se completa.

## Testing Strategy

- **Pruebas unitarias**: `IActaService` (crear/editar/validaciones), `IRegistroActividadService` (registrar/listar), filtro financiero de documentos.
- **Pruebas basadas en propiedades**: ver Correctness Properties (mínimo 100 iteraciones cada una).
- **Actualización de pruebas existentes**: `EliminarContenidoEditorPropertyTests` y `GestionCuentasEditorPropertyTests` se migran de `Rol.Editor` a `Rol.Secretario` (y demás roles no-Gobernador) conservando su intención.
- **Pruebas de integración**: un acta no es accesible públicamente; descarga de acta solo con rol autorizado.

## Correctness Properties

### Property 1: Las actas nunca son accesibles desde el sitio público
Ninguna ruta del Sitio_Publico expone un acta ni su archivo mediante enlace.

**Validates: Requirements 4.3**

### Property 2: Solo roles autorizados gestionan o descargan actas
Solo roles con `GestionActas` crean/editan actas; solo `ConsultaActas` las descarga.

**Validates: Requirements 3.1, 4.2, 4.4**

### Property 3: Toda acción de contenido genera un registro de actividad
Cada operación de crear/editar/eliminar/publicar en módulos de contenido produce exactamente un `RegistroActividad`.

**Validates: Requirements 5.1**

### Property 4: El Tesorero solo gestiona documentos financieros
El rol Tesorero solo puede crear/editar/subir documentos de categorías con `EsFinanciero = true`.

**Validates: Requirements 6.2, 6.3**

### Property 5: Ningún rol distinto de AdministradorGeneral elimina contenido ni gestiona cuentas
Preserva las Property 6 y 7 del spec original tras renombrar Editor a Secretario.

**Validates: Requirements 2.1, 2.6**
