using CapaDto.Peticiones;
using CapaDto.Respuestas;
using System.Data;

namespace CapaDatos.Interfaz.auth.interfaz
{
    public interface IloginCapaDatos
    {
        Task<DataTable> autenticarUsuario(string Usuario, string Contrasena);
        Task<CurrentUserInfo> Isadmin(string username);
        Task<int> iduser(string username);
        Task<string> Cambiodepassword(string password, string usuario, string newpassword);
        Task<string> Updtepassword(int idUsuario, string newpassword);
        Task<bool> CrearUsuario(UsuarioDto objRegistro, string Randompass);
        Task<(bool exito, string mensaje)> RegistrarUsuario(RegistroDto dto);
        Task<List<UsuariolistDto>> ListaUsuariosapp(int? idEmpresaFiltro = null);
        Task<List<UsuariolistDto>> ListaUsuariosappxTipoUsuario(int idTipoUsuario, int? idEmpresaFiltro = null);
        Task<bool> EditarUsuario(UsuarioDto objRegistro, string Passtemp);
        Task<DataTable> DatosRecuperacion(string Email);
        Task<bool> CambiodepasswordAleatorio(int IdUsuario, string NuevoPass);
        Task<List<UsuariolistDto>> ListaUsuariosappmisareas(int IdUsuario);

        Task GuardarRefreshToken(int idUsuario, string token, DateTime expiracion);
        Task<(bool valido, int idUsuario, string usuario, string rol, int? idEmpresa, string? nombreEmpresa)> ValidarRefreshToken(string token);
        Task RevocarRefreshToken(string token);
        Task RevocarTodosRefreshTokensUsuario(int idUsuario);

        Task ActualizarUltimoLogin(int idUsuario, string ip);
        Task<int> IncrementarIntentosFallidos(string usuario);
        Task ResetearIntentosFallidos(int idUsuario);
        Task BloquearUsuario(int idUsuario);
        Task<(bool bloqueado, int idUsuario)> VerificarBloqueo(string usuario);

        Task<int> RegistrarSesion(string usuario, int? idUsuario, string ip, string dispositivo, bool exitoso, string motivoFallo);
        Task CerrarSesion(int idUsuario);

        Task RegistrarAuditoria(int idUsuario, string accion, string campoModificado, string valorAnterior, string valorNuevo, int? realizadoPor, string ip, string descripcion);
    }
}
