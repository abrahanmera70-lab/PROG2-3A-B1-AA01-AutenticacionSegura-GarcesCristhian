using AutenticacionSegura.Datos;
using AutenticacionSegura.Negocio;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Capa de datos: conexión a SQL Server (sin credenciales en el código)
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer(cadenaConexion));

// ASP.NET Core Identity: usuarios, roles, política de contraseña y bloqueo
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opciones =>
{
    // Política de contraseña
    opciones.Password.RequiredLength = 8;
    opciones.Password.RequireDigit = true;
    opciones.Password.RequireLowercase = true;
    opciones.Password.RequireUppercase = true;
    opciones.Password.RequireNonAlphanumeric = true;

    // Correo único
    opciones.User.RequireUniqueEmail = true;

    // Bloqueo por intentos fallidos
    opciones.Lockout.AllowedForNewUsers = true;
    opciones.Lockout.MaxFailedAccessAttempts = 3;
    opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<DescriptorErroresEs>();

// Los tokens de recuperación caducan a los 30 minutos
builder.Services.Configure<DataProtectionTokenProviderOptions>(opciones =>
    opciones.TokenLifespan = TimeSpan.FromMinutes(30));

// Revisa la sesión en cada petición: los cambios de rol o de estado se aplican de inmediato
builder.Services.Configure<SecurityStampValidatorOptions>(opciones =>
    opciones.ValidationInterval = TimeSpan.Zero);

// Cookie de sesión
builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Account/Login";
    opciones.AccessDeniedPath = "/Account/AccesoDenegado";
    opciones.Cookie.HttpOnly = true;
    opciones.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opciones.SlidingExpiration = true;
    opciones.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// Capa de negocio
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CorreoSimulado>();
builder.Services.AddScoped<UsuarioService>();

// Capa de presentación (antiforgery automático en todos los POST)
builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();

// Crea los roles y el administrador inicial (seeding)
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();