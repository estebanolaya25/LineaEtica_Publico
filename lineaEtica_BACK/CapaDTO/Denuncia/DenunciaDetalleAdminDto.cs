namespace CapaDTO.Denuncia;

public class DenunciaDetalleAdminDto
{
    public int Id { get; set; }
    public int IdFormulario { get; set; }
    public int IdEstado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EsAnonimo { get; set; }
    public string? UsuarioReportante { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public int? Consecutivo { get; set; }
    public List<RespuestaCampoDto> Respuestas { get; set; } = [];
    public List<RespuestaTablaDto> RespuestasTabla { get; set; } = [];
    public List<CampoRechazadoDto> CamposRechazados { get; set; } = [];
    public string? ComentarioAdmin { get; set; }
    public int? IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}
