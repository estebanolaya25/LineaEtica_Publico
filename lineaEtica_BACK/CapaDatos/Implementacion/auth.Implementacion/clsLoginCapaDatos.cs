using CapaDatos.Interfaz.auth.interfaz;
using CapaDatos.util;
using CapaDto.Peticiones;
using CapaDto.Respuestas;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text.RegularExpressions;

namespace CapaDatos.Implementacion.auth.Implementacion
{
    public class clsLoginCapaDatos : IloginCapaDatos
    {
        private readonly ILogger<clsLoginCapaDatos> _logger;
        private readonly IConfiguration _configuration;

        public clsLoginCapaDatos(IConfiguration configuration, ILogger<clsLoginCapaDatos> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string ConnString => _configuration.GetConnectionString("SQLConnectionString");

        public async Task<DataTable> autenticarUsuario(string Usuario, string Contrasena)
        {
            string strEncryptPass = cEncriptacion.CifradoData(Contrasena);
            const string query = @"
                SELECT u.id AS Id,
                       CASE
                           WHEN u.es_admin = 1 THEN 'Consultor'
                           WHEN u.es_admin = 2 THEN 'Investigador'
                           WHEN u.es_admin = 3 THEN 'Jefe del proceso'
                           WHEN u.es_admin = 4 THEN 'Registrador'
                           ELSE 'Denunciante'
                       END AS Rol,
                       ISNULL(u.actualizar_pass, 0) AS ActualizarPass,
                       u.id_empresa_fk AS IdEmpresa,
                       e.nombre AS NombreEmpresa
                FROM gnl_usuario u
                LEFT JOIN gnl_empresa e ON e.id = u.id_empresa_fk
                WHERE u.usuario = @Usuario AND u.clave = @Password AND u.activo = 1";

            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", Usuario);
                cmd.Parameters.AddWithValue("@Password", strEncryptPass);
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error autenticarUsuario: " + ex.Message);
                dt.Rows.Clear(); dt.Columns.Clear();
            }
            return dt;
        }

        public async Task<DataTable> DatosRecuperacion(string Email)
        {
            const string query = @"
                SELECT u.id AS Id, u.nombre AS Nombre, u.correo
                FROM gnl_usuario u
                WHERE u.correo = @Email AND u.activo = 1";

            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Email", Email ?? "");
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error DatosRecuperacion: " + ex.Message);
                dt.Rows.Clear(); dt.Columns.Clear();
            }
            return dt;
        }

        public async Task<CurrentUserInfo> Isadmin(string username)
        {
            const string query = @"
                SELECT u.id, u.nombre AS Nombre, u.correo AS Email,
                       CASE
                           WHEN u.es_admin = 1 THEN 'Consultor'
                           WHEN u.es_admin = 2 THEN 'Investigador'
                           WHEN u.es_admin = 3 THEN 'Jefe del proceso'
                           WHEN u.es_admin = 4 THEN 'Registrador'
                           ELSE 'Denunciante'
                       END AS Rol
                FROM gnl_usuario u
                WHERE u.usuario = @Usuario AND u.activo = 1";

            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", username);
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Isadmin: " + ex.Message);
            }

            if (dt.Rows.Count > 0)
            {
                return new CurrentUserInfo
                {
                    IdUsuario = Convert.ToInt32(dt.Rows[0]["id"]),
                    Nombre = dt.Rows[0]["Nombre"]?.ToString()?.Trim() ?? "",
                    Apellido = "",
                    Usuario = dt.Rows[0]["Email"]?.ToString()?.Trim() ?? "",
                    Rol = dt.Rows[0]["Rol"]?.ToString()?.Trim() ?? "",
                    ActualizarPass = false
                };
            }
            return null;
        }

        public async Task<int> iduser(string username)
        {
            const string query = "SELECT id FROM gnl_usuario WHERE usuario = @Usuario";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", username);
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error iduser: " + ex.Message);
                return 0;
            }
        }

        public async Task<string> Cambiodepassword(string password, string usuario, string newpassword)
        {
            string strMensaje = string.Empty;
            string passActual = await ObtenerClaveUsuario(usuario);

            if (mtdCompararContrasenasEncriptadas(password, passActual))
            {
                if (!mtdValidarContrasena(newpassword, ref strMensaje))
                    return strMensaje;

                if (!mtdCompararContrasenasEncriptadas(newpassword, passActual))
                {
                    int id = await iduser(usuario);
                    await ActualizarClave(newpassword, id);
                    return "Contraseña cambiada con éxito.";
                }
                else
                    throw new InvalidOperationException("1");
            }
            else
                throw new InvalidOperationException("2");
        }

        public async Task<bool> CambiodepasswordAleatorio(int IdUsuario, string NuevoPass)
        {
            try
            {
                await ActualizarClave(NuevoPass, IdUsuario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CambiodepasswordAleatorio: " + ex.Message);
                return false;
            }
        }

        public async Task<string> Updtepassword(int idUsuario, string newpassword)
        {
            string strMensaje = string.Empty;
            if (!mtdValidarContrasena(newpassword, ref strMensaje))
                return strMensaje;

            string passActual = await ObtenerClavePorId(idUsuario);
            if (!mtdCompararContrasenasEncriptadas(newpassword, passActual))
            {
                await ActualizarClave(newpassword, idUsuario);
                return "OK";
            }
            throw new InvalidOperationException("La nueva contraseña no puede ser igual a la actual.");
        }

        private async Task<string> ObtenerClaveUsuario(string usuario)
        {
            const string query = "SELECT LTRIM(RTRIM(clave)) FROM gnl_usuario WHERE usuario = @Usuario";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString()?.Trim() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private async Task<string> ObtenerClavePorId(int idUsuario)
        {
            const string query = "SELECT LTRIM(RTRIM(clave)) FROM gnl_usuario WHERE id = @Id";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Id", idUsuario);
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString()?.Trim() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        private async Task ActualizarClave(string nuevaClave, int idUsuario)
        {
            string strPassEncrypted = cEncriptacion.CifradoData(nuevaClave);
            const string query = "UPDATE gnl_usuario SET clave = @Clave, actualizar_pass = 0, fecha_actualizacion = GETDATE() WHERE id = @Id";
            using var connection = new SqlConnection(ConnString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@Clave", strPassEncrypted);
            cmd.Parameters.AddWithValue("@Id", idUsuario);
            await cmd.ExecuteNonQueryAsync();
        }

        public bool mtdValidarContrasena(string strContrasena, ref string strMensaje)
        {
            if (strContrasena.Length < 8)
            {
                strMensaje = "1";
                return false;
            }
            string strRegExpr = @"(?=^.{8,}$)(?=.*?[0-9]*)(?=.*[!-/:-@{-~Ç-■^_])(?=.*[A-Z]+)(?=.*[a-z]*).*$";
            if (!Regex.IsMatch(strContrasena, strRegExpr))
            {
                strMensaje = "2";
                return false;
            }
            return true;
        }

        private bool mtdCompararContrasenasEncriptadas(string strPassUser, string strPassBD)
        {
            string strPassUserCrypt = cEncriptacion.CifradoData(strPassUser);
            return string.Compare(strPassUserCrypt, strPassBD) == 0;
        }

        public async Task<bool> CrearUsuario(UsuarioDto objRegistro, string Randompass)
        {
            string usuario = objRegistro.Email ?? "";

            bool usuarioExiste = await ValidaUsuarioExistente(usuario);
            if (usuarioExiste)
                throw new InvalidOperationException("El usuario ya se encuentra creado");

            string strPassEncrypted = cEncriptacion.CifradoData(Randompass);
            string nombre = string.Concat(objRegistro.Nombres ?? "", " ", objRegistro.Apellidos ?? "").Trim();
            int esAdmin = objRegistro.IdTipoUsuario == 1 ? 1
                        : objRegistro.IdTipoUsuario == 3 ? 2
                        : objRegistro.IdTipoUsuario == 4 ? 3
                        : objRegistro.IdTipoUsuario == 5 ? 4
                        : 0;

            const string query = @"
                INSERT INTO gnl_usuario (usuario, nombre, tipo_documento, numero_documento, correo, clave, es_admin, activo, actualizar_pass, id_empresa_fk, fecha_creacion)
                VALUES (@Usuario, @Nombre, @TipoDoc, @NumDoc, @Correo, @Clave, @EsAdmin, 1, 1, @IdEmpresa, GETDATE())";

            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@TipoDoc", objRegistro.TipoIdentificacion);
                cmd.Parameters.AddWithValue("@NumDoc", objRegistro.Identificacion ?? "");
                cmd.Parameters.AddWithValue("@Correo", objRegistro.Email ?? "");
                cmd.Parameters.AddWithValue("@Clave", strPassEncrypted);
                cmd.Parameters.AddWithValue("@EsAdmin", esAdmin);
                cmd.Parameters.AddWithValue("@IdEmpresa", (object?)objRegistro.IdEmpresa ?? DBNull.Value);
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error CrearUsuario: " + ex.Message);
                throw new Exception("Error al crear usuario: " + ex.Message);
            }
        }

        public async Task<(bool exito, string mensaje)> RegistrarUsuario(RegistroDto dto)
        {
            bool existe = await ValidaUsuarioExistente(dto.Correo);
            if (existe)
                return (false, "El correo ya está registrado.");

            string strPassEncrypted = cEncriptacion.CifradoData(dto.Clave);

            const string query = @"
                INSERT INTO gnl_usuario (usuario, nombre, correo, clave, es_admin, activo, fecha_creacion)
                VALUES (@Usuario, @Nombre, @Correo, @Clave, 0, 1, GETDATE())";

            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", dto.Correo ?? "");
                cmd.Parameters.AddWithValue("@Nombre", dto.Nombre ?? "");
                cmd.Parameters.AddWithValue("@Correo", dto.Correo ?? "");
                cmd.Parameters.AddWithValue("@Clave", strPassEncrypted);
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0 ? (true, "Registro exitoso.") : (false, "No se pudo crear el usuario.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error RegistrarUsuario: " + ex.Message);
                return (false, "Error al registrar usuario.");
            }
        }

        private async Task<bool> ValidaUsuarioExistente(string usuario)
        {
            const string query = "SELECT 1 FROM gnl_usuario WHERE usuario = @Usuario";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
            catch { return true; }
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosapp(int? idEmpresaFiltro = null)
        {
            return MapearUsuarios(await ConsultaUsuarios(null, idEmpresaFiltro));
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosappxTipoUsuario(int idTipoUsuario, int? idEmpresaFiltro = null)
        {
            int esAdmin = idTipoUsuario == 1 ? 1
                        : idTipoUsuario == 3 ? 2
                        : idTipoUsuario == 4 ? 3
                        : idTipoUsuario == 5 ? 4
                        : 0;
            return MapearUsuarios(await ConsultaUsuarios(esAdmin, idEmpresaFiltro));
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosappmisareas(int IdUsuario)
        {
            return await ListaUsuariosapp();
        }

        private async Task<DataTable> ConsultaUsuarios(int? esAdminFiltro = null, int? idEmpresaFiltro = null)
        {
            var condiciones = new List<string>();
            if (esAdminFiltro.HasValue) condiciones.Add("u.es_admin = @EsAdmin");
            if (idEmpresaFiltro.HasValue) condiciones.Add("u.id_empresa_fk = @IdEmpresaFiltro");
            string whereClause = condiciones.Count > 0 ? "WHERE " + string.Join(" AND ", condiciones) : "";
            string query = $@"
                SELECT u.id AS Id, u.nombre AS Nombre, '' AS Apellidos,
                       ISNULL(u.numero_documento, '') AS Identificacion,
                       ISNULL(u.tipo_documento, 0) AS TipoIdentificacion,
                       ISNULL(u.correo, '') AS Email, u.usuario AS Usuario,
                       CASE WHEN u.es_admin = 1 THEN 1 WHEN u.es_admin = 2 THEN 3 WHEN u.es_admin = 3 THEN 4 WHEN u.es_admin = 4 THEN 5 ELSE 2 END AS TipoUsuario,
                       CASE WHEN u.es_admin = 1 THEN 'Consultor' WHEN u.es_admin = 2 THEN 'Investigador' WHEN u.es_admin = 3 THEN 'Jefe del proceso' WHEN u.es_admin = 4 THEN 'Registrador' ELSE 'Denunciante' END AS NombreTipoUsuario,
                       u.activo AS Activo,
                       u.id_empresa_fk AS IdEmpresa,
                       e.nombre AS NombreEmpresa
                FROM gnl_usuario u
                LEFT JOIN gnl_empresa e ON e.id = u.id_empresa_fk
                {whereClause}";

            var dt = new DataTable();
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                if (esAdminFiltro.HasValue)
                    cmd.Parameters.AddWithValue("@EsAdmin", esAdminFiltro.Value);
                if (idEmpresaFiltro.HasValue)
                    cmd.Parameters.AddWithValue("@IdEmpresaFiltro", idEmpresaFiltro.Value);
                using var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error ConsultaUsuarios: " + ex.Message);
            }
            return dt;
        }

        private List<UsuariolistDto> MapearUsuarios(DataTable dt)
        {
            var lista = new List<UsuariolistDto>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new UsuariolistDto
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Nombres = row["Nombre"]?.ToString()?.Trim() ?? "",
                    Apellidos = "",
                    Email = row["Email"]?.ToString()?.Trim() ?? "",
                    Identificacion = row["Identificacion"]?.ToString()?.Trim() ?? "",
                    TipoIdentificacion = row["TipoIdentificacion"] == DBNull.Value ? 0 : Convert.ToInt32(row["TipoIdentificacion"]),
                    Usuario = row["Usuario"]?.ToString()?.Trim() ?? "",
                    IdTipoUsuario = Convert.ToInt32(row["TipoUsuario"]),
                    NombreTipoUsuario = row["NombreTipoUsuario"]?.ToString()?.Trim() ?? "",
                    Activo = Convert.ToInt32(row["Activo"]) == 1,
                    IdEmpresa = row["IdEmpresa"] == DBNull.Value ? null : Convert.ToInt32(row["IdEmpresa"]),
                    NombreEmpresa = row["NombreEmpresa"] == DBNull.Value ? null : row["NombreEmpresa"].ToString()?.Trim()
                });
            }
            return lista;
        }

        public async Task<bool> EditarUsuario(UsuarioDto objRegistro, string passtempo)
        {
            string strPassEncrypted = cEncriptacion.CifradoData(passtempo);
            string nombre = string.Concat(objRegistro.Nombres ?? "", " ", objRegistro.Apellidos ?? "").Trim();
            int esAdmin = objRegistro.IdTipoUsuario == 1 ? 1
                        : objRegistro.IdTipoUsuario == 3 ? 2
                        : objRegistro.IdTipoUsuario == 4 ? 3
                        : objRegistro.IdTipoUsuario == 5 ? 4
                        : 0;
            int activo = objRegistro.Activo ? 1 : 0;

            const string query = @"
                UPDATE gnl_usuario SET
                    nombre = @Nombre,
                    tipo_documento = @TipoDoc,
                    numero_documento = @NumDoc,
                    correo = @Correo,
                    usuario = @Usuario,
                    es_admin = @EsAdmin,
                    activo = @Activo,
                    clave = @Clave,
                    actualizar_pass = 1,
                    id_empresa_fk = @IdEmpresa,
                    fecha_actualizacion = GETDATE()
                WHERE id = @Id";

            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@TipoDoc", objRegistro.TipoIdentificacion);
                cmd.Parameters.AddWithValue("@NumDoc", objRegistro.Identificacion ?? "");
                cmd.Parameters.AddWithValue("@Correo", objRegistro.Email ?? "");
                cmd.Parameters.AddWithValue("@Usuario", objRegistro.Email ?? "");
                cmd.Parameters.AddWithValue("@EsAdmin", esAdmin);
                cmd.Parameters.AddWithValue("@Activo", activo);
                cmd.Parameters.AddWithValue("@Clave", strPassEncrypted);
                cmd.Parameters.AddWithValue("@IdEmpresa", (object?)objRegistro.IdEmpresa ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", objRegistro.Id);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    throw new InvalidOperationException("No se encontró el usuario a actualizar");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error EditarUsuario: " + ex.Message);
                throw new InvalidOperationException("Error al actualizar el usuario");
            }
        }

        public async Task GuardarRefreshToken(int idUsuario, string token, DateTime expiracion)
        {
            const string query = @"
                INSERT INTO gnl_refresh_token (id_usuario, token, fecha_expiracion, fecha_creacion, revocado)
                VALUES (@IdUsuario, @Token, @FechaExpiracion, GETDATE(), 0)";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@FechaExpiracion", expiracion);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar refresh token: " + ex.Message);
            }
        }

        public async Task<(bool valido, int idUsuario, string usuario, string rol, int? idEmpresa, string? nombreEmpresa)> ValidarRefreshToken(string token)
        {
            const string query = @"
                SELECT rt.id_usuario, u.usuario,
                       CASE
                           WHEN u.es_admin = 1 THEN 'Consultor'
                           WHEN u.es_admin = 2 THEN 'Investigador'
                           WHEN u.es_admin = 3 THEN 'Jefe del proceso'
                           WHEN u.es_admin = 4 THEN 'Registrador'
                           ELSE 'Denunciante'
                       END AS Rol,
                       u.id_empresa_fk AS IdEmpresa,
                       e.nombre AS NombreEmpresa
                FROM gnl_refresh_token rt
                INNER JOIN gnl_usuario u ON rt.id_usuario = u.id
                LEFT JOIN gnl_empresa e ON e.id = u.id_empresa_fk
                WHERE rt.token = @Token
                  AND rt.revocado = 0
                  AND rt.fecha_expiracion > GETDATE()
                  AND u.activo = 1";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", token);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    int? idEmpresa = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                    string? nombreEmpresa = reader.IsDBNull(4) ? null : reader.GetString(4);
                    return (true, reader.GetInt32(0), reader.GetString(1), reader.GetString(2), idEmpresa, nombreEmpresa);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar refresh token: " + ex.Message);
            }
            return (false, 0, string.Empty, string.Empty, null, null);
        }

        public async Task RevocarRefreshToken(string token)
        {
            const string query = "UPDATE gnl_refresh_token SET revocado = 1, fecha_revocacion = GETDATE() WHERE token = @Token";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", token);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al revocar refresh token: " + ex.Message);
            }
        }

        public async Task RevocarTodosRefreshTokensUsuario(int idUsuario)
        {
            const string query = "UPDATE gnl_refresh_token SET revocado = 1, fecha_revocacion = GETDATE() WHERE id_usuario = @IdUsuario AND revocado = 0";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al revocar tokens del usuario: " + ex.Message);
            }
        }

        public async Task ActualizarUltimoLogin(int idUsuario, string ip) { }

        public async Task<int> IncrementarIntentosFallidos(string usuario) => 0;

        public async Task ResetearIntentosFallidos(int idUsuario) { }

        public async Task BloquearUsuario(int idUsuario) { }

        public async Task<(bool bloqueado, int idUsuario)> VerificarBloqueo(string usuario)
        {
            const string query = "SELECT id FROM gnl_usuario WHERE usuario = @Usuario AND activo = 1";
            try
            {
                using var connection = new SqlConnection(ConnString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                    return (false, Convert.ToInt32(result));
            }
            catch { }
            return (false, 0);
        }

        public async Task<int> RegistrarSesion(string usuario, int? idUsuario, string ip, string dispositivo, bool exitoso, string motivoFallo) => 0;

        public async Task CerrarSesion(int idUsuario) { }

        public async Task RegistrarAuditoria(int idUsuario, string accion, string campoModificado, string valorAnterior, string valorNuevo, int? realizadoPor, string ip, string descripcion) { }
    }
}
