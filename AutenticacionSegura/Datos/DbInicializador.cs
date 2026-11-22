using Microsoft.AspNetCore.Identity;

namespace AutenticacionSegura.Datos;

// Crea los datos iniciales: los roles y el administrador.
public static class DbInicializador
{
    public static async Task InicializarAsync(IServiceProvider servicios, IConfiguration config)
    {
        var roleManager = servicios.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = servicios.GetRequiredService<UserManager<ApplicationUser>>();

        // 1. Crear los roles si todavía no existen
        string[] roles = { "Administrador", "Cliente" };
        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
            }
        }

        // 2. Leer los datos del administrador desde User Secrets
        var correo = config["AdminSeed:Email"];
        var clave = config["AdminSeed:Password"];

        // Si no están configurados, no se crea el administrador
        if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(clave))
        {
            return;
        }

        // 3. Crear el administrador solo si no existe
        if (await userManager.FindByEmailAsync(correo) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = correo,
                Email = correo,
                NombreCompleto = "Administrador Inicial",
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(admin, clave);

            if (resultado.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrador");
            }
        }
    }
}
