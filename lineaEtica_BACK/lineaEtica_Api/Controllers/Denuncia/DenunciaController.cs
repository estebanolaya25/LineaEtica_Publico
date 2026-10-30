using CapaDTO.Denuncia;
using CapaNegocios.Interfaz.denuncia.interfaz;
using lineaEtica_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Denuncia;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DenunciaController : ControllerBase
{
    private readonly IDenunciaCapaNegocios _negocios;
    private readonly IBlobService _blob;

    public DenunciaController(IDenunciaCapaNegocios negocios, IBlobService blob)
    {
        _negocios = negocios;
        _blob     = blob;
    }

    private string UsuarioActual =>
        User.FindFirst("name")?.Value
        ?? User.FindFirst("unique_name")?.Value
        ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
        ?? "DESCONOCIDO";

    [HttpGet("formulario/{idFormulario:int}")]
    public async Task<IActionResult> GetFormulario(int idFormulario)
    {
        if (idFormulario != 1 && idFormulario != 2)
            return BadRequest("Formulario no válido. Use 1 (Anónimo) o 2 (No Anónimo).");

        var formulario = await _negocios.GetFormulario(idFormulario);
        if (formulario == null) return NotFound("Formulario no encontrado.");
        return Ok(formulario);
    }

    [HttpGet("parametros/{idTipo:int}")]
    public async Task<IActionResult> GetParametros(int idTipo)
    {
        var parametros = await _negocios.GetParametros(idTipo);
        return Ok(parametros);
    }

    [HttpGet("mis-denuncias")]
    public async Task<IActionResult> GetMisDenuncias()
    {
        var resultado = await _negocios.GetMisDenuncias(UsuarioActual);
        return Ok(resultado);
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> GuardarDenuncia([FromBody] DenunciaRequestDto request)
    {
        if (request.IdEmpresa <= 0) return BadRequest("Debe indicar la empresa.");
        var resultado = await _negocios.GuardarDenuncia(request, UsuarioActual);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("borrador/{idDenuncia:int}")]
    public async Task<IActionResult> GetBorrador(int idDenuncia)
    {
        var borrador = await _negocios.GetBorrador(idDenuncia, UsuarioActual);
        if (borrador == null) return NotFound("Borrador no encontrado.");
        return Ok(borrador);
    }

    [HttpPost("crear")]
    public async Task<IActionResult> CrearDenuncia([FromBody] DenunciaRequestDto request)
    {
        if (request.IdEmpresa <= 0) return BadRequest("Debe indicar la empresa.");
        var resultado = await _negocios.CrearDenuncia(request, UsuarioActual);
        if (!resultado.Exito) return BadRequest(resultado);
        return Ok(resultado);
    }

    [HttpPost("{idDenuncia:int}/adjunto/{idCampo:int}")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> SubirAdjunto(int idDenuncia, int idCampo, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
            return BadRequest("No se recibió ningún archivo.");

        var borrador = await _negocios.GetBorrador(idDenuncia, UsuarioActual);
        if (borrador == null)
            return NotFound("Denuncia no encontrada o no le pertenece.");

        var ext           = Path.GetExtension(archivo.FileName);
        var nombreBlob    = $"{Guid.NewGuid()}{ext}";

        await using var stream = archivo.OpenReadStream();
        await _blob.SubirAsync(stream, nombreBlob, archivo.ContentType ?? "application/octet-stream");

        var dto = await _negocios.RegistrarAdjunto(
            idDenuncia, idCampo,
            archivo.FileName, nombreBlob,
            archivo.ContentType ?? "application/octet-stream",
            archivo.Length);

        return Ok(dto);
    }

    [HttpGet("{idDenuncia:int}/adjuntos")]
    public async Task<IActionResult> GetAdjuntos(int idDenuncia)
    {
        var borrador = await _negocios.GetBorrador(idDenuncia, UsuarioActual);
        if (borrador == null) return NotFound();

        var lista = await _negocios.GetAdjuntos(idDenuncia);
        return Ok(lista);
    }

    [HttpGet("adjunto/{idAdjunto:int}")]
    public async Task<IActionResult> DescargarAdjunto(int idAdjunto)
    {
        var adjunto = await _negocios.GetAdjuntoDetalle(idAdjunto);
        if (adjunto == null) return NotFound("Adjunto no encontrado.");

        bool esAdmin = User.IsInRole("Consultor");
        if (!esAdmin && adjunto.UsuarioPropietario != UsuarioActual)
            return Forbid();

        try
        {
            var (stream, contentType) = await _blob.DescargarAsync(adjunto.NombreArchivo);
            return File(stream, contentType,
                System.Net.WebUtility.UrlEncode(adjunto.NombreOriginal));
        }
        catch
        {
            return NotFound("El archivo ya no está disponible.");
        }
    }

    [HttpDelete("adjunto/{idAdjunto:int}")]
    public async Task<IActionResult> EliminarAdjunto(int idAdjunto)
    {
        var adjunto = await _negocios.GetAdjuntoDetalle(idAdjunto);
        if (adjunto == null) return NotFound();
        if (adjunto.UsuarioPropietario != UsuarioActual) return Forbid();

        await _blob.EliminarAsync(adjunto.NombreArchivo);
        await _negocios.EliminarAdjunto(idAdjunto);
        return Ok(new { mensaje = "Adjunto eliminado." });
    }
}
