using CapaDTO.Notificaciones;

namespace CapaNegocios.Interfaz.notificaciones.interfaz;

public interface INotificacionesCapaNegocios
{
    Task<List<NotificacionDto>> ObtenerNoLeidas(int idUsuario);
    Task MarcarLeidas(int idUsuario);
    Task MarcarLeida(int id, int idUsuario);
}
