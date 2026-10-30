using CapaDTO.Investigador;
using CapaNegocios.Interfaz.investigador.interfaz;
using lineaEtica_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Investigador;

[ApiController]
[Route("api/investigador")]
[Authorize(Roles = "Investigador")]
public class InvestigadorController : ControllerBase
{
    private readonly IInvestigadorCapaNegocios _negocios;
    private readonly IBlobService _blob;

    public InvestigadorController(IInvestigadorCapaNegocios negocios, IBlobService blob)
    {
        _negocios = negocios;
        _blob     = blob;
    }

    private string UsuarioActual =>
        User.FindFirst("name")?.Value ?? User.FindFirst("sub")?.Value ?? "INVESTIGADOR";

    [HttpGet("mis-denuncias")]
    public async Task<IActionResult> GetMisDenuncias()
    {
        var lista = await _negocios.GetMisDenuncias(UsuarioActual);
        return Ok(lista);
    }

    [HttpGet("denuncia/{idDenuncia:int}")]
    public async Task<IActionResult> GetDetalle(int idDenuncia)
    {
        var detalle = await _negocios.GetDetalle(idDenuncia, UsuarioActual);
        if (detalle == null) return NotFound("Denuncia no encontrada o no asignada a usted.");
        return Ok(detalle);
    }

    [HttpGet("denuncia/{idDenuncia:int}/seguimientos")]
    public async Task<IActionResult> GetSeguimientos(int idDenuncia)
    {
        var lista = await _negocios.GetSeguimientos(idDenuncia);
        return Ok(lista);
    }

    [HttpPost("denuncia/{idDenuncia:int}/seguimiento")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> CrearSeguimiento(
        int idDenuncia,
        [FromForm] string comentario,
        [FromForm] IFormFileCollection? archivos)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            return BadRequest("El comentario no puede estar vacío.");

        var infoArchivos = new List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)>();

        if (archivos != null)
        {
            foreach (var archivo in archivos)
            {
                if (archivo.Length == 0) continue;
                if (archivo.Length > 20 * 1024 * 1024)
                    return BadRequest($"El archivo '{archivo.FileName}' supera el límite de 20 MB.");

                var ext        = Path.GetExtension(archivo.FileName);
                var nombreBlob = $"inv_{Guid.NewGuid():N}{ext}";

                await using var stream = archivo.OpenReadStream();
                await _blob.SubirAsync(stream, nombreBlob, archivo.ContentType);

                infoArchivos.Add((archivo.FileName, nombreBlob, archivo.ContentType, archivo.Length));
            }
        }

        var seg = await _negocios.CrearSeguimiento(idDenuncia, comentario.Trim(), UsuarioActual, infoArchivos);
        return Ok(seg);
    }

    [HttpGet("investigadores")]
    public async Task<IActionResult> GetInvestigadores()
    {
        var lista = await _negocios.GetInvestigadores();
        return Ok(lista);
    }

    [HttpPost("denuncia/{idDenuncia:int}/reasignar")]
    public async Task<IActionResult> ReasignarInvestigador(int idDenuncia, [FromBody] ReasignarInvestigadorDto dto)
    {
        if (dto.IdNuevoInvestigador <= 0)
            return BadRequest("Debe seleccionar un investigador válido.");
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return BadRequest("Debe ingresar el motivo de la reasignación.");

        try
        {
            await _negocios.ReasignarInvestigador(idDenuncia, dto.IdNuevoInvestigador, UsuarioActual, dto.Comentario.Trim());
            return Ok(new { mensaje = "Investigación reasignada exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpGet("adjunto/{idAdjunto:int}")]
    public async Task<IActionResult> DescargarAdjunto(int idAdjunto)
    {
        var adjunto = await _negocios.GetAdjuntoDetalle(idAdjunto);
        if (adjunto == null) return NotFound("Adjunto no encontrado.");

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

    [HttpPost("denuncia/{idDenuncia:int}/solicitar-informacion")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> SolicitarInformacion(
        int idDenuncia,
        [FromForm] string comentario,
        [FromForm] IFormFileCollection? archivos)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            return BadRequest("Debe indicar qué información necesita.");

        var infoArchivos = new List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)>();

        if (archivos != null)
        {
            foreach (var archivo in archivos)
            {
                if (archivo.Length == 0) continue;
                if (archivo.Length > 20 * 1024 * 1024)
                    return BadRequest($"El archivo '{archivo.FileName}' supera el límite de 20 MB.");

                var ext        = Path.GetExtension(archivo.FileName);
                var nombreBlob = $"inv_{Guid.NewGuid():N}{ext}";

                await using var stream = archivo.OpenReadStream();
                await _blob.SubirAsync(stream, nombreBlob, archivo.ContentType);

                infoArchivos.Add((archivo.FileName, nombreBlob, archivo.ContentType, archivo.Length));
            }
        }

        var seg = await _negocios.SolicitarInformacionAnalista(idDenuncia, UsuarioActual, comentario.Trim(), infoArchivos);
        return Ok(seg);
    }

    [HttpPost("denuncia/{idDenuncia:int}/cerrar")]
    public async Task<IActionResult> CerrarInvestigacion(int idDenuncia, [FromBody] CerrarInvestigacionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return BadRequest("Debe ingresar un comentario de cierre.");

        await _negocios.CerrarInvestigacion(idDenuncia, UsuarioActual, dto.Comentario.Trim());
        return Ok(new { mensaje = "Investigación cerrada exitosamente." });
    }
}
