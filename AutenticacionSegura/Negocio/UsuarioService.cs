using System.Security.Claims;
using AutenticacionSegura.Datos;
using AutenticacionSegura.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutenticacionSegura.Negocio;

// Servicio de usuarios: perfil propio y administración de usuarios
public class UsuarioService
{
    private const string RolAdmin = "Administrador";
    private const string RolCliente = "Cliente";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UsuarioService(UserManager<ApplicationUser> userManager,
                          SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ---------- PERFIL PROPIO ----------

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
            return Error("Usuario no encontrado.");
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

    // ---------- ADMINISTRACIÓN DE USUARIOS ----------

    // Lista todos los usuarios con su rol y estado
    public async Task<List<UsuarioAdminViewModel>> ListarUsuariosAsync()
    {
        var lista = new List<UsuarioAdminViewModel>();
        var usuarios = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            lista.Add(new UsuarioAdminViewModel
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Correo = usuario.Email ?? string.Empty,
                Rol = string.Join(", ", roles),
                Habilitado = EstaHabilitado(usuario)
            });
        }

        return lista;
    }

    // Cambia el rol de un usuario
    public async Task<IdentityResult> CambiarRolAsync(string id, string nuevoRol)
    {
        // Solo se aceptan los dos roles del sistema
        if (nuevoRol != RolAdmin && nuevoRol != RolCliente)
        {
            return Error("Rol no válido.");
        }

        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario == null)
        {
            return Error("Usuario no encontrado.");
        }

        var esAdministrador = await _userManager.IsInRoleAsync(usuario, RolAdmin);

        // Regla: no se puede quitar el rol a la última cuenta administradora activa
        if (esAdministrador && nuevoRol == RolCliente && EstaHabilitado(usuario)
            && await ContarAdministradoresActivosAsync() <= 1)
        {
            return Error("No se puede quitar el rol a la última cuenta administradora activa.");
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
        var resultado = await _userManager.AddToRoleAsync(usuario, nuevoRol);

        // Renueva el sello de seguridad: el cambio se aplica de inmediato
        await _userManager.UpdateSecurityStampAsync(usuario);

        return resultado;
    }

    // Habilita o deshabilita una cuenta
    public async Task<IdentityResult> CambiarEstadoAsync(string id, bool habilitar)
    {
        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario == null)
        {
            return Error("Usuario no encontrado.");
        }

        if (!habilitar)
        {
            var esAdministrador = await _userManager.IsInRoleAsync(usuario, RolAdmin);

            // Regla: no se puede deshabilitar la última cuenta administradora activa
            if (esAdministrador && EstaHabilitado(usuario)
                && await ContarAdministradoresActivosAsync() <= 1)
            {
                return Error("No se puede deshabilitar la última cuenta administradora activa.");
            }

            // Deshabilitar = bloqueo hasta una fecha lejana
            await _userManager.SetLockoutEnabledAsync(usuario, true);
            await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);

            // Cierra la sesión que el usuario tenga abierta
            await _userManager.UpdateSecurityStampAsync(usuario);

            return IdentityResult.Success;
        }

        // Habilitar = quitar el bloqueo
        await _userManager.SetLockoutEndDateAsync(usuario, null);
        await _userManager.ResetAccessFailedCountAsync(usuario);

        return IdentityResult.Success;
    }

    // ---------- MÉTODOS DE APOYO ----------

    // Una cuenta está deshabilitada si su bloqueo termina en una fecha muy lejana.
    // El bloqueo temporal por intentos fallidos (5 minutos) no cuenta como deshabilitada.
    private static bool EstaHabilitado(ApplicationUser usuario)
    {
        return !(usuario.LockoutEnd.HasValue
                 && usuario.LockoutEnd.Value > DateTimeOffset.UtcNow.AddYears(50));
    }

    private async Task<int> ContarAdministradoresActivosAsync()
    {
        var administradores = await _userManager.GetUsersInRoleAsync(RolAdmin);
        return administradores.Count(a => EstaHabilitado(a));
    }

    private static IdentityResult Error(string mensaje)
    {
        return IdentityResult.Failed(new IdentityError { Description = mensaje });
    }
}