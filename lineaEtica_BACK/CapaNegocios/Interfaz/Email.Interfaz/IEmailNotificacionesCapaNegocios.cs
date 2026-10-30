namespace CapaNegocios.Interfaz.Email.Interfaz;

public interface IEmailNotificacionesCapaNegocios
{
    void NotificarNuevaDenuncia(List<string> correosConsultores, int? consecutivo, bool esCorreccion);
    void NotificarDenunciaAprobada(string? correoDenunciante, int? consecutivo);
    void NotificarDenunciaDevuelta(string? correoDenunciante, int? consecutivo);
    void NotificarDenunciaRechazada(string? correoDenunciante, int? consecutivo);
    void NotificarDenunciaCerrada(string? correoDenunciante, int? consecutivo);
    void NotificarInvestigadorAsignado(string? correoInvestigador, int? consecutivo);
    void NotificarInvestigadorReasignado(string? correoInvestigador, int? consecutivo, string? nombreAnterior);
    void NotificarInvestigadorReactivado(string? correoInvestigador, int? consecutivo);
    void NotificarSolicitudInformacion(List<string> correosConsultores, int? consecutivo);
    void NotificarRespuestaInvestigador(string? correoInvestigador, int? consecutivo);
}
