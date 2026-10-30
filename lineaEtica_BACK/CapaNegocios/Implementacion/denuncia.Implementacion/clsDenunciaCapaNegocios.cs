using CapaDatos.Interfaz.denuncia.interfaz;
using CapaDTO.Denuncia;
using CapaNegocios.Interfaz.denuncia.interfaz;
using CapaNegocios.Interfaz.Email.Interfaz;
using Microsoft.Extensions.Logging;

namespace CapaNegocios.Implementacion.denuncia.Implementacion;

public class clsDenunciaCapaNegocios : IDenunciaCapaNegocios
{
    private readonly IDenunciaCapaDatos _capaDatos;
    private readonly IEmailNotificacionesCapaNegocios _emailNotif;
    private readonly ILogger<clsDenunciaCapaNegocios> _logger;

    public clsDenunciaCapaNegocios(
        IDenunciaCapaDatos capaDatos,
        IEmailNotificacionesCapaNegocios emailNotif,
        ILogger<clsDenunciaCapaNegocios> logger)
    {
        _capaDatos = capaDatos;
        _emailNotif = emailNotif;
        _logger = logger;
    }

    public Task<FormularioDto?> GetFormulario(int idFormulario)
        => _capaDatos.GetFormulario(idFormulario);

    public Task<List<ParametroDto>> GetParametros(int idTipoParametro)
        => _capaDatos.GetParametros(idTipoParametro);

    public async Task<DenunciaResponseDto> CrearDenuncia(DenunciaRequestDto request, string usuario)
    {
        var resultado = await _capaDatos.CrearDenuncia(request, usuario);
        if (resultado.Exito && resultado.CorreosNotificar.Count > 0)
            _emailNotif.NotificarNuevaDenuncia(resultado.CorreosNotificar, resultado.Consecutivo, resultado.EsCorreccion);
        return resultado;
    }

    public Task<DenunciaResponseDto> GuardarDenuncia(DenunciaRequestDto request, string usuario)
        => _capaDatos.GuardarDenuncia(request, usuario);

    public Task<BorradorDto?> GetBorrador(int idDenuncia, string usuario)
        => _capaDatos.GetBorrador(idDenuncia, usuario);

    public Task<List<MisDenunciaDto>> GetMisDenuncias(string usuario)
        => _capaDatos.GetMisDenuncias(usuario);

    public Task<EstadisticasDto> GetEstadisticas(int? idEmpresaFiltro = null)
        => _capaDatos.GetEstadisticas(idEmpresaFiltro);

    public Task<List<DenunciaAdminResumenDto>> GetTodasDenuncias(int? idEmpresaFiltro = null)
        => _capaDatos.GetTodasDenuncias(idEmpresaFiltro);

    public Task<DenunciaDetalleAdminDto?> GetDenunciaDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null)
        => _capaDatos.GetDenunciaDetalleAdmin(idDenuncia, idEmpresaFiltro);

    public async Task AprobarDenuncia(int idDenuncia, string adminUsuario, string? comentario, int? idEmpresaFiltro = null)
    {
        var info = await _capaDatos.AprobarDenuncia(idDenuncia, adminUsuario, comentario, idEmpresaFiltro);
        _emailNotif.NotificarDenunciaAprobada(info.CorreoDenunciante, info.Consecutivo);
        if (info.CorreoInvestigador != null)
            _emailNotif.NotificarInvestigadorReactivado(info.CorreoInvestigador, info.Consecutivo);
    }

    public async Task DevolverDenuncia(int idDenuncia, string adminUsuario, string comentario, List<CampoRechazadoDto> campos, int? idEmpresaFiltro = null)
    {
        var info = await _capaDatos.DevolverDenuncia(idDenuncia, adminUsuario, comentario, campos, idEmpresaFiltro);
        _emailNotif.NotificarDenunciaDevuelta(info.CorreoDenunciante, info.Consecutivo);
    }

    public async Task RechazarDenuncia(int idDenuncia, string adminUsuario, string comentario, int? idEmpresaFiltro = null)
    {
        var info = await _capaDatos.RechazarDenuncia(idDenuncia, adminUsuario, comentario, idEmpresaFiltro);
        _emailNotif.NotificarDenunciaRechazada(info.CorreoDenunciante, info.Consecutivo);
    }

    public Task<AdjuntoDto> RegistrarAdjunto(int idDenuncia, int idCampo, string nombreOriginal, string nombreArchivo, string tipoMime, long tamanoBytes)
        => _capaDatos.RegistrarAdjunto(idDenuncia, idCampo, nombreOriginal, nombreArchivo, tipoMime, tamanoBytes);

    public Task<List<AdjuntoDto>> GetAdjuntos(int idDenuncia, int? idEmpresaFiltro = null)
        => _capaDatos.GetAdjuntos(idDenuncia, idEmpresaFiltro);

    public Task<AdjuntoDetalleInterno?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null)
        => _capaDatos.GetAdjuntoDetalle(idAdjunto, idEmpresaFiltro);

    public Task EliminarAdjunto(int idAdjunto)
        => _capaDatos.EliminarAdjunto(idAdjunto);
}
