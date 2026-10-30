using CapaNegocios.Interfaz.notificaciones.interfaz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Notificaciones;

[ApiController]
[Route("api/notificaciones")]
[Authorize]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionesCapaNegocios _negocios;

    public NotificacionesController(INotificacionesCapaNegocios negocios)
    {
        _negocios = negocios;
    }

    private int IdUsuarioActual =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> ObtenerNoLeidas()
    {
        if (IdUsuarioActual == 0) return Unauthorized();
        var lista = await _negocios.ObtenerNoLeidas(IdUsuarioActual);
        return Ok(lista);
    }

    [HttpPut("marcar-leidas")]
    public async Task<IActionResult> MarcarLeidas()
    {
        if (IdUsuarioActual == 0) return Unauthorized();
        await _negocios.MarcarLeidas(IdUsuarioActual);
        return Ok();
    }

    [HttpPut("{id:int}/marcar-leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        if (IdUsuarioActual == 0) return Unauthorized();
        await _negocios.MarcarLeida(id, IdUsuarioActual);
        return Ok();
    }
}
