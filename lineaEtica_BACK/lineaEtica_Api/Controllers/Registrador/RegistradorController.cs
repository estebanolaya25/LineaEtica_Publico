using CapaDTO.Registrador;
using CapaNegocios.Interfaz.registrador.interfaz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Registrador;

[ApiController]
[Route("api/registrador")]
[Authorize(Roles = "Registrador")]
public class RegistradorController : ControllerBase
{
    private readonly IRegistradorCapaNegocios _negocios;

    public RegistradorController(IRegistradorCapaNegocios negocios)
    {
        _negocios = negocios;
    }

    private int IdRegistradorActual =>
        int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value, out var id) ? id : 0;

    [HttpGet("buscar-usuario")]
    public async Task<IActionResult> BuscarUsuario([FromQuery] string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return BadRequest("Debe indicar un correo.");

        var resultado = await _negocios.BuscarUsuarioPorCorreo(correo);
        return Ok(resultado);
    }

    [HttpPost("montar-denuncia")]
    public async Task<IActionResult> MontarDenuncia([FromBody] MontarDenunciaRequestDto request)
    {
        var resultado = await _negocios.MontarDenuncia(request, IdRegistradorActual);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("mis-denuncias")]
    public async Task<IActionResult> GetMisDenuncias()
    {
        var lista = await _negocios.GetMisDenunciasMontadas(IdRegistradorActual);
        return Ok(lista);
    }
}
