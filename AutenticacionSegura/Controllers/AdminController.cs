using AutenticacionSegura.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Controllers;

// Solo los usuarios con rol Administrador pueden usar TODAS estas acciones.
// Un Cliente recibe "Acceso denegado" aunque escriba la URL directamente.
[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly UsuarioService _usuarioService;

    public AdminController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usuarios = await _usuarioService.ListarUsuariosAsync();
        return View(usuarios);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id, string rol)
    {
        var resultado = await _usuarioService.CambiarRolAsync(id, rol);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = "Rol actualizado correctamente.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", resultado.Errors.Select(e => e.Description));
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(string id, bool habilitar)
    {
        var resultado = await _usuarioService.CambiarEstadoAsync(id, habilitar);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = habilitar ? "Cuenta habilitada." : "Cuenta deshabilitada.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", resultado.Errors.Select(e => e.Description));
        }

        return RedirectToAction("Index");
    }
}