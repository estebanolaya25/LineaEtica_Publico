namespace CapaDTO.Denuncia;

public class AdjuntoDto
{
    public int Id { get; set; }
    public int IdCampo { get; set; }
    public string NombreOriginal { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; }
}

public class AdjuntoDetalleInterno
{
    public int Id { get; set; }
    public int IdCampo { get; set; }
    public int IdFormularioUsuario { get; set; }
    public string UsuarioPropietario { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
}
