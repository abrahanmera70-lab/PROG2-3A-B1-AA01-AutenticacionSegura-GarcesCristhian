using AutenticacionSegura.Models;
using AutenticacionSegura.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Controllers;

// Capa de presentación: solo recibe peticiones y llama a la capa de negocio
public class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly CorreoSimulado _correo;

    public AccountController(AuthService authService, CorreoSimulado correo)
    {
        _authService = authService;
        _correo = correo;
    }

    // ---------- REGISTRO ----------

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistroViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _authService.RegistrarAsync(modelo);

        if (resultado.Succeeded)
        {
            TempData["Mensaje"] = "Registro exitoso. Ya puede iniciar sesión.";
            return RedirectToAction("Login");
        }

        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(modelo);
    }

    // ---------- LOGIN / LOGOUT ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _authService.IniciarSesionAsync(modelo);

        if (resultado.Succeeded)
        {
            // Solo redirige a direcciones del propio sitio (evita open redirect)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "AreaPersonal");
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "La cuenta está bloqueada o deshabilitada. Intente más tarde o contacte al administrador.");
        }
        else
        {
            // Mensaje genérico: no dice si falló el correo o la contraseña
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        }

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.CerrarSesionAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    // ---------- RECUPERACIÓN DE CONTRASEÑA ----------

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(OlvideClaveViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var token = await _authService.GenerarTokenRecuperacionAsync(modelo.Correo);

        if (token != null)
        {
            var enlace = Url.Action("ResetPassword", "Account",
                new { correo = modelo.Correo, token }, Request.Scheme);

            _correo.EnviarRecuperacion(modelo.Correo, enlace ?? string.Empty);
        }

        // Siempre la misma respuesta, exista o no el correo
        return RedirectToAction("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? correo, string? token)
    {
        var modelo = new RestablecerViewModel
        {
            Correo = correo ?? string.Empty,
            Token = token ?? string.Empty
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(RestablecerViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _authService.RestablecerClaveAsync(modelo);

        if (resultado.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(modelo);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}