using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDTO.Notificaciones;
using CapaNegocios.Interfaz.notificaciones.interfaz;

namespace CapaNegocios.Implementacion.notificaciones.Implementacion;

public class clsNotificacionesCapaNegocios : INotificacionesCapaNegocios
{
    private readonly INotificacionesCapaDatos _datos;

    public clsNotificacionesCapaNegocios(INotificacionesCapaDatos datos)
    {
        _datos = datos;
    }

    public Task<List<NotificacionDto>> ObtenerNoLeidas(int idUsuario)
        => _datos.ObtenerNoLeidas(idUsuario);

    public Task MarcarLeidas(int idUsuario)
        => _datos.MarcarLeidas(idUsuario);

    public Task MarcarLeida(int id, int idUsuario)
        => _datos.MarcarLeida(id, idUsuario);
}
