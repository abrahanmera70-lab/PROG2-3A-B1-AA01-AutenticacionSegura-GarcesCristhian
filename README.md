# Módulo integral de autenticación y seguridad (AA01)

Proyecto ASP.NET Core MVC con SQL Server, Entity Framework Core e ASP.NET Core Identity.
Asignatura: Programación II. Autor: Cristhian Garces Mera. Paralelo 3A.

## Arquitectura (3 capas)
- **Presentación:** `Controllers/`, `Views/`, `Models/` (ViewModels). Los controladores no tienen reglas de negocio ni consultas.
- **Negocio:** `Negocio/` (`AuthService`, `UsuarioService`, `CorreoSimulado`).
- **Datos:** `Datos/` (`ApplicationDbContext`, `ApplicationUser`, `DbInicializador`) y `Migrations/`.

## Requisitos
- Visual Studio 2026 Community (carga de trabajo ASP.NET y desarrollo web)
- .NET 10
- SQL Server LocalDB

## Cómo ejecutar
1. Clonar el repositorio y abrir la solución en Visual Studio.
2. Configurar el administrador inicial con User Secrets (no se guarda en el repositorio):
   ```
   dotnet user-secrets set "AdminSeed:Email" "admin@taller.com"
   dotnet user-secrets set "AdminSeed:Password" "<contraseña de prueba>"
   ```
   La contraseña debe cumplir la política (8+ caracteres, mayúscula, minúscula, número y símbolo).
3. Crear la base de datos: en la Consola del Administrador de paquetes ejecutar `Update-Database`
   (o ejecutar `Script_BD.sql` en SQL Server).
4. Ejecutar con F5. Al iniciar, la app crea los roles `Administrador` y `Cliente` y el administrador inicial.

## Configuración
- `appsettings.json` solo contiene la cadena de conexión a LocalDB con autenticación de Windows (sin usuario ni clave).
- Las credenciales del administrador inicial se guardan en User Secrets.

## Usuarios de prueba (datos ficticios)
| Usuario | Rol | Cómo se crea |
|---|---|---|
| admin@taller.com | Administrador | Seeding con User Secrets |
| cliente1@taller.com | Cliente | Registro desde la web |
| cliente2@taller.com | Cliente | Registro desde la web |

## Funcionalidades
Registro (rol Cliente automático, correo único), login/logout con cookie, bloqueo tras 3 intentos fallidos (5 min),
recuperación de contraseña con token de un solo uso y caducidad de 30 min, perfil y cambio de contraseña (exige la actual),
panel de administrador (listar usuarios, cambiar rol, habilitar/deshabilitar), protección de la última cuenta administradora activa,
acceso denegado para rutas no autorizadas.

## Seguridad aplicada
- Contraseñas con `PasswordHasher` de Identity (hash con sal, nunca texto plano).
- `[Authorize]` y `[Authorize(Roles = "Administrador")]` validados en el servidor.
- Antiforgery en todos los formularios POST.
- Mensajes genéricos en login y recuperación (no revelan si el correo existe).
- Redirección solo a URLs locales después del login.
- Cookie `HttpOnly`, expiración de 30 min.

## Diferencias Desarrollo vs Producción
| Aspecto | Desarrollo | Producción |
|---|---|---|
| Correo de recuperación | Simulado: el enlace se escribe en la consola | No se muestra ningún token; se requiere un servicio real de correo |
| Cookie de sesión | `Secure` según la petición (permite http/https local) | `Secure` siempre (solo HTTPS) |
| Errores | Página de errores detallada | Página de error genérica y HSTS |
| Secretos | User Secrets | Variables de entorno o gestor de secretos |