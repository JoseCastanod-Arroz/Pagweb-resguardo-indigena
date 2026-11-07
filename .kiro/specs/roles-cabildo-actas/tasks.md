# Implementation Plan: Roles del Cabildo y Módulo de Actas

## Overview

Este plan implementa, de forma incremental y sin romper los módulos existentes, los roles del cabildo con uso digital, el módulo privado de Actas, el registro de actividad (veeduría) y el marcado financiero de documentos (tesorería), sobre el proyecto ASP.NET Core MVC (net10.0) en `PagWebCabildo/PagWebCabildo`.

El orden general es: modelo de roles y compatibilidad → entidades y migraciones → políticas de autorización → servicios → controladores y vistas del Area Admin → integración del registro de actividad → pruebas (unitarias, de propiedades e integración).

Las tareas marcadas con `*` son opcionales (pruebas). Las pruebas basadas en propiedades implementan las 5 Correctness Properties del diseño con un mínimo de 100 iteraciones cada una.

## Tasks

- [x] 1. Ampliar el modelo de roles (compatibilidad)
  - [x] 1.1 Reescribir el enum Rol
    - Ampliar `Rol` a `AdministradorGeneral=0`, `Secretario=1`, `Tesorero=2`, `Fiscal=3`, `Comisario=4` (renombrando el antiguo `Editor=1` a `Secretario=1`)
    - Actualizar todas las referencias a `Rol.Editor` en el código de producción
    - _Requisitos: 1.1, 1.2, 1.4_

  - [x]* 1.2 Actualizar las pruebas de propiedades existentes
    - Migrar `EliminarContenidoEditorPropertyTests` y `GestionCuentasEditorPropertyTests` de `Rol.Editor` a `Rol.Secretario` y demás roles no-Gobernador
    - **Feature: roles-cabildo-actas, Property 5: Ningún rol distinto de AdministradorGeneral elimina contenido ni gestiona cuentas**
    - **Validates: Requirements 2.1, 2.6**
    - _Requisitos: 2.1, 2.6_

- [x] 2. Añadir entidades y migraciones
  - [x] 2.1 Crear entidades Acta, TipoAsamblea y RegistroActividad
    - Crear `TipoAsamblea` (Ordinaria, Extraordinaria), `Acta` y `RegistroActividad` según el modelo de datos
    - Añadir `bool EsFinanciero` a `CategoriaDocumento`
    - _Requisitos: 3.1, 5.1, 6.1_

  - [x] 2.2 Registrar DbSets, relaciones y migración
    - Añadir los `DbSet<Acta>` y `DbSet<RegistroActividad>` y la relación `Acta`→`Administrador`
    - Crear la migración EF Core y aplicarla; crear `wwwroot/uploads/actas` con `.gitkeep`
    - _Requisitos: 3.1, 4.1, 5.1, 6.1_

- [x] 3. Configurar las políticas de autorización
  - [x] 3.1 Añadir las políticas de rol en Program.cs
    - Definir `GestionActas`, `ConsultaActas`, `GestionFinanciera`, `GestionMensajes`, `Veeduria` y `GestionComunicadosEventos` con los roles de la matriz de permisos
    - Conservar `GestionCuentas` y `EliminarContenido` (siguen exigiendo `AdministradorGeneral`)
    - _Requisitos: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

- [x] 4. Implementar servicios
  - [x] 4.1 Implementar IRegistroActividadService
    - Implementar `Registrar(administradorId, accion, modulo, detalle)` y `Listar()` (orden descendente por fecha)
    - _Requisitos: 5.1, 5.2, 5.3_

  - [x] 4.2 Implementar IActaService
    - Implementar `Listar` (reciente primero), `Obtener`, `Crear`, `Editar` y `AbrirArchivo`, usando `IArchivoService` para el adjunto en `uploads/actas`
    - Validar consecutivo, fecha, tipo y título
    - _Requisitos: 3.1, 3.2, 3.3, 3.4, 4.1, 4.2_

  - [x] 4.3 Extender IDocumentoService para documentos financieros
    - Añadir filtrado por categorías con `EsFinanciero = true`
    - _Requisitos: 6.1, 6.2_

  - [x]* 4.4 Escribir pruebas unitarias de los servicios nuevos
    - Probar validaciones de acta, orden de listados y registro de actividad
    - _Requisitos: 3.2, 4.1, 5.1_

  - [x]* 4.5 Escribir prueba basada en propiedades del Tesorero y documentos financieros
    - **Feature: roles-cabildo-actas, Property 4: El Tesorero solo gestiona documentos financieros**
    - **Validates: Requirements 6.2, 6.3**
    - Mínimo 100 iteraciones
    - _Requisitos: 6.2, 6.3_

- [x] 5. Implementar el módulo de Actas (Area Admin)
  - [x] 5.1 Implementar ActasController y vistas
    - `Index`, `Crear`, `Editar` bajo `[Authorize(Policy="GestionActas")]`; `Descargar` bajo `[Authorize(Policy="ConsultaActas")]`, con sus vistas
    - _Requisitos: 3.1, 4.1, 4.2, 4.4_

  - [x]* 5.2 Escribir prueba basada en propiedades de autorización de actas
    - **Feature: roles-cabildo-actas, Property 2: Solo roles autorizados gestionan o descargan actas**
    - **Validates: Requirements 3.1, 4.2, 4.4**
    - Mínimo 100 iteraciones
    - _Requisitos: 3.1, 4.2, 4.4_

  - [x]* 5.3 Escribir prueba de integración de privacidad de actas
    - **Feature: roles-cabildo-actas, Property 1: Las actas nunca son accesibles desde el sitio público**
    - **Validates: Requirements 4.3**
    - Verificar que ninguna ruta pública expone un acta ni su archivo
    - _Requisitos: 4.3_

- [x] 6. Implementar la veeduría (registro de actividad)
  - [x] 6.1 Integrar el registro de actividad en los servicios de contenido
    - Llamar a `IRegistroActividadService.Registrar(...)` en crear/editar/eliminar/publicar de los módulos de contenido
    - _Requisitos: 5.1_

  - [x] 6.2 Implementar RegistroActividadController y vista
    - `Index` bajo `[Authorize(Policy="Veeduria")]`, solo lectura
    - _Requisitos: 5.2, 5.3_

  - [x]* 6.3 Escribir prueba basada en propiedades del registro de actividad
    - **Feature: roles-cabildo-actas, Property 3: Toda acción de contenido genera un registro de actividad**
    - **Validates: Requirements 5.1**
    - Mínimo 100 iteraciones
    - _Requisitos: 5.1_

- [x] 7. Ajustar gestión de cuentas y documentos financieros (Area Admin)
  - [x] 7.1 Actualizar UsuariosController y vistas para los nuevos roles
    - Permitir asignar uno de los cinco roles al crear/editar cuentas; actualizar el selector de rol
    - _Requisitos: 1.3_

  - [x] 7.2 Marcar categorías financieras y aplicar el rol Tesorero en Documentos
    - Añadir `EsFinanciero` a los formularios de categoría; aplicar `[Authorize(Policy="GestionFinanciera")]` donde corresponda
    - _Requisitos: 6.1, 6.2, 6.3_

  - [x] 7.3 Aplicar políticas por módulo en los controladores de contenido
    - Aplicar `GestionComunicadosEventos` y `GestionMensajes` en los controladores correspondientes según la matriz de permisos
    - _Requisitos: 2.2, 2.5_

- [x] 8. Checkpoint final - Verificar e integrar
  - Compilar la solución y ejecutar todas las pruebas (unitarias, de propiedades e integración); asegurar que los módulos existentes siguen funcionando y que la autorización por rol opera según la matriz; preguntar al usuario si surgen dudas.

## Notes

- Las tareas marcadas con `*` son opcionales (pruebas) y pueden omitirse para un avance más rápido.
- Cada tarea referencia requisitos específicos para trazabilidad.
- Las pruebas basadas en propiedades validan las 5 Correctness Properties del diseño con un mínimo de 100 iteraciones.
- El renombrado `Editor`→`Secretario` preserva las Property 6 y 7 del spec original.

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2", "2.1"] },
    { "id": 2, "tasks": ["2.2", "3.1"] },
    { "id": 3, "tasks": ["4.1", "4.2", "4.3"] },
    { "id": 4, "tasks": ["4.4", "4.5", "5.1", "6.1"] },
    { "id": 5, "tasks": ["5.2", "5.3", "6.2", "7.1", "7.2", "7.3"] },
    { "id": 6, "tasks": ["6.3"] },
    { "id": 7, "tasks": ["8"] }
  ]
}
```
