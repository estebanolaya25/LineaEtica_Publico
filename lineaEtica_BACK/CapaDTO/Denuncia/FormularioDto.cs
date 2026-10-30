namespace CapaDTO.Denuncia;

public class FormularioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<SeccionDto> Secciones { get; set; } = [];
    public List<CondicionDto> Condiciones { get; set; } = [];
}

public class CondicionDto
{
    public int Id { get; set; }
    public int? IdCampoOrigen { get; set; }
    public string Condicion { get; set; } = string.Empty;
    public string? ValorCondicion { get; set; }
    public int IdSeccionDestino { get; set; }
    public int? IdCampoDestino { get; set; }
    public string Accion { get; set; } = string.Empty;
}

public class SeccionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool MostrarNombre { get; set; }
    public int Orden { get; set; }
    public List<CampoDto> Campos { get; set; } = [];
}

public class CampoDto
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public string? Ayuda { get; set; }
    public bool Obligatorio { get; set; }
    public int Orden { get; set; }
    public int TipoCampo { get; set; }
    public int TipoParametro { get; set; }
    public int TipoParametroHijo { get; set; }
    public int Col { get; set; }
    public int IdCampo { get; set; }
    public int CantAdjuntos { get; set; }
    public List<CampoDto> CamposHijo { get; set; } = [];
}

public class ParametroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public int DependeDe { get; set; }
}
