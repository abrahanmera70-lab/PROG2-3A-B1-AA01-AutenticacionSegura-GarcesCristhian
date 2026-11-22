namespace AutenticacionSegura.Models;

// Datos del perfil que se muestran en pantalla
public class PerfilViewModel
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}