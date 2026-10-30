using CapaDTO.Denuncia;

namespace CapaDTO.Investigador;

public class DenunciaInvestigadorResumenDto
{
    public int Id { get; set; }
    public int? Consecutivo { get; set; }
    public string NombreFormulario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public bool EsAnonimo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaAsignacion { get; set; }
}

public class InvestigacionAdminResumenDto
{
    public int Id { get; set; }
    public int? Consecutivo { get; set; }
    public string NombreFormulario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public bool EsAnonimo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public string? NombreInvestigador { get; set; }
    public string? CorreoInvestigador { get; set; }
    public int CantidadSeguimientos { get; set; }
    public int? IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}

public class DenunciaInvestigadorDetalleDto
{
    public int Id { get; set; }
    public int IdFormulario { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsAnonimo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? Consecutivo { get; set; }
    public string? ComentarioAdmin { get; set; }
    public string? NombreInvestigador { get; set; }
    public string? CorreoInvestigador { get; set; }
    public List<RespuestaCampoDto> Respuestas { get; set; } = [];
    public List<RespuestaTablaDto> RespuestasTabla { get; set; } = [];
    public List<SeguimientoDto> Seguimientos { get; set; } = [];
}

public class SeguimientoDto
{
    public int Id { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public List<AdjuntoInvestigadorDto> Adjuntos { get; set; } = [];
}

public class AdjuntoInvestigadorDto
{
    public int Id { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; }
    public string Usuario { get; set; } = string.Empty;
}

public class AsignarInvestigadorDto
{
    public int IdInvestigador { get; set; }
    public string? Comentario { get; set; }
}

public class InvestigadorResumenDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}

public class CerrarInvestigacionDto
{
    public string Comentario { get; set; } = string.Empty;
}

public class ReasignarInvestigadorDto
{
    public int    IdNuevoInvestigador { get; set; }
    public string Comentario         { get; set; } = string.Empty;
}

public class SolicitarInformacionDto
{
    public string Comentario { get; set; } = string.Empty;
}
