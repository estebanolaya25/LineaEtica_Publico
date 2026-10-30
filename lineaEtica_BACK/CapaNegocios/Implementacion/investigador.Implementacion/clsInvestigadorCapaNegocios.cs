using CapaDatos.Interfaz.investigador.interfaz;
using CapaDTO.Investigador;
using CapaNegocios.Interfaz.investigador.interfaz;
using CapaNegocios.Interfaz.Email.Interfaz;
using Microsoft.Extensions.Logging;

namespace CapaNegocios.Implementacion.investigador.Implementacion;

public class clsInvestigadorCapaNegocios : IInvestigadorCapaNegocios
{
    private readonly IInvestigadorCapaDatos _datos;
    private readonly IEmailNotificacionesCapaNegocios _emailNotif;
    private readonly ILogger<clsInvestigadorCapaNegocios> _logger;

    public clsInvestigadorCapaNegocios(
        IInvestigadorCapaDatos datos,
        IEmailNotificacionesCapaNegocios emailNotif,
        ILogger<clsInvestigadorCapaNegocios> logger)
    {
        _datos  = datos;
        _emailNotif = emailNotif;
        _logger = logger;
    }

    public Task<List<InvestigadorResumenDto>> GetInvestigadores(int? idEmpresaFiltro = null)
        => _datos.GetInvestigadores(idEmpresaFiltro);

    public async Task AsignarInvestigador(int idDenuncia, int idInvestigador, string adminUsuario, string? comentario, int? idEmpresaFiltro = null)
    {
        var info = await _datos.AsignarInvestigador(idDenuncia, idInvestigador, adminUsuario, comentario, idEmpresaFiltro);
        _emailNotif.NotificarInvestigadorAsignado(info.CorreoInvestigador, info.Consecutivo);
    }

    public Task<List<DenunciaInvestigadorResumenDto>> GetMisDenuncias(string usuario)
        => _datos.GetMisDenuncias(usuario);

    public Task<DenunciaInvestigadorDetalleDto?> GetDetalle(int idDenuncia, string usuario)
        => _datos.GetDetalle(idDenuncia, usuario);

    public Task<List<SeguimientoDto>> GetSeguimientos(int idDenuncia)
        => _datos.GetSeguimientos(idDenuncia);

    public Task<SeguimientoDto> CrearSeguimiento(
        int idDenuncia, string comentario, string usuario,
        List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos)
        => _datos.CrearSeguimiento(idDenuncia, comentario, usuario, archivos);

    public Task<AdjuntoInvestigadorDto?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null)
        => _datos.GetAdjuntoDetalle(idAdjunto, idEmpresaFiltro);

    public async Task CerrarInvestigacion(int idDenuncia, string usuario, string comentario)
    {
        var info = await _datos.CerrarInvestigacion(idDenuncia, usuario, comentario);
        _emailNotif.NotificarDenunciaCerrada(info.CorreoDenunciante, info.Consecutivo);
    }

    public Task<List<InvestigacionAdminResumenDto>> GetInvestigacionesAdmin(int? idEmpresaFiltro = null)
        => _datos.GetInvestigacionesAdmin(idEmpresaFiltro);

    public Task<DenunciaInvestigadorDetalleDto?> GetDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null)
        => _datos.GetDetalleAdmin(idDenuncia, idEmpresaFiltro);

    public async Task ReasignarInvestigador(int idDenuncia, int idNuevoInvestigador, string usuarioActual, string comentario)
    {
        var info = await _datos.ReasignarInvestigador(idDenuncia, idNuevoInvestigador, usuarioActual, comentario);
        _emailNotif.NotificarInvestigadorReasignado(info.CorreoInvestigador, info.Consecutivo, info.NombreInvestigadorAnterior);
    }

    public async Task<SeguimientoDto> SolicitarInformacionAnalista(int idDenuncia, string usuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos)
    {
        var (seguimiento, info) = await _datos.SolicitarInformacionAnalista(idDenuncia, usuario, comentario, archivos);
        _emailNotif.NotificarSolicitudInformacion(info.CorreosConsultores, info.Consecutivo);
        return seguimiento;
    }

    public async Task<SeguimientoDto> ResponderInvestigador(int idDenuncia, string adminUsuario, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null)
    {
        var (seguimiento, info) = await _datos.ResponderInvestigador(idDenuncia, adminUsuario, comentario, archivos, idEmpresaFiltro);
        _emailNotif.NotificarRespuestaInvestigador(info.CorreoInvestigador, info.Consecutivo);
        return seguimiento;
    }

    public Task<SeguimientoDto> AgregarComentarioAdmin(int idDenuncia, string usuarioAdmin, string comentario, List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos, int? idEmpresaFiltro = null)
        => _datos.AgregarComentarioAdmin(idDenuncia, usuarioAdmin, comentario, archivos, idEmpresaFiltro);
}
