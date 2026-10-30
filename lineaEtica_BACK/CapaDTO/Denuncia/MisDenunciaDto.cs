namespace CapaDTO.Denuncia;

public class MisDenunciaDto
{
    public int Id { get; set; }
    public int? Consecutivo { get; set; }
    public string NombreFormulario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaCreacion { get; set; }
    public bool EsAnonimo { get; set; }
}
