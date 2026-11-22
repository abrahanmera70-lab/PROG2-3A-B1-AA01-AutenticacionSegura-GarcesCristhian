using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutenticacionSegura.Controllers;

// [Authorize] en la clase: TODAS sus acciones exigen haber iniciado sesión
[Authorize]
public class AreaPersonalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}