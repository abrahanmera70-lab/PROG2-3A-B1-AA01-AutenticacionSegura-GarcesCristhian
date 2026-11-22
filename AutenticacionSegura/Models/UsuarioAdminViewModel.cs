namespace AutenticacionSegura.Models;

// Fila de la tabla de usuarios del panel de administrador
public class UsuarioAdminViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Habilitado { get; set; }
}