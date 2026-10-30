using CapaDTO.Notificaciones;

namespace CapaDatos.Interfaz.notificaciones.interfaz;

public interface INotificacionesCapaDatos
{
    Task<List<NotificacionDto>> ObtenerNoLeidas(int idUsuario);
    Task MarcarLeidas(int idUsuario);
    Task MarcarLeida(int id, int idUsuario);
    Task Insertar(int idUsuario, string mensaje, string tipo, int? idFormularioUsuario);
    Task InsertarAAdmins(string mensaje, string tipo, int? idFormularioUsuario, int? idEmpresa);
    Task InsertarADuenio(int idFormularioUsuario, string mensaje, string tipo);
    Task<List<string>> ObtenerCorreosConsultoresEmpresa(int? idEmpresa);
}
