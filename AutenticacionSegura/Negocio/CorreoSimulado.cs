namespace AutenticacionSegura.Negocio;

// CORREO SIMULADO (solo para demostración local).
// En lugar de enviar un email real, escribe el enlace en la consola.
// En producción se reemplazaría por un servicio de correo real (SMTP).
public class CorreoSimulado
{
    private readonly ILogger<CorreoSimulado> _logger;
    private readonly IWebHostEnvironment _entorno;

    public CorreoSimulado(ILogger<CorreoSimulado> logger, IWebHostEnvironment entorno)
    {
        _logger = logger;
        _entorno = entorno;
    }

    public void EnviarEnlaceRecuperacion(string correo, string enlace)
    {
        // El token NUNCA se muestra fuera del entorno de desarrollo
        if (_entorno.IsDevelopment())
        {
            _logger.LogWarning("CORREO SIMULADO para {Correo}. Enlace de recuperacion: {Enlace}", correo, enlace);
        }
    }
}