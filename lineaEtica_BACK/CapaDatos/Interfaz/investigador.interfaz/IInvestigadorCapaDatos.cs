using CapaDTO.Investigador;
using CapaDTO.Notificaciones;

namespace CapaDatos.Interfaz.investigador.interfaz;

public interface IInvestigadorCapaDatos
{
    Task<List<DenunciaInvestigadorResumenDto>> GetMisDenuncias(string usuario);
    Task<DenunciaInvestigadorDetalleDto?> GetDetalle(int idDenuncia, string usuario);
    Task<List<SeguimientoDto>> GetSeguimientos(int idDenuncia);
    Task<SeguimientoDto> CrearSeguimiento(int idDenuncia, string comentario, string usuario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos);
    Task<AdjuntoInvestigadorDto?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> CerrarInvestigacion(int idDenuncia, string usuario, string comentario);

    Task<List<InvestigadorResumenDto>> GetInvestigadores(int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> AsignarInvestigador(int idDenuncia, int idInvestigador, string adminUsuario, string? comentario, int? idEmpresaFiltro = null);
    Task<List<InvestigacionAdminResumenDto>> GetInvestigacionesAdmin(int? idEmpresaFiltro = null);
    Task<DenunciaInvestigadorDetalleDto?> GetDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null);
    Task<InfoNotificacionEmailDto> ReasignarInvestigador(int idDenuncia, int idNuevoInvestigador, string usuarioActual, string comentario);
    Task<(SeguimientoDto Seguimiento, InfoNotificacionEmailDto Info)> SolicitarInformacionAnalista(int idDenuncia, string usuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos);
    Task<(SeguimientoDto Seguimiento, InfoNotificacionEmailDto Info)> ResponderInvestigador(int idDenuncia, string adminUsuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null);
    Task<SeguimientoDto> AgregarComentarioAdmin(int idDenuncia, string usuarioAdmin, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null);
}
