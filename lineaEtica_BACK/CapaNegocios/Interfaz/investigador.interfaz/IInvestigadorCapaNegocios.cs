using CapaDTO.Investigador;

namespace CapaNegocios.Interfaz.investigador.interfaz;

public interface IInvestigadorCapaNegocios
{
    Task<List<DenunciaInvestigadorResumenDto>> GetMisDenuncias(string usuario);
    Task<DenunciaInvestigadorDetalleDto?> GetDetalle(int idDenuncia, string usuario);
    Task<List<SeguimientoDto>> GetSeguimientos(int idDenuncia);
    Task<SeguimientoDto> CrearSeguimiento(int idDenuncia, string comentario, string usuario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos);
    Task<AdjuntoInvestigadorDto?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null);
    Task CerrarInvestigacion(int idDenuncia, string usuario, string comentario);

    Task<List<InvestigadorResumenDto>> GetInvestigadores(int? idEmpresaFiltro = null);
    Task AsignarInvestigador(int idDenuncia, int idInvestigador, string adminUsuario, string? comentario, int? idEmpresaFiltro = null);
    Task<List<InvestigacionAdminResumenDto>> GetInvestigacionesAdmin(int? idEmpresaFiltro = null);
    Task<DenunciaInvestigadorDetalleDto?> GetDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null);
    Task ReasignarInvestigador(int idDenuncia, int idNuevoInvestigador, string usuarioActual, string comentario);
    Task<SeguimientoDto> SolicitarInformacionAnalista(int idDenuncia, string usuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos);
    Task<SeguimientoDto> ResponderInvestigador(int idDenuncia, string adminUsuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null);
    Task<SeguimientoDto> AgregarComentarioAdmin(int idDenuncia, string usuarioAdmin, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null);
}
