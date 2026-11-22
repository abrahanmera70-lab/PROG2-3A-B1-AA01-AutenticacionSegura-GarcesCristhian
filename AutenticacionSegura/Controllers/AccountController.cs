using AutenticacionSegura.Models;
using AutenticacionSegura.Negocio;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Controllers;

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
            TempData["Mensaje"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(modelo);
    }

    // ---------- LOGIN ----------

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
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _authService.IniciarSesionAsync(modelo);

        if (resultado.Succeeded)
        {
            // Solo vuelve a direcciones de este mismo sitio (evita redirecciones a sitios externos)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "AreaPersonal");
        }

        if (resultado.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Cuenta bloqueada temporalmente por varios intentos fallidos. Intente de nuevo en unos minutos.");
        }
        else
        {
            // Mensaje genérico: no revela si el correo existe o si falló la contraseña
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
        }

        return View(modelo);
    }

    // ---------- LOGOUT ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.CerrarSesionAsync();
        return RedirectToAction("Index", "Home");
    }

    // ---------- ACCESO DENEGADO ----------

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
            // Construye el enlace con el correo y el token
            var enlace = Url.Action("ResetPassword", "Account",
                new { correo = modelo.Correo, token = token }, Request.Scheme);

            _correo.EnviarEnlaceRecuperacion(modelo.Correo, enlace!);
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
        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        var modelo = new RestablecerViewModel { Correo = correo, Token = token };
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