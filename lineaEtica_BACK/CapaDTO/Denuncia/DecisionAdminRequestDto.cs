namespace CapaDTO.Denuncia;

public class DecisionAdminRequestDto
{
    public string? Comentario { get; set; }
    public List<CampoRechazadoDto> CamposRechazados { get; set; } = [];
}
