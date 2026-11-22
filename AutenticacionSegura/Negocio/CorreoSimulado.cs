namespace AutenticacionSegura.Negocio;

// Simula el envío de correo: solo en Desarrollo escribe el enlace en la consola.
// En producción NO muestra nada (no se exponen tokens).
public class CorreoSimulado
{
    private readonly ILogger<CorreoSimulado> _logger;
    private readonly IWebHostEnvironment _entorno;

    public CorreoSimulado(ILogger<CorreoSimulado> logger, IWebHostEnvironment entorno)
    {
        _logger = logger;
        _entorno = entorno;
    }

    public void EnviarRecuperacion(string correo, string enlace)
    {
        if (_entorno.IsDevelopment())
        {
            _logger.LogWarning(
                "CORREO SIMULADO para {Correo}. Enlace de recuperacion: {Enlace}",
                correo, enlace);
        }
    }
}