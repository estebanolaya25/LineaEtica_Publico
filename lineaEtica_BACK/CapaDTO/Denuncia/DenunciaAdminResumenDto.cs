namespace CapaDTO.Denuncia;

public class DenunciaAdminResumenDto
{
    public int Id { get; set; }
    public int? Consecutivo { get; set; }
    public string NombreFormulario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public bool EsAnonimo { get; set; }
    public string? UsuarioReportante { get; set; }
    public int? IdEmpresa { get; set; }
    public string? NombreEmpresa { get; set; }
}
