# Línea Ética

Aplicación web de **canal ético corporativo** para recibir, gestionar e investigar denuncias (fraude, conflictos de interés, conductas indebidas, etc.) en un grupo de empresas, con flujo de revisión, asignación a investigadores y seguimiento hasta el cierre.

## Funcionalidades

- **Registro y autenticación**: registro público de denunciantes, login con captcha, recuperación y cambio de contraseña, JWT con refresh tokens.
- **Denuncias con formularios dinámicos**: los formularios, secciones, campos, tipos de campo y reglas condicionales (mostrar u ocultar campos según respuestas) se definen en base de datos.
- **Borradores y adjuntos**: la denuncia puede guardarse como borrador y admite archivos adjuntos.
- **Revisión**: el Consultor aprueba, rechaza o devuelve la denuncia marcando los campos que el denunciante debe corregir.
- **Investigación**: asignación y reasignación de investigadores, seguimientos con adjuntos, solicitud de información adicional y cierre del caso.
- **Canales externos**: el Registrador monta denuncias recibidas por correo o WhatsApp a nombre de un denunciante.
- **Multi-empresa**: cada usuario interno pertenece a una empresa del grupo; los dashboards permiten filtrar por empresa.
- **Dashboards e indicadores** por rol, **notificaciones** internas y por correo, exportación de la denuncia a **PDF** y **manual de usuario** integrado.
- Interfaz bilingüe (español / inglés).

### Roles

| Rol | Descripción |
|---|---|
| Denunciante | Crea denuncias y consulta su estado; elige la empresa en cada denuncia |
| Consultor | Gestiona las denuncias de su empresa: aprueba, rechaza, devuelve y asigna investigador |
| Investigador | Investiga las denuncias asignadas y registra el seguimiento |
| Jefe del proceso | Consulta el flujo de todas las empresas para indicadores globales (solo lectura) |
| Registrador | Registra denuncias recibidas por canales externos |

## Tecnologías

**Backend**
- .NET 9 / ASP.NET Core Web API
- ADO.NET con `Microsoft.Data.SqlClient`
- SQL Server
- Autenticación JWT con refresh tokens y autorización por roles
- Swagger (Swashbuckle)
- Azure Blob Storage para adjuntos (Azurite en desarrollo)
- Envío de correo vía SMTP

**Frontend**
- Angular 20 (componentes standalone, lazy loading)
- TypeScript 5.8
- Bootstrap 5
- ngx-translate para internacionalización
- jsPDF para la exportación de denuncias

## Arquitectura

El backend sigue una arquitectura en capas:

```
lineaEtica_BACK/
├── lineaEtica_Api/   Controladores, configuración, JWT y servicio de Blob Storage
├── CapaNegocios/     Reglas de negocio y servicios de correo
├── CapaDatos/        Acceso a datos con SQL Server
└── CapaDTO/          Objetos de petición y respuesta
```

Cada capa expone interfaces (`Interfaz/`) y sus implementaciones (`Implementacion/`), registradas por inyección de dependencias en `Program.cs`.

```
lineaEtica_FRONT/src/app/
├── auth/    Login, registro, recuperación de contraseña, guards e interceptor HTTP
├── core/    Servicios transversales (configuración, notificaciones, PDF, tema)
└── pages/   Layout, dashboards, denuncias, bandejas de Consultor, Investigador y Registrador, administración
```

## Puesta en marcha

### Requisitos

- .NET SDK 9
- Node.js 20+ y npm
- SQL Server (LocalDB, Express o superior)
- [Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) o una cuenta de Azure Storage para los adjuntos

### 1. Base de datos

Crear la base de datos `LineaEtica` y ejecutar, en este orden:

1. `lineaeticaschema.sql` (esquema base)
2. `migration_gnl_refresh_token.sql`
3. `migration_form_condicion.sql`
4. `lineaEtica_BACK/migration_admin.sql`
5. `migration_notificaciones.sql`
6. `migration_investigador.sql`
7. `migration_adjunto_seguimiento.sql`
8. `migration_registrador.sql`
9. `migration_empresa.sql`
10. `migration_roles.sql`

Después de `migration_empresa.sql`, registrar las empresas del grupo en `gnl_empresa`:

```sql
INSERT INTO gnl_empresa (nombre) VALUES ('Empresa 1'), ('Empresa 2');
```

> Los datos de configuración de los formularios dinámicos (`form_formulario`, `form_seccion`, `form_campo`, parámetros y catálogos) no se incluyen en el repositorio. Para que el flujo de denuncias funcione deben cargarse en esas tablas.

### 2. Backend

```bash
cd lineaEtica_BACK/lineaEtica_Api
cp appsettings.Example.json appsettings.json
```

Completar en `appsettings.json`:

| Clave | Descripción |
|---|---|
| `ConnectionStrings:SQLConnectionString` | Cadena de conexión a SQL Server |
| `Jwt:Key` | Clave secreta para firmar los tokens (mínimo 64 caracteres) |
| `Encriptacion:Key` / `Encriptacion:IV` | Clave e IV de 8 caracteres para el cifrado de contraseñas |
| `EmailConfig:*` | Servidor SMTP para el envío de notificaciones |
| `UrlAmbiente:urlambiente` | URL del frontend, usada en los enlaces de los correos |

En desarrollo, `appsettings.Development.json` ya apunta el almacenamiento de adjuntos a Azurite (`UseDevelopmentStorage=true`). En otros entornos se debe definir `AzureStorage:ConnectionString` y `AzureStorage:ContainerName`.

`appsettings.json` está excluido del repositorio para no versionar credenciales.

```bash
dotnet run --launch-profile https
```

La API queda disponible en `https://localhost:7195` y la documentación Swagger en `https://localhost:7195/swagger`.

### 3. Frontend

```bash
cd lineaEtica_FRONT
npm install
npm start
```

La aplicación queda disponible en `http://localhost:4200`. La URL de la API se configura en `public/assets/config.json`.

### Usuarios

Los denunciantes se registran desde la pantalla de registro. Los usuarios internos (Consultor, Investigador, Jefe del proceso, Registrador) se crean desde **Administración de usuarios**; el primero debe insertarse directamente en `gnl_usuario`, con la contraseña cifrada mediante `cEncriptacion.CifradoData` usando la clave configurada.
