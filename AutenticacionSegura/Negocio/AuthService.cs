using AutenticacionSegura.Datos;
using AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio;

// Reglas de negocio de autenticación: registro, login, logout y recuperación
public class AuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthService(UserManager<ApplicationUser> userManager,
                       SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Registra al usuario y le asigna SIEMPRE el rol Cliente (el usuario no elige rol)
    public async Task<IdentityResult> RegistrarAsync(RegistroViewModel modelo)
    {
        var usuario = new ApplicationUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            NombreCompleto = modelo.NombreCompleto
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Clave);

        if (resultado.Succeeded)
        {
            await _userManager.AddToRoleAsync(usuario, "Cliente");
        }

        return resultado;
    }

    // Inicia sesión (cookie). Cada intento fallido cuenta para el bloqueo.
    public async Task<Microsoft.AspNetCore.Identity.SignInResult> IniciarSesionAsync(LoginViewModel modelo)
    {
        return await _signInManager.PasswordSignInAsync(
            modelo.Correo, modelo.Clave, isPersistent: false, lockoutOnFailure: true);
    }

    // Cierra la sesión y elimina la cookie
    public async Task CerrarSesionAsync()
    {
        await _signInManager.SignOutAsync();
    }

    // Genera el token de recuperación. Devuelve null si el correo no existe.
    public async Task<string?> GenerarTokenRecuperacionAsync(string correo)
    {
        var usuario = await _userManager.FindByEmailAsync(correo);

        if (usuario == null)
        {
            return null;
        }

        return await _userManager.GeneratePasswordResetTokenAsync(usuario);
    }

    // Cambia la contraseña con el token (de un solo uso y con caducidad)
    public async Task<IdentityResult> RestablecerClaveAsync(RestablecerViewModel modelo)
    {
        var usuario = await _userManager.FindByEmailAsync(modelo.Correo);

        if (usuario == null)
        {
            // Mensaje genérico: no revela si el correo existe
            return IdentityResult.Failed(new IdentityError
            {
                Description = "El enlace no es válido, ya fue usado o ha caducado."
            });
        }

        return await _userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.Clave);
    }
}