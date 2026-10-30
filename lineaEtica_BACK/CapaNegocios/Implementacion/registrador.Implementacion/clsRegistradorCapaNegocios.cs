using CapaDatos.Interfaz.registrador.interfaz;
using CapaDto.Common;
using CapaDTO.Registrador;
using CapaNegocios.Implementacion.Email.Implementacion;
using CapaNegocios.Interfaz.Email.Interfaz;
using CapaNegocios.Interfaz.registrador.interfaz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapaNegocios.Implementacion.registrador.Implementacion;

public class clsRegistradorCapaNegocios : IRegistradorCapaNegocios
{
    private readonly IRegistradorCapaDatos _datos;
    private readonly IEMailService _iEMailService;
    private readonly IEmailNotificacionesCapaNegocios _emailNotif;
    private readonly IConfiguration _configuration;
    private readonly ILogger<clsRegistradorCapaNegocios> _logger;

    public clsRegistradorCapaNegocios(
        IRegistradorCapaDatos datos,
        IEMailService iEMailService,
        IEmailNotificacionesCapaNegocios emailNotif,
        IConfiguration configuration,
        ILogger<clsRegistradorCapaNegocios> logger)
    {
        _datos = datos;
        _iEMailService = iEMailService;
        _emailNotif = emailNotif;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<BuscarUsuarioResponseDto> BuscarUsuarioPorCorreo(string correo)
    {
        if (string.IsNullOrWhiteSpace(correo))
            throw new ArgumentException("El correo es obligatorio.");
        return _datos.BuscarUsuarioPorCorreo(correo.Trim());
    }

    public Task<List<DenunciaRegistradorResumenDto>> GetMisDenunciasMontadas(int idRegistrador)
        => _datos.GetMisDenunciasMontadas(idRegistrador);

    public async Task<MontarDenunciaResponseDto> MontarDenuncia(MontarDenunciaRequestDto request, int idRegistrador)
    {
        if (string.IsNullOrWhiteSpace(request.CorreoDestino))
            return new MontarDenunciaResponseDto { Exito = false, Mensaje = "El correo del denunciante es obligatorio." };

        var resultado = await _datos.MontarDenuncia(request, idRegistrador);

        if (resultado.Exito && resultado.UsuarioCreado && !string.IsNullOrEmpty(resultado.PasswordGeneradaInterna))
        {
            string correoDestino = request.CorreoDestino;
            string? nombreDestino = request.NombreDestino;
            string passwordGenerada = resultado.PasswordGeneradaInterna;
            int? consecutivo = resultado.Consecutivo;
            _ = Task.Run(async () =>
            {
                try
                {
                    await EnviarNotificacionDenuncianteNuevo(correoDestino, nombreDestino, passwordGenerada, consecutivo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "No se pudo enviar correo de bienvenida al denunciante {Correo} (SMTP posiblemente no configurado)",
                        correoDestino);
                }
            });
        }

        if (resultado.Exito && resultado.CorreosConsultores.Count > 0)
            _emailNotif.NotificarNuevaDenuncia(resultado.CorreosConsultores, resultado.Consecutivo, esCorreccion: false);

        resultado.PasswordGeneradaInterna = null;
        resultado.CorreosConsultores = [];
        return resultado;
    }

    private async Task EnviarNotificacionDenuncianteNuevo(string correo, string? nombre, string randomPass, int? consecutivo)
    {
        string urlambiente = _configuration["UrlAmbiente:urlambiente"]?.ToString() ?? "";
        string nombreMostrado = string.IsNullOrWhiteSpace(nombre) ? correo : nombre;
        string radicadoTexto = consecutivo.HasValue ? consecutivo.Value.ToString() : "N/A";

        string subject = $"Su denuncia fue registrada - Radicado {radicadoTexto}";
        string cuerpo = EmailTemplateHelper.Layout(
            "Denuncia registrada",
            EmailTemplateHelper.Parrafo($"Hola <b>{nombreMostrado}</b>, su denuncia fue registrada en el canal de ética.")
            + EmailTemplateHelper.CajaDestacada("Radicado", radicadoTexto)
            + EmailTemplateHelper.Parrafo("Se creó una cuenta para que pueda hacer seguimiento a su denuncia con las siguientes credenciales:")
            + EmailTemplateHelper.CajaCredenciales(correo, randomPass)
            + EmailTemplateHelper.Parrafo("Se le pedirá cambiarla al ingresar por primera vez."),
            urlambiente, urlambiente, "Ir al aplicativo");

        await _iEMailService.SendMail(new EmailMessageRequestDto
        {
            To = new List<string> { correo },
            Subject = subject,
            Body = cuerpo
        });
    }
}
