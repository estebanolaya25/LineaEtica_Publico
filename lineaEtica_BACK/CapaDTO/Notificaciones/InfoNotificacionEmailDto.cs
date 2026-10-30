namespace CapaDTO.Notificaciones;

public class InfoNotificacionEmailDto
{
    public int? Consecutivo { get; set; }
    public List<string> CorreosConsultores { get; set; } = new();
    public string? CorreoDenunciante { get; set; }
    public string? CorreoInvestigador { get; set; }
    public string? NombreInvestigadorAnterior { get; set; }
}
