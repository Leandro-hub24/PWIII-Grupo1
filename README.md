[README.md](https://github.com/user-attachments/files/35053505/README.md)
# RuteoYa

**RuteoYa** es una aplicación web desarrollada como trabajo práctico de **Programación Web III**. Su objetivo es conectar personas que necesitan enviar paquetes con conductores que publican viajes, aprovechando recorridos y capacidad disponible para facilitar las entregas.

El sistema permite gestionar viajes y envíos, consultar oportunidades de transporte y registrar asignaciones. También incorpora cálculo de rutas, seguimiento de estados y funcionalidades de consulta y análisis de la actividad.

> **Estado:** proyecto académico. La configuración y algunos servicios externos están orientados a un entorno local de desarrollo.

## Funcionalidades principales

- **Usuarios y autenticación:** registro e inicio de sesión. La aplicación Web utiliza autenticación por cookies y la API utiliza JWT.
- **Viajes:** creación, edición, listado y detalle de viajes; selección de días, horarios, capacidad, paradas y desvío máximo; activación y desactivación.
- **Envíos:** publicación y consulta de paquetes, carga de origen y destino, peso, tamaño, fecha límite y seguimiento del estado del envío.
- **Oportunidades:** búsqueda de combinaciones posibles entre envíos y viajes, con validaciones de disponibilidad, capacidad y desvío antes de aceptar una oportunidad.
- **Asignaciones:** vinculación de un envío con un viaje y registro de los cambios de estado asociados.
- **Optimización:** funcionalidades para evaluar y confirmar combinaciones de envíos y recorridos.
- **Dashboard e historial:** visualización de información de actividad y consulta de datos históricos.

## Tecnologías

| Tecnología | Uso en el proyecto |
| --- | --- |
| **C# / .NET 10** | Lenguaje y plataforma de desarrollo |
| **ASP.NET Core MVC** | Aplicación Web, controladores y vistas Razor |
| **ASP.NET Core Web API** | Endpoints HTTP para las funcionalidades del sistema |
| **Entity Framework Core** | Acceso a datos y mapeo de entidades |
| **SQL Server / LocalDB** | Persistencia de datos |
| **JWT y cookies** | Autenticación de API y Web, respectivamente |
| **HttpClient** | Comunicación de la Web con la API |
| **HERE API** | Servicios externos de geocodificación y cálculo de distancias utilizados por la lógica |
| **Swagger / OpenAPI** | Exploración de endpoints durante el desarrollo |
| **xUnit, Moq** | Pruebas automatizadas y dobles de prueba |

## Estructura de la solución

```text
PWIII-Grupo1/
├── RuteoYa.API/          # API REST, endpoints, autenticación JWT y configuración
├── RuteoYa.Web/          # Interfaz MVC, vistas Razor, modelos y servicios HTTP
├── RuteoYa.Logica/       # Reglas de negocio, servicios y repositorios
├── RuteoYa.Entidades/    # Entidades, DTOs, interfaces y DbContext
├── RuteoYa.Tests/        # Pruebas automatizadas
└── RuteoYa.slnx         # Solución de Visual Studio
```

### Arquitectura

La solución separa la interfaz, la comunicación HTTP, las reglas de negocio y el acceso a datos:

```text
Navegador
   |
   v
RuteoYa.Web (vistas Razor + controladores MVC)
   |
   v
Servicios HTTP de Web (HttpClient)
   |
   v
RuteoYa.API (controladores REST)
   |
   v
RuteoYa.Logica (servicios / reglas de negocio)
   |
   v
Repositorios + Entity Framework Core
   |
   v
SQL Server
```

Algunas operaciones también consultan servicios externos de geocodificación y distancias. La arquitectura real puede variar ligeramente según la funcionalidad.

## Requisitos

- **.NET SDK 10**.
- **Visual Studio** compatible con .NET 10 o un editor con soporte para C#.
- **SQL Server LocalDB** o una instancia de SQL Server accesible desde el equipo.
- Acceso a Internet y una **clave válida de HERE API** para las funcionalidades que necesitan geocodificación o cálculo de distancias.

## Instalación y ejecución local

### 1. Clonar el repositorio

```bash
git clone https://github.com/Leandro-hub24/PWIII-Grupo1.git
cd PWIII-Grupo1
```

Abrir `RuteoYa.slnx` en Visual Studio o trabajar desde la terminal con `dotnet`.

### 2. Configurar SQL Server

La API utiliza la cadena `ConnectionStrings:DefaultConnection`. La configuración actual del repositorio apunta a una instancia **SQL Server LocalDB** y a una base de datos llamada `RuteoYa`.

Ejemplo de estructura de configuración **sin credenciales**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RuteoYa;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Importante:** `RuteoYaDbContext` también define una conexión en `OnConfiguring()`. Antes de utilizar otra instancia de SQL Server, revisar ambas configuraciones para evitar que una sobrescriba a la otra. La base de datos y su esquema deben existir o crearse aplicando las migraciones disponibles para el proyecto.

### 3. Configurar las claves y la conexión entre aplicaciones

La API utiliza parámetros de **JWT** y de **HERE API**. Definir los valores reales mediante *User Secrets* de .NET o variables de entorno, sin publicarlos en Git:

```bash
dotnet user-secrets init --project RuteoYa.API
dotnet user-secrets set "Jwt:Key" "<CLAVE_PRIVADA_SEGURA>" --project RuteoYa.API
dotnet user-secrets set "HereApi:ApiKey" "<CLAVE_HERE>" --project RuteoYa.API
```

También deben existir valores apropiados para `Jwt:Issuer` y `Jwt:Audience`.

La Web utiliza la configuración `ApiBaseUrl` para comunicarse con la API. Para los perfiles HTTPS incluidos en el repositorio, se puede definir localmente con:

```bash
dotnet user-secrets init --project RuteoYa.Web
dotnet user-secrets set "ApiBaseUrl" "https://localhost:7019" --project RuteoYa.Web
```

> **Seguridad:** revisar y rotar las claves que ya se hayan publicado en archivos versionados. La validación de certificados deshabilitada en algunos clientes HTTP está orientada al desarrollo y no debe mantenerse en producción.

### 4. Restaurar dependencias

```bash
dotnet restore RuteoYa.slnx
```

### 5. Ejecutar API y Web

**Es necesario ejecutar ambas aplicaciones al mismo tiempo**, por ejemplo, en dos terminales:

```bash
# Terminal 1: API
dotnet run --project RuteoYa.API --launch-profile https
```

```bash
# Terminal 2: Web
dotnet run --project RuteoYa.Web --launch-profile https
```

Con los perfiles de lanzamiento actuales:

| Aplicación | HTTPS | HTTP |
| --- | --- | --- |
| API | `https://localhost:7019` | `http://localhost:5224` |
| Web | `https://localhost:7236` | `http://localhost:5271` |

Abrir la Web desde `https://localhost:7236`. Durante el desarrollo, la documentación interactiva de la API está configurada mediante Swagger, normalmente en `https://localhost:7019/swagger`.

Los puertos pueden variar si se modifican los perfiles de lanzamiento o la configuración local.

## Ejemplo: recorrido de creación de un viaje

1. El usuario completa el formulario Razor de viajes.
2. `ViajesController` de Web recibe y valida `ViajeFormViewModel`.
3. El ViewModel se convierte en `CrearViajeDto`.
4. `ViajesApiService` envía `POST /api/viajes` mediante `HttpClient`.
5. `ViajesController` de API valida la solicitud e identifica al usuario autenticado.
6. `ViajesService` aplica reglas de negocio y solicita el cálculo del recorrido.
7. El servicio construye las entidades `Viaje`, `Parada` y `ViajeDia`.
8. `ViajesRepositorio` utiliza `RuteoYaDbContext` y `SaveChangesAsync()` para persistir los cambios.
9. La API devuelve el viaje creado y Web redirige a su detalle.

Este recorrido ilustra el patrón general utilizado en varias funcionalidades del sistema, aunque cada una tiene sus particularidades.

## Entidades principales

- `Usuario`: datos de las personas registradas.
- `Viaje`: recorrido, horarios, capacidad y disponibilidad.
- `ViajeDia`: días asociados a un viaje.
- `Parada`: puntos intermedios de un recorrido.
- `Envio`: paquete publicado y su estado.
- `Asignacion`: relación entre un envío y un viaje, con información del transporte.
- `EnvioEstadoHistorial`: registro de transiciones de estado de un envío.

Las relaciones y restricciones se administran mediante las entidades y `RuteoYaDbContext` de Entity Framework Core.

## Pruebas

El proyecto `RuteoYa.Tests` utiliza **xUnit** y **Moq**. Para ejecutar las pruebas desde la raíz del repositorio:

```bash
dotnet test RuteoYa.Tests/RuteoYa.Tests.csproj
```

La ejecución correcta depende de que estén instalados los requisitos del entorno y de que las pruebas no necesiten servicios locales adicionales.

## Consideraciones de desarrollo

- Mantener las credenciales y claves fuera del control de versiones.
- Revisar las conexiones definidas tanto en `appsettings.json` como en `RuteoYaDbContext.OnConfiguring()`.
- Verificar que la URL base de la API en Web coincida con el perfil con el que se está ejecutando.
- Ejecutar **API y Web** simultáneamente para probar la aplicación completa.
- Utilizar el entorno de desarrollo para Swagger y las configuraciones locales; antes de desplegar en producción, revisar certificados, claves, manejo de errores y permisos.

## Repositorio

[Leandro-hub24/PWIII-Grupo1](https://github.com/Leandro-hub24/PWIII-Grupo1)

**Proyecto académico — Programación Web III, Grupo 1.**
