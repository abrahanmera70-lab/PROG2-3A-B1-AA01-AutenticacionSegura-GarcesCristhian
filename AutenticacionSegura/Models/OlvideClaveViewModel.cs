using System.ComponentModel.DataAnnotations;

namespace AutenticacionSegura.Models;

// Formulario "Olvidé mi contraseña": solo pide el correo
public class OlvideClaveViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;
}