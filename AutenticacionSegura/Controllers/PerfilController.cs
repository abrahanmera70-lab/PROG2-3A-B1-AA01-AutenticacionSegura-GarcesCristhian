using AutenticacionSegura.Models;
using AutenticacionSegura.Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Controllers;

// [Authorize]: todas las acciones exigen sesión iniciada
[Authorize]
public class PerfilController : Controller
{
    private readonly UsuarioService _usuarioService;

    public PerfilController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Siempre usa el usuario de la sesión (User): nadie puede ver el perfil de otro
        var perfil = await _usuarioService.ObtenerPerfilAsync(User);

        if (perfil == null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(perfil);
    }

    [HttpGet]
    public IActionResult CambiarClave()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarClave(CambiarClaveViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _usuarioService.CambiarClaveAsync(User, modelo);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = "Contraseña cambiada correctamente.";
            return RedirectToAction("Index");
        }

        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(modelo);
    }
}