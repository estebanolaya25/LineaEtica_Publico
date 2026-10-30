using System.Text.Json.Serialization;

namespace CapaDTO.Denuncia;

public class DenunciaRequestDto
{
    public int? IdDenuncia { get; set; }
    public int IdFormulario { get; set; }
    public bool EsAnonimo { get; set; }
    public int IdEmpresa { get; set; }
    public List<RespuestaCampoDto> Respuestas { get; set; } = [];
    public List<RespuestaTablaDto> RespuestasTabla { get; set; } = [];
}

public class RespuestaCampoDto
{
    public int IdCampo { get; set; }
    public int? IdOpcion { get; set; }
    public string? TextoRespuesta { get; set; }
}

public class RespuestaTablaDto
{
    public int IdCampoTabla { get; set; }
    public int IdGrupo { get; set; }
    public int IdCampoHijo { get; set; }
    public string? TextoRespuesta { get; set; }
    public int? IdOpcion { get; set; }
}

public class DenunciaResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? IdDenuncia { get; set; }
    public int? Consecutivo { get; set; }

    [JsonIgnore]
    public List<string> CorreosNotificar { get; set; } = [];
    [JsonIgnore]
    public bool EsCorreccion { get; set; }
}
