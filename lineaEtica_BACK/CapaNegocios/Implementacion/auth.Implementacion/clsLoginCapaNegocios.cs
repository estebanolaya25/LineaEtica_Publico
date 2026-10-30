using CapaDatos.Interfaz.auth.interfaz;
using CapaDto.Common;
using CapaDto.Peticiones;
using CapaDto.Respuestas;
using CapaNegocios.Implementacion.Email.Implementacion;
using CapaNegocios.Interfaz.auth.interfaz;
using CapaNegocios.Interfaz.Email.Interfaz;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace CapaNegocios.Implementacion.auth.Implementacion
{
    public class clsLoginCapaNegocios : IloginCapaNegocios
    {
        private readonly ILogger<clsLoginCapaNegocios> _logger;
        protected readonly IloginCapaDatos ilogincapadatos;
        private readonly IConfiguration _configuration;
        protected readonly IEMailService _iEMailService;

        public clsLoginCapaNegocios(IloginCapaDatos ilogincapadatos, IConfiguration configuration, IEMailService iEMailService, ILogger<clsLoginCapaNegocios> logger)
        {
            this.ilogincapadatos = ilogincapadatos;
            _configuration = configuration;
            _iEMailService = iEMailService;
            _logger = logger;
        }

        public async Task<DataTable> autenticarUsuario(string Usuario, string Contrasena)
        {
            return await ilogincapadatos.autenticarUsuario(Usuario, Contrasena);
        }

        public async Task<CurrentUserInfo> Isadmin(string username)
        {
            return await ilogincapadatos.Isadmin(username);
        }

        public async Task<int> iduser(string username)
        {
            return await ilogincapadatos.iduser(username);
        }

        public async Task<string> Cambiodepassword(string password, string usuario, string newpassword)
        {
            return await ilogincapadatos.Cambiodepassword(password, usuario, newpassword);
        }

        public async Task<string> Updtepassword(int idUsuario, string newpassword)
        {
            return await ilogincapadatos.Updtepassword(idUsuario, newpassword);
        }

        public async Task<bool> CrearUsuario(UsuarioDto objRegistro)
        {
            const string claveDefault = "Denuncias2026*";
            return await ilogincapadatos.CrearUsuario(objRegistro, claveDefault);
        }

        public async Task<(bool exito, string mensaje)> RegistrarUsuario(CapaDto.Peticiones.RegistroDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido.");

            if (string.IsNullOrWhiteSpace(dto.Correo))
                return (false, "El correo es requerido.");

            if (dto.Clave != dto.ConfirmarClave)
                return (false, "Las contraseñas no coinciden.");

            if (dto.Clave.Length < 8)
                return (false, "La contraseña debe tener al menos 8 caracteres.");

            return await ilogincapadatos.RegistrarUsuario(dto);
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosapp(int? idEmpresaFiltro = null)
        {
            return await ilogincapadatos.ListaUsuariosapp(idEmpresaFiltro);
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosappxTipoUsuario(int idTipoUsuario, int? idEmpresaFiltro = null)
        {
            return await ilogincapadatos.ListaUsuariosappxTipoUsuario(idTipoUsuario, idEmpresaFiltro);
        }

        public async Task<bool> EditarUsuario(UsuarioDto objRegistro)
        {
            const string claveDefault = "Denuncias2026*";
            return await ilogincapadatos.EditarUsuario(objRegistro, claveDefault);
        }

        public string GenerarPassword()
        {
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*-_=+";
            Random random = new Random();
            return new string(Enumerable.Range(0, 9)
                .Select(_ => caracteres[random.Next(caracteres.Length)])
                .ToArray());
        }

        public async Task<bool> DatosRecuperacion(string Email)
        {
            DataTable datosUsuario = await ilogincapadatos.DatosRecuperacion(Email);

            if (datosUsuario.Rows.Count > 0)
            {
                string Nuevapassword = mtdCrearRandomPassword(10);
                bool cambiocontrasenia = await ilogincapadatos.CambiodepasswordAleatorio(Convert.ToInt32(datosUsuario.Rows[0]["Id"]), Nuevapassword);

                if (cambiocontrasenia)
                {
                    string urlambienteRecuperacion = _configuration["UrlAmbiente:urlambiente"]?.ToString() ?? "";
                    EmailMessageRequestDto objetomail = new EmailMessageRequestDto();
                    List<string> destinatarios = new List<string> { Email };

                    objetomail.To = destinatarios;
                    objetomail.Subject = "Restauración de contraseña";
                    objetomail.Body = EmailTemplateHelper.Layout(
                        "Contraseña restablecida",
                        EmailTemplateHelper.Parrafo("Su contraseña ha sido restablecida. Use las siguientes credenciales para ingresar:")
                        + EmailTemplateHelper.CajaCredenciales(Email, Nuevapassword)
                        + EmailTemplateHelper.Parrafo("Se le recomienda cambiarla apenas ingrese."),
                        urlambienteRecuperacion,
                        urlambienteRecuperacion,
                        "Ir al aplicativo");

                    try
                    {
                        var resultadoEnvio = await _iEMailService.SendMail(objetomail);
                        if (resultadoEnvio != null && resultadoEnvio.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                            _logger.LogWarning("Fallo al enviar correo de recuperación de contraseña a {Email}: {Resultado}", Email, resultadoEnvio);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Excepción al enviar correo de recuperación de contraseña a {Email}", Email);
                        return true;
                    }
                }
            }
            else
            {
                throw new Exception("Error No se encuentra usuario");
            }

            return false;
        }

        private string mtdCrearRandomPassword(int PasswordLength)
        {
            string _allowedChars = "ASDFGHJKLQWERTYUIOPZXCVBNMasdfghjklqwertyuiopzxcvbnm0123456789";
            Byte[] randomBytes = new Byte[PasswordLength];
            char[] chars = new char[PasswordLength];
            int allowedCharCount = _allowedChars.Length;

            for (int i = 0; i < PasswordLength; i++)
            {
                Random randomObj = new Random();
                randomObj.NextBytes(randomBytes);
                chars[i] = _allowedChars[(int)randomBytes[i] % allowedCharCount];
            }

            return new string(chars);
        }

        public async Task<List<UsuariolistDto>> ListaUsuariosappmisareas(int IdUsuario)
        {
            return await ilogincapadatos.ListaUsuariosappmisareas(IdUsuario);
        }

        private async Task EnviarNotificacionCreaccionUsuario(UsuarioDto objRegistro, string RandomPass)
        {
            bool enIngles = objRegistro.Lang == "en";
            string urlambiente = _configuration["UrlAmbiente:urlambiente"]?.ToString() ?? "";
            string nombreCompleto = string.Concat(objRegistro.Nombres, " ", objRegistro.Apellidos);

            string subject = enIngles
                ? $"Registration Notification - {nombreCompleto}"
                : $"Notificación de Inscripción - {nombreCompleto}";

            string cuerpo = enIngles
                ? EmailTemplateHelper.Layout(
                    "Access credentials",
                    EmailTemplateHelper.Parrafo($"The following user has been successfully created on the platform: <b>{nombreCompleto}</b> ({objRegistro.Email}).")
                    + EmailTemplateHelper.CajaCredenciales(objRegistro.Email, RandomPass)
                    + EmailTemplateHelper.Parrafo("Please log in using the button below."),
                    urlambiente, urlambiente, "Go to the platform")
                : EmailTemplateHelper.Layout(
                    "Credenciales de acceso",
                    EmailTemplateHelper.Parrafo($"Se ha creado con éxito el siguiente usuario en la plataforma: <b>{nombreCompleto}</b> ({objRegistro.Email}).")
                    + EmailTemplateHelper.CajaCredenciales(objRegistro.Email, RandomPass)
                    + EmailTemplateHelper.Parrafo("Ingrese al aplicativo con el botón de abajo."),
                    urlambiente, urlambiente, "Ir al aplicativo");

            try
            {
                await _iEMailService.SendMail(new EmailMessageRequestDto
                {
                    To = new List<string> { objRegistro.Email },
                    Subject = subject,
                    Body = cuerpo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar correo de creación de usuario {objRegistro.Email}: {ex.Message}");
            }
        }

        private async Task EnviarNotificacionEdicionUsuario(UsuarioDto objRegistro, string PassTemporal)
        {
            string urlambiente = _configuration["UrlAmbiente:urlambiente"]?.ToString() ?? "";
            string nombreCompleto = string.Concat(objRegistro.Nombres, " ", objRegistro.Apellidos);

            string cuerpo = EmailTemplateHelper.Layout(
                "Credenciales de acceso - Edición",
                EmailTemplateHelper.Parrafo($"Se ha editado con éxito el siguiente usuario en la plataforma: <b>{nombreCompleto}</b> ({objRegistro.Email}).")
                + EmailTemplateHelper.CajaCredenciales(objRegistro.Email, PassTemporal)
                + EmailTemplateHelper.Parrafo("Ingrese al aplicativo con el botón de abajo."),
                urlambiente, urlambiente, "Ir al aplicativo");

            try
            {
                await _iEMailService.SendMail(new EmailMessageRequestDto
                {
                    To = new List<string> { objRegistro.Email },
                    Subject = $"Notificación de Edición de usuario - {nombreCompleto}",
                    Body = cuerpo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar correo de edición {objRegistro.Email}: {ex.Message}");
            }
        }

        public async Task GuardarRefreshTokenInicio(int idUsuario, string token, DateTime expiracion)
        {
            await ilogincapadatos.GuardarRefreshToken(idUsuario, token, expiracion);
        }

        public async Task<AuthResponseDto> RefrescarToken(string refreshToken)
        {
            var (valido, idUsuario, usuario, rol, idEmpresa, nombreEmpresa) = await ilogincapadatos.ValidarRefreshToken(refreshToken);

            if (!valido)
                throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

            await ilogincapadatos.RevocarRefreshToken(refreshToken);

            string nuevoAccessToken  = GenerarAccessTokenInterno(usuario, rol, idUsuario, idEmpresa, nombreEmpresa);
            string nuevoRefreshToken = GenerarRefreshTokenInterno();
            DateTime expiracion = DateTime.UtcNow.AddDays(ObtenerDiasExpiracionRefresh());

            await ilogincapadatos.GuardarRefreshToken(idUsuario, nuevoRefreshToken, expiracion);

            return new AuthResponseDto
            {
                access_token  = nuevoAccessToken,
                refresh_token = nuevoRefreshToken,
                expiration    = _configuration["Jwt:JWT_EXPIRE_MINUTES"],
                user          = usuario,
                userId        = idUsuario
            };
        }

        public async Task<bool> RevocarToken(string refreshToken)
        {
            var (valido, _, _, _, _, _) = await ilogincapadatos.ValidarRefreshToken(refreshToken);
            if (!valido)
                return false;

            await ilogincapadatos.RevocarRefreshToken(refreshToken);
            return true;
        }

        private string GenerarAccessTokenInterno(string email, string rol, int userId, int? idEmpresa, string? nombreEmpresa)
        {
            var secretKey     = _configuration["Jwt:Key"];
            var audienceToken = _configuration["Jwt:Audience"];
            var issuerToken   = _configuration["Jwt:Issuer"];
            var expireTime    = _configuration["Jwt:JWT_EXPIRE_MINUTES"];

            var securityKey        = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("name",   email),
                new System.Security.Claims.Claim("sub",    email),
                new System.Security.Claims.Claim("email",  email),
                new System.Security.Claims.Claim("role",   rol),
                new System.Security.Claims.Claim("userId", userId.ToString())
            };
            if (idEmpresa.HasValue)
                claims.Add(new System.Security.Claims.Claim("idEmpresa", idEmpresa.Value.ToString()));
            if (!string.IsNullOrWhiteSpace(nombreEmpresa))
                claims.Add(new System.Security.Claims.Claim("nombreEmpresa", nombreEmpresa));

            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(claims);

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
                signingCredentials: signingCredentials);

            return tokenHandler.WriteToken(jwtToken);
        }

        private string GenerarRefreshTokenInterno()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private int ObtenerDiasExpiracionRefresh()
        {
            return int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int dias) ? dias : 7;
        }

        public async Task<(bool bloqueado, int idUsuario)> VerificarBloqueo(string usuario)
        {
            return await ilogincapadatos.VerificarBloqueo(usuario);
        }

        public async Task RegistrarLoginExitoso(int idUsuario, string usuario, string ip, string dispositivo)
        {
            await ilogincapadatos.ActualizarUltimoLogin(idUsuario, ip);
            await ilogincapadatos.ResetearIntentosFallidos(idUsuario);
            await ilogincapadatos.RegistrarSesion(usuario, idUsuario, ip, dispositivo, true, null);
        }

        public async Task RegistrarLoginFallido(string usuario, string ip, string dispositivo, string motivo)
        {
            var (_, idUsuario) = await ilogincapadatos.VerificarBloqueo(usuario);
            await ilogincapadatos.IncrementarIntentosFallidos(usuario);
            await ilogincapadatos.RegistrarSesion(usuario, idUsuario > 0 ? idUsuario : (int?)null, ip, dispositivo, false, motivo);
        }

        public async Task CerrarSesionUsuario(int idUsuario)
        {
            await ilogincapadatos.CerrarSesion(idUsuario);
        }

        public async Task RegistrarAuditoriaAccion(int idUsuario, string accion, int? realizadoPor, string ip, string descripcion, string campo = null, string valorAnterior = null, string valorNuevo = null)
        {
            await ilogincapadatos.RegistrarAuditoria(idUsuario, accion, campo, valorAnterior, valorNuevo, realizadoPor, ip, descripcion);
        }
    }
}
