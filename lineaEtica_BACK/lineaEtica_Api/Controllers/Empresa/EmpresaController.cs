using CapaNegocios.Interfaz.empresa.interfaz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Empresa;

[ApiController]
[Route("api/empresa")]
[Authorize]
public class EmpresaController : ControllerBase
{
    private readonly IEmpresaCapaNegocios _negocios;

    public EmpresaController(IEmpresaCapaNegocios negocios)
    {
        _negocios = negocios;
    }

    [HttpGet("listar")]
    public async Task<IActionResult> Listar()
    {
        var lista = await _negocios.ListarEmpresas();
        return Ok(lista);
    }
}
