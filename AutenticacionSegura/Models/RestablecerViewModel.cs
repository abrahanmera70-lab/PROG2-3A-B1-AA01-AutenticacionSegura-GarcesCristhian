using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Models;

// Formulario para escribir la nueva contraseña
public class RestablecerViewModel
{
    [Required]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string Clave { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña")]
    [DataType(DataType.Password)]
    [Compare("Clave", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarClave { get; set; } = string.Empty;
}