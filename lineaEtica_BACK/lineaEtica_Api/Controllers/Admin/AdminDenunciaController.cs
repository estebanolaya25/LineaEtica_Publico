using CapaDTO.Denuncia;
using CapaDTO.Investigador;
using CapaNegocios.Interfaz.denuncia.interfaz;
using CapaNegocios.Interfaz.investigador.interfaz;
using lineaEtica_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lineaEtica_Api.Controllers.Admin;

[ApiController]
[Route("api/admin/denuncia")]
[Authorize]
public class AdminDenunciaController : ControllerBase
{
    private readonly IDenunciaCapaNegocios _negocios;
    private readonly IInvestigadorCapaNegocios _invNegocios;
    private readonly IBlobService _blob;

    public AdminDenunciaController(IDenunciaCapaNegocios negocios, IInvestigadorCapaNegocios invNegocios, IBlobService blob)
    {
        _negocios    = negocios;
        _invNegocios = invNegocios;
        _blob        = blob;
    }

    private string AdminUsuario =>
        User.FindFirst("name")?.Value ?? "ADMIN";

    private IActionResult? ResolverFiltroEmpresa(out int? idEmpresaFiltro, int? idEmpresaSeleccionada = null)
    {
        var rol = User.FindFirst("role")?.Value;
        if (rol == "Jefe del proceso")
        {
            idEmpresaFiltro = idEmpresaSeleccionada;
            return null;
        }

        var claim = User.FindFirst("idEmpresa")?.Value;
        if (int.TryParse(claim, out var idEmpresa))
        {
            idEmpresaFiltro = idEmpresa;
            return null;
        }

        idEmpresaFiltro = null;
        return StatusCode(403, new { mensaje = "Su usuario no tiene empresa asignada. Contacte al administrador." });
    }

    [HttpGet("estadisticas")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetEstadisticas([FromQuery] int? idEmpresa = null)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro, idEmpresa);
        if (errorEmpresa != null) return errorEmpresa;

        var stats = await _negocios.GetEstadisticas(idEmpresaFiltro);
        return Ok(stats);
    }

    [HttpGet("todas")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetTodasDenuncias()
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var lista = await _negocios.GetTodasDenuncias(idEmpresaFiltro);
        return Ok(lista);
    }

    [HttpGet("{idDenuncia:int}")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetDetalle(int idDenuncia)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var detalle = await _negocios.GetDenunciaDetalleAdmin(idDenuncia, idEmpresaFiltro);
        if (detalle == null) return NotFound("Denuncia no encontrada.");
        return Ok(detalle);
    }

    [HttpPost("{idDenuncia:int}/aprobar")]
    [Authorize(Roles = "Consultor")]
    public async Task<IActionResult> Aprobar(int idDenuncia, [FromBody] DecisionAdminRequestDto dto)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        try
        {
            await _negocios.AprobarDenuncia(idDenuncia, AdminUsuario, dto.Comentario, idEmpresaFiltro);
            return Ok(new { mensaje = "Denuncia aprobada exitosamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpPost("{idDenuncia:int}/devolver")]
    [Authorize(Roles = "Consultor")]
    public async Task<IActionResult> Devolver(int idDenuncia, [FromBody] DecisionAdminRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return BadRequest("Debe ingresar un comentario al devolver la denuncia.");

        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        try
        {
            await _negocios.DevolverDenuncia(idDenuncia, AdminUsuario, dto.Comentario, dto.CamposRechazados, idEmpresaFiltro);
            return Ok(new { mensaje = "Denuncia devuelta al reportante para corrección." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpPost("{idDenuncia:int}/rechazar")]
    [Authorize(Roles = "Consultor")]
    public async Task<IActionResult> Rechazar(int idDenuncia, [FromBody] DecisionAdminRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Comentario))
            return BadRequest("Debe ingresar un comentario al rechazar la denuncia.");

        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        try
        {
            await _negocios.RechazarDenuncia(idDenuncia, AdminUsuario, dto.Comentario, idEmpresaFiltro);
            return Ok(new { mensaje = "Denuncia rechazada definitivamente." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpGet("/api/admin/investigadores")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetInvestigadores()
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var lista = await _invNegocios.GetInvestigadores(idEmpresaFiltro);
        return Ok(lista);
    }

    [HttpPost("{idDenuncia:int}/asignar-investigador")]
    [Authorize(Roles = "Consultor")]
    public async Task<IActionResult> AsignarInvestigador(int idDenuncia, [FromBody] AsignarInvestigadorDto dto)
    {
        if (dto.IdInvestigador <= 0)
            return BadRequest("Debe seleccionar un investigador válido.");

        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        try
        {
            await _invNegocios.AsignarInvestigador(idDenuncia, dto.IdInvestigador, AdminUsuario, dto.Comentario, idEmpresaFiltro);
            return Ok(new { mensaje = "Investigador asignado exitosamente. La denuncia está en investigación." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpGet("/api/admin/investigaciones")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetInvestigaciones()
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var lista = await _invNegocios.GetInvestigacionesAdmin(idEmpresaFiltro);
        return Ok(lista);
    }

    [HttpGet("/api/admin/investigacion/{idDenuncia:int}")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetDetalleInvestigacion(int idDenuncia)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var detalle = await _invNegocios.GetDetalleAdmin(idDenuncia, idEmpresaFiltro);
        if (detalle == null) return NotFound("Investigación no encontrada.");
        return Ok(detalle);
    }

    [HttpGet("/api/admin/investigacion-adjunto/{idAdjunto:int}")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> DescargarAdjuntoInvestigacion(int idAdjunto)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var adjunto = await _invNegocios.GetAdjuntoDetalle(idAdjunto, idEmpresaFiltro);
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

    [HttpPost("/api/admin/investigacion/{idDenuncia:int}/responder-investigador")]
    [Authorize(Roles = "Consultor")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> ResponderInvestigador(
        int idDenuncia,
        [FromForm] string comentario,
        [FromForm] IFormFileCollection? archivos)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            return BadRequest("Debe ingresar un comentario de respuesta.");

        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

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

        try
        {
            var seg = await _invNegocios.ResponderInvestigador(idDenuncia, AdminUsuario, comentario.Trim(), infoArchivos, idEmpresaFiltro);
            return Ok(seg);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpPost("/api/admin/investigacion/{idDenuncia:int}/comentario")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> AgregarComentario(
        int idDenuncia,
        [FromForm] string comentario,
        [FromForm] IFormFileCollection? archivos)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            return BadRequest("Debe ingresar un comentario.");

        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

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

        try
        {
            var seg = await _invNegocios.AgregarComentarioAdmin(idDenuncia, AdminUsuario, comentario.Trim(), infoArchivos, idEmpresaFiltro);
            return Ok(seg);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { mensaje = ex.Message });
        }
    }

    [HttpGet("{idDenuncia:int}/adjuntos")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> GetAdjuntos(int idDenuncia)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var lista = await _negocios.GetAdjuntos(idDenuncia, idEmpresaFiltro);
        return Ok(lista);
    }

    [HttpGet("adjunto/{idAdjunto:int}")]
    [Authorize(Roles = "Consultor,Jefe del proceso")]
    public async Task<IActionResult> DescargarAdjunto(int idAdjunto)
    {
        var errorEmpresa = ResolverFiltroEmpresa(out var idEmpresaFiltro);
        if (errorEmpresa != null) return errorEmpresa;

        var adjunto = await _negocios.GetAdjuntoDetalle(idAdjunto, idEmpresaFiltro);
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
}
