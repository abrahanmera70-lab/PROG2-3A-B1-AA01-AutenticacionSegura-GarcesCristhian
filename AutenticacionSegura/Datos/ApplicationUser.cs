using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Datos;

// Usuario del sistema. Hereda de IdentityUser, que ya trae
// correo, contraseña (con hash), bloqueo y más.
public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
}