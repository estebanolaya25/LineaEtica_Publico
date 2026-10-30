namespace CapaDTO.Notificaciones;

public class NotificacionDto
{
    public int      Id                  { get; set; }
    public string   Mensaje             { get; set; } = string.Empty;
    public string   Tipo                { get; set; } = "info";
    public bool     Leida               { get; set; }
    public DateTime Fecha               { get; set; }
    public int?     IdFormularioUsuario { get; set; }
}
