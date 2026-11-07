# Requirements Document

## Introduction

Este documento especifica los requisitos de la evolución del Panel de Administración del sitio del Resguardo Indígena Rincón Bodega, orientada a reflejar los **cargos reales del cabildo** como roles del sistema, cada uno con un conjunto de funcionalidades digitales propias. Además, introduce un **módulo privado de Actas** para que la Secretaría registre y archive las actas de asamblea, conforme a sus funciones en el Reglamento Interno.

Esta feature amplía el sistema existente (ASP.NET Core MVC, net10.0), que hoy cuenta con dos roles técnicos: `AdministradorGeneral` y `Editor`. Se parte del boceto validado con el cabildo, en el que solo algunos cargos requieren herramientas digitales; los cargos de labor física o de campo se descartan como roles del sistema.

El idioma del sistema es español.

## Alcance y No-Objetivos

Dentro del alcance:
- Modelo de roles basado en los cargos del cabildo con uso digital.
- Permisos (autorización) diferenciados por rol.
- Módulo de Actas privado (solo Panel_Admin), con carga de archivo.
- Registro de actividad (auditoría básica) para el rol de veeduría.
- Marcado/categoría financiera de documentos para el rol de tesorería.

Fuera del alcance (no generan requisitos en esta versión):
- Recuperación de contraseña y autenticación de doble factor.
- Firma digital o validación legal de las actas.
- Publicación de actas en el sitio público (las actas son internas).
- Rediseño visual (se mantiene el estilo actual del panel).
- Notificaciones por correo.

## Supuestos

- Los cargos del cabildo ya existen como registros de `Autoridad` (sembrados).
- Cada usuario del Panel_Admin tiene exactamente un rol.
- Las actas son documentos internos y nunca se exponen en el sitio público.
- El Gobernador Suplente no es un rol propio: cuando reemplaza al Gobernador, usa una cuenta con el rol de Gobernador.

## Glossary

- **Rol**: Conjunto de permisos asociado a un cargo del cabildo con uso digital.
- **Gobernador (Administrador General)**: Rol con control total del panel, cuentas y contenido.
- **Secretario**: Rol que gestiona contenidos (comunicados, eventos, documentos, galería) y el módulo de Actas.
- **Tesorero**: Rol que gestiona documentos financieros; lectura del resto del contenido.
- **Fiscal (Veeduría)**: Rol de solo lectura que además consulta el registro de actividad.
- **Comisario**: Rol que gestiona mensajes de contacto y crea convocatorias (eventos/comunicados).
- **Acta**: Registro interno de una asamblea, con consecutivo, fecha, tipo, título, descripción y archivo adjunto.
- **Registro de Actividad**: Bitácora de acciones relevantes (quién hizo qué y cuándo) en el Panel_Admin.
- **Cargo descartado**: Cargo del cabildo sin funcionalidades digitales (Gobernador Suplente, Alcalde Mayor, Alguacil).

---

## Requirements

### Módulo: Modelo de Roles

### Requisito 1: Roles por cargo del cabildo [Alta]

**Historia de Usuario:** Como cabildo, quiero que los cargos con uso digital se representen como roles del sistema, para que cada usuario acceda solo a lo que le corresponde según su cargo.

#### Criterios de Aceptación
1. El sistema DEBE ofrecer los roles: Gobernador (Administrador General), Secretario, Tesorero, Fiscal y Comisario.
2. El sistema NO DEBE ofrecer roles para los cargos descartados (Gobernador Suplente, Alcalde Mayor, Alguacil).
3. CUANDO se crea o edita una cuenta de administrador, el sistema DEBE permitir asignar exactamente uno de los roles disponibles.
4. El sistema DEBE conservar la compatibilidad con las cuentas existentes, asignándoles un rol válido durante la migración.

### Requisito 2: Autorización diferenciada por rol [Alta]

**Historia de Usuario:** Como Gobernador, quiero que cada rol tenga permisos definidos, para proteger las acciones sensibles.

#### Criterios de Aceptación
1. El rol Gobernador DEBE tener acceso total: gestión de cuentas, edición de todo el contenido y eliminación de contenido.
2. El rol Secretario DEBE poder crear y editar comunicados, eventos, documentos, galería y actas; NO DEBE gestionar cuentas ni eliminar contenido de otros módulos.
3. El rol Tesorero DEBE poder gestionar documentos financieros; DEBE tener acceso de solo lectura al resto del contenido.
4. El rol Fiscal DEBE tener acceso de solo lectura a todo el panel y al registro de actividad; NO DEBE crear, editar ni eliminar contenido.
5. El rol Comisario DEBE poder gestionar mensajes de contacto y crear eventos y comunicados; NO DEBE gestionar cuentas ni documentos.
6. CUANDO un usuario intenta una acción no permitida por su rol, el sistema DEBE denegarla y mostrar una pantalla de acceso denegado.

### Módulo: Actas (privado)

### Requisito 3: Registro de actas [Alta]

**Historia de Usuario:** Como Secretario, quiero registrar las actas de asamblea con sus datos y archivo, para archivarlas de forma ordenada.

#### Criterios de Aceptación
1. El Secretario (y el Gobernador) DEBE poder crear un acta con: consecutivo, fecha de la asamblea, tipo (ordinaria/extraordinaria), título, descripción y archivo adjunto.
2. El sistema DEBE validar que el consecutivo, la fecha, el tipo y el título estén presentes.
3. El sistema DEBE aceptar archivos de documento (PDF, DOC, DOCX, ODT, TXT) como adjunto del acta.
4. El sistema DEBE registrar qué usuario creó el acta y la fecha de registro.

### Requisito 4: Consulta y archivo de actas [Media]

**Historia de Usuario:** Como Secretario o Fiscal, quiero consultar las actas registradas, para revisarlas o auditarlas.

#### Criterios de Aceptación
1. El sistema DEBE listar las actas ordenadas por fecha de asamblea (más reciente primero).
2. El sistema DEBE permitir descargar el archivo adjunto de un acta.
3. Las actas NO DEBEN aparecer en el sitio público bajo ninguna circunstancia.
4. El rol Fiscal DEBE poder consultar y descargar actas, pero NO editarlas ni eliminarlas.

### Módulo: Veeduría

### Requisito 5: Registro de actividad [Media]

**Historia de Usuario:** Como Fiscal, quiero ver un registro de las acciones realizadas en el panel, para ejercer la veeduría.

#### Criterios de Aceptación
1. El sistema DEBE registrar las acciones relevantes (crear, editar, eliminar, publicar) con usuario, fecha/hora y descripción.
2. El rol Fiscal y el rol Gobernador DEBEN poder consultar el registro de actividad.
3. El registro de actividad DEBE ser de solo lectura: ningún rol puede editarlo ni borrarlo desde la interfaz.

### Módulo: Tesorería

### Requisito 6: Documentos financieros [Media]

**Historia de Usuario:** Como Tesorero, quiero gestionar los documentos financieros del resguardo, para publicar balances e informes.

#### Criterios de Aceptación
1. El sistema DEBE permitir marcar un documento (o su categoría) como financiero.
2. El rol Tesorero DEBE poder crear, editar y subir documentos financieros.
3. El rol Tesorero NO DEBE poder eliminar documentos de otros módulos ni gestionar cuentas.
