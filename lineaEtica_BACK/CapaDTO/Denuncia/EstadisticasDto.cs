namespace CapaDTO.Denuncia;

public class EstadisticasDto
{
    public int Total           { get; set; }
    public int Pendientes      { get; set; }
    public int Aprobadas       { get; set; }
    public int Rechazadas      { get; set; }
    public int Devueltas       { get; set; }
    public int EnInvestigacion { get; set; }
    public int Cerradas        { get; set; }
    public int Anonimas        { get; set; }
    public int NoAnonimas      { get; set; }

    public List<EstMesDto>    PorMes    { get; set; } = new();
    public List<EstEstadoDto> PorEstado { get; set; } = new();
    public List<EstEmpresaDto> PorEmpresa { get; set; } = new();
    public List<EstCanalDto> PorCanal { get; set; } = new();
}

public class EstMesDto
{
    public string Mes      { get; set; } = string.Empty;
    public int    Cantidad { get; set; }
}

public class EstEstadoDto
{
    public int    IdEstado { get; set; }
    public string Estado   { get; set; } = string.Empty;
    public int    Cantidad { get; set; }
}

public class EstEmpresaDto
{
    public int?   IdEmpresa     { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public int    Cantidad      { get; set; }
}

public class EstCanalDto
{
    public string Canal    { get; set; } = string.Empty;
    public int    Cantidad { get; set; }
}
