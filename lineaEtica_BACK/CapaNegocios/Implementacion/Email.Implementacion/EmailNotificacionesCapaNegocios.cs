using CapaDto.Common;
using CapaNegocios.Interfaz.Email.Interfaz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapaNegocios.Implementacion.Email.Implementacion;

public class EmailNotificacionesCapaNegocios : IEmailNotificacionesCapaNegocios
{
    private readonly IEMailService _iEMailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificacionesCapaNegocios> _logger;

    public EmailNotificacionesCapaNegocios(
        IEMailService iEMailService,
        IConfiguration configuration,
        ILogger<EmailNotificacionesCapaNegocios> logger)
    {
        _iEMailService = iEMailService;
        _configuration = configuration;
        _logger = logger;
    }

    private string UrlAmbiente => _configuration["UrlAmbiente:urlambiente"]?.ToString() ?? "";

    private static string RadicadoTexto(int? consecutivo) => consecutivo.HasValue ? consecutivo.Value.ToString() : "N/A";

    private string Plantilla(string titulo, string mensaje, int? consecutivo) =>
        EmailTemplateHelper.Layout(
            titulo,
            EmailTemplateHelper.Parrafo(mensaje) + EmailTemplateHelper.CajaDestacada("Radicado", RadicadoTexto(consecutivo)),
            UrlAmbiente,
            UrlAmbiente,
            "Ir al aplicativo");

    private void EnviarSeguro(List<string> destinatarios, string subject, string body)
    {
        var validos = destinatarios?.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().ToList() ?? new List<string>();
        if (validos.Count == 0) return;

        _ = Task.Run(async () =>
        {
            try
            {
                var resultado = await _iEMailService.SendMail(new EmailMessageRequestDto
                {
                    To = validos,
                    Subject = subject,
                    Body = body
                });

                if (resultado != null && resultado.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                    _logger.LogWarning("Fallo al enviar correo '{Subject}' a {Destinatarios}: {Resultado}",
                        subject, string.Join(", ", validos), resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al enviar correo '{Subject}' a {Destinatarios}",
                    subject, string.Join(", ", validos));
            }
        });
    }

    private void EnviarSeguro(string? destinatario, string subject, string body)
        => EnviarSeguro(destinatario == null ? new List<string>() : new List<string> { destinatario }, subject, body);

    public void NotificarNuevaDenuncia(List<string> correosConsultores, int? consecutivo, bool esCorreccion)
    {
        string radicado = RadicadoTexto(consecutivo);
        string subject = esCorreccion
            ? $"Denuncia corregida pendiente de revisión - Radicado {radicado}"
            : $"Nueva denuncia para revisar - Radicado {radicado}";
        string mensaje = esCorreccion
            ? "Una denuncia devuelta fue corregida por el denunciante y está pendiente de revisión."
            : "Se recibió una nueva denuncia para revisar.";
        EnviarSeguro(correosConsultores, subject, Plantilla("Nueva denuncia para revisar", mensaje, consecutivo));
    }

    public void NotificarDenunciaAprobada(string? correoDenunciante, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoDenunciante, $"Su denuncia fue aprobada - Radicado {radicado}",
            Plantilla("Denuncia aprobada", "Su respuesta fue aceptada y la investigación de su denuncia ha sido retomada.", consecutivo));
    }

    public void NotificarDenunciaDevuelta(string? correoDenunciante, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoDenunciante, $"Su denuncia requiere corrección - Radicado {radicado}",
            Plantilla("Denuncia devuelta para corrección", "Su denuncia fue revisada y tiene campos que requieren corrección. Ingrese al aplicativo para corregirla.", consecutivo));
    }

    public void NotificarDenunciaRechazada(string? correoDenunciante, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoDenunciante, $"Su denuncia fue rechazada - Radicado {radicado}",
            Plantilla("Denuncia rechazada", "Su denuncia ha sido rechazada.", consecutivo));
    }

    public void NotificarDenunciaCerrada(string? correoDenunciante, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoDenunciante, $"Su denuncia fue cerrada - Radicado {radicado}",
            Plantilla("Investigación cerrada", "La investigación de su denuncia ha sido cerrada.", consecutivo));
    }

    public void NotificarInvestigadorAsignado(string? correoInvestigador, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoInvestigador, $"Nueva denuncia asignada - Radicado {radicado}",
            Plantilla("Denuncia asignada para investigación", "Se le ha asignado una nueva denuncia para investigar.", consecutivo));
    }

    public void NotificarInvestigadorReasignado(string? correoInvestigador, int? consecutivo, string? nombreAnterior)
    {
        string radicado = RadicadoTexto(consecutivo);
        string origen = string.IsNullOrWhiteSpace(nombreAnterior) ? "otro investigador" : nombreAnterior;
        EnviarSeguro(correoInvestigador, $"Investigación reasignada - Radicado {radicado}",
            Plantilla("Investigación reasignada", $"Se le ha reasignado una investigación anteriormente a cargo de {origen}.", consecutivo));
    }

    public void NotificarInvestigadorReactivado(string? correoInvestigador, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoInvestigador, $"Investigación reactivada - Radicado {radicado}",
            Plantilla("Investigación reactivada", "El denunciante aportó nueva información sobre la denuncia. Su investigación ha sido reactivada.", consecutivo));
    }

    public void NotificarSolicitudInformacion(List<string> correosConsultores, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correosConsultores, $"Solicitud de información - Radicado {radicado}",
            Plantilla("Solicitud de información del investigador", "Un investigador solicita información adicional sobre una denuncia.", consecutivo));
    }

    public void NotificarRespuestaInvestigador(string? correoInvestigador, int? consecutivo)
    {
        string radicado = RadicadoTexto(consecutivo);
        EnviarSeguro(correoInvestigador, $"Respondieron su solicitud - Radicado {radicado}",
            Plantilla("Respuesta a solicitud de información", "El consultor respondió a su solicitud de información sobre la denuncia.", consecutivo));
    }
}
