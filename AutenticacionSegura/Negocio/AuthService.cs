using AutenticacionSegura.Datos;
using AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio;

// Servicio de autenticación: contiene las reglas de registro, login, logout y recuperación
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

    public async Task<IdentityResult> RegistrarAsync(RegistroViewModel modelo)
    {
        var usuario = new ApplicationUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            NombreCompleto = modelo.NombreCompleto
        };

        // Identity valida la política de contraseña y el correo único,
        // y guarda la contraseña con hash
        var resultado = await _userManager.CreateAsync(usuario, modelo.Clave);

        if (resultado.Succeeded)
        {
            // Todo usuario nuevo es Cliente. El registro público nunca crea Administradores
            await _userManager.AddToRoleAsync(usuario, "Cliente");
        }

        return resultado;
    }

    public async Task<SignInResult> IniciarSesionAsync(LoginViewModel modelo)
    {
        // lockoutOnFailure: true cuenta los intentos fallidos y bloquea la cuenta
        return await _signInManager.PasswordSignInAsync(
            modelo.Correo, modelo.Clave, isPersistent: false, lockoutOnFailure: true);
    }

    public async Task CerrarSesionAsync()
    {
        // Elimina la cookie de sesión
        await _signInManager.SignOutAsync();
    }

    // Genera el token de recuperación. Devuelve null si el correo no existe
    public async Task<string?> GenerarTokenRecuperacionAsync(string correo)
    {
        var usuario = await _userManager.FindByEmailAsync(correo);

        if (usuario == null)
        {
            return null;
        }

        return await _userManager.GeneratePasswordResetTokenAsync(usuario);
    }

    // Cambia la contraseña si el token es válido, no ha caducado y no se ha usado
    public async Task<IdentityResult> RestablecerClaveAsync(RestablecerViewModel modelo)
    {
        var usuario = await _userManager.FindByEmailAsync(modelo.Correo);

        if (usuario == null)
        {
            return IdentityResult.Failed(
                new IdentityError { Description = "El enlace no es válido o ha caducado." });
        }

        return await _userManager.ResetPasswordAsync(usuario, modelo.Token, modelo.Clave);
    }
}