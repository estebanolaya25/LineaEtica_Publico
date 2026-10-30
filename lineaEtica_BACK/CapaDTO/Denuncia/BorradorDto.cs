namespace CapaDTO.Denuncia;

public class BorradorDto
{
    public int Id { get; set; }
    public int IdFormulario { get; set; }
    public int IdEstado { get; set; }
    public List<RespuestaCampoDto> Respuestas { get; set; } = [];
    public List<RespuestaTablaDto> RespuestasTabla { get; set; } = [];
    public List<CampoRechazadoDto> CamposRechazados { get; set; } = [];
    public string? ComentarioAdmin { get; set; }
}
