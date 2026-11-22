using AutenticacionSegura.Datos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutenticacionSegura.Negocio;

var builder = WebApplication.CreateBuilder(args);

// 1. BASE DE DATOS: conecta la aplicación con SQL Server
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(cadenaConexion));

// 2. IDENTITY: usuarios, roles, hash de contraseñas y tokens
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opciones =>
{
    // Política de contraseña
    opciones.Password.RequiredLength = 8;
    opciones.Password.RequireDigit = true;
    opciones.Password.RequireLowercase = true;
    opciones.Password.RequireUppercase = true;
    opciones.Password.RequireNonAlphanumeric = true;

    // El correo no se puede repetir
    opciones.User.RequireUniqueEmail = true;

    // Bloqueo temporal: 3 intentos fallidos = 5 minutos bloqueado
    opciones.Lockout.AllowedForNewUsers = true;
    opciones.Lockout.MaxFailedAccessAttempts = 3;
    opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. TOKENS de recuperación de contraseña: caducan en 30 minutos
builder.Services.Configure<DataProtectionTokenProviderOptions>(opciones =>
    opciones.TokenLifespan = TimeSpan.FromMinutes(30));

// 4. COOKIE de sesión
builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Account/Login";
    opciones.AccessDeniedPath = "/Account/AccesoDenegado";
    opciones.Cookie.HttpOnly = true;
    opciones.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opciones.SlidingExpiration = true;

    // En desarrollo acepta HTTP; en producción exige HTTPS (Secure)
    opciones.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CorreoSimulado>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Crea roles y administrador inicial al arrancar
using (var alcance = app.Services.CreateScope())
{
    await DbInicializador.InicializarAsync(alcance.ServiceProvider, app.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// El orden importa: primero identifica quién eres, luego qué puedes hacer
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
