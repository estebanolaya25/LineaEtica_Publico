using CapaDTO.Denuncia;
using CapaDTO.Notificaciones;

namespace CapaDatos.Interfaz.denuncia.interfaz;

public interface IDenunciaCapaDatos
{
    Task<FormularioDto?> GetFormulario(int idFormulario);
    Task<List<ParametroDto>> GetParametros(int idTipoParametro);
    Task<DenunciaResponseDto> CrearDenuncia(DenunciaRequestDto request, string usuario);
    Task<DenunciaResponseDto> GuardarDenuncia(DenunciaRequestDto request, string usuario);
    Task<BorradorDto?> GetBorrador(int idDenuncia, string usuario);
    Task<List<MisDenunciaDto>> GetMisDenuncias(string usuario);

    Task<EstadisticasDto> GetEstadisticas(int? idEmpresaFiltro = null);
    Task<List<DenunciaAdminResumenDto>> GetTodasDenuncias(int? idEmpresaFiltro = null);
    Task<DenunciaDetalleAdminDto?> GetDenunciaDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> AprobarDenuncia(int idDenuncia, string adminUsuario, string? comentario, int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> DevolverDenuncia(int idDenuncia, string adminUsuario, string comentario, List<CampoRechazadoDto> campos, int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> RechazarDenuncia(int idDenuncia, string adminUsuario, string comentario, int? idEmpresaFiltro = null);

    Task<AdjuntoDto> RegistrarAdjunto(int idDenuncia, int idCampo, string nombreOriginal, string nombreArchivo, string tipoMime, long tamanoBytes);
    Task<List<AdjuntoDto>> GetAdjuntos(int idDenuncia, int? idEmpresaFiltro = null);
    Task<AdjuntoDetalleInterno?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null);
    Task EliminarAdjunto(int idAdjunto);
}
