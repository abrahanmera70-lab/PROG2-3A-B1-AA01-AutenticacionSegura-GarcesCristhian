using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Models;

// Formulario de cambio de contraseña
public class CambiarClaveViewModel
{
    [Required(ErrorMessage = "Escribe tu contraseña actual")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string ClaveActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe la nueva contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string ClaveNueva { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la nueva contraseña")]
    [DataType(DataType.Password)]
    [Compare("ClaveNueva", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmarClaveNueva { get; set; } = string.Empty;
}