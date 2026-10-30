using System.Text.Json.Serialization;
using CapaDTO.Denuncia;

namespace CapaDTO.Registrador;

public class BuscarUsuarioResponseDto
{
    public bool Existe { get; set; }
    public string? Nombre { get; set; }
    public string? Correo { get; set; }
}

public class MontarDenunciaRequestDto
{
    public string CorreoDestino { get; set; } = string.Empty;
    public string? NombreDestino { get; set; }
    public string CanalOrigen { get; set; } = string.Empty;
    public int IdFormulario { get; set; } = 2;
    public List<RespuestaCampoDto> Respuestas { get; set; } = [];
    public List<RespuestaTablaDto> RespuestasTabla { get; set; } = [];
}

public class MontarDenunciaResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int? IdDenuncia { get; set; }
    public int? Consecutivo { get; set; }
    public bool UsuarioCreado { get; set; }

    [JsonIgnore]
    public string? PasswordGeneradaInterna { get; set; }

    [JsonIgnore]
    public List<string> CorreosConsultores { get; set; } = [];
}

public class DenunciaRegistradorResumenDto
{
    public int Id { get; set; }
    public int? Consecutivo { get; set; }
    public string NombreFormulario { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int IdEstado { get; set; }
    public bool EsAnonimo { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? OrigenCanal { get; set; }
}
