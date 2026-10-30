using CapaDto.Peticiones;
using CapaDto.Respuestas;
using System.Data;

namespace CapaNegocios.Interfaz.auth.interfaz
{
    public interface IloginCapaNegocios
    {
        Task<DataTable> autenticarUsuario(string Usuario, string Contrasena);
        Task<CurrentUserInfo> Isadmin(string username);
        Task<int> iduser(string username);
        Task<string> Cambiodepassword(string password, string usuario, string newpassword);
        Task<string> Updtepassword(int idUsuario, string newpassword);
        Task<bool> CrearUsuario(UsuarioDto objRegistro);
        Task<(bool exito, string mensaje)> RegistrarUsuario(RegistroDto dto);
        Task<List<UsuariolistDto>> ListaUsuariosapp(int? idEmpresaFiltro = null);
        Task<List<UsuariolistDto>> ListaUsuariosappxTipoUsuario(int idTipoUsuario, int? idEmpresaFiltro = null);
        Task<bool> EditarUsuario(UsuarioDto objRegistro);
        Task<bool> DatosRecuperacion(string Email);
        Task<List<UsuariolistDto>> ListaUsuariosappmisareas(int IdUsuario);

        Task GuardarRefreshTokenInicio(int idUsuario, string token, DateTime expiracion);
        Task<AuthResponseDto> RefrescarToken(string refreshToken);
        Task<bool> RevocarToken(string refreshToken);

        Task<(bool bloqueado, int idUsuario)> VerificarBloqueo(string usuario);
        Task RegistrarLoginExitoso(int idUsuario, string usuario, string ip, string dispositivo);
        Task RegistrarLoginFallido(string usuario, string ip, string dispositivo, string motivo);
        Task CerrarSesionUsuario(int idUsuario);
        Task RegistrarAuditoriaAccion(int idUsuario, string accion, int? realizadoPor, string ip, string descripcion, string campo = null, string valorAnterior = null, string valorNuevo = null);
    }
}
