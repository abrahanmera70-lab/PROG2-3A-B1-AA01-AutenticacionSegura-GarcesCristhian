using System.Security.Claims;
using AutenticacionSegura.Datos;
using AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Negocio;

// Servicio de usuarios: perfil propio y cambio de contraseña
public class UsuarioService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UsuarioService(UserManager<ApplicationUser> userManager,
                          SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Devuelve los datos del usuario que tiene la sesión iniciada (solo los propios)
    public async Task<PerfilViewModel?> ObtenerPerfilAsync(ClaimsPrincipal usuarioActual)
    {
        var usuario = await _userManager.GetUserAsync(usuarioActual);

        if (usuario == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(usuario);

        return new PerfilViewModel
        {
            NombreCompleto = usuario.NombreCompleto,
            Correo = usuario.Email ?? string.Empty,
            Rol = string.Join(", ", roles)
        };
    }

    // Cambia la contraseña SOLO si la contraseña actual es correcta
    public async Task<IdentityResult> CambiarClaveAsync(ClaimsPrincipal usuarioActual, CambiarClaveViewModel modelo)
    {
        var usuario = await _userManager.GetUserAsync(usuarioActual);

        if (usuario == null)
        {
            return IdentityResult.Failed(
                new IdentityError { Description = "Usuario no encontrado." });
        }

        var resultado = await _userManager.ChangePasswordAsync(
            usuario, modelo.ClaveActual, modelo.ClaveNueva);

        if (resultado.Succeeded)
        {
            // Mantiene la sesión abierta con la información actualizada
            await _signInManager.RefreshSignInAsync(usuario);
        }

        return resultado;
    }
}