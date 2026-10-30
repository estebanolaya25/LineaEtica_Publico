using CapaDto.Peticiones;
using CapaDto.Respuestas;
using CapaNegocios.Interfaz.auth.interfaz;
using lineaEtica_Api.jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;

namespace lineaEtica_Api.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly ILogger<authController> _logger;
        protected readonly IloginCapaNegocios _iloginCapaNegocios;
        private readonly IConfiguration _configuration;
        private readonly TokenGenerator _itogengenerator;

        public authController(IloginCapaNegocios iloginCapaNegocios, TokenGenerator itogengenerator, IConfiguration configuration, ILogger<authController> logger)
        {
            _iloginCapaNegocios = iloginCapaNegocios;
            _configuration = configuration;
            _itogengenerator = itogengenerator;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("login")]
        public async Task<IActionResult> Authenticate(LoginRequest login)
        {       
            
            if (login == null)
                return BadRequest(ModelState);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string dispositivo = Request.Headers["User-Agent"].ToString();

            var (bloqueado, idUsuarioBloqueado) = await _iloginCapaNegocios.VerificarBloqueo(login.email);
            if (bloqueado)
            {
                await _iloginCapaNegocios.RegistrarLoginFallido(login.email, ip, dispositivo, "Usuario bloqueado");
                return Unauthorized(new { mensaje = "Usuario bloqueado. Contacte al administrador." });
            }

            DataTable dtInformacion = await _iloginCapaNegocios.autenticarUsuario(login.email, login.password);

            if (dtInformacion.Rows.Count > 0)
            {
                AuthResponseDto response = _itogengenerator.GenerateTokenJwt(login, dtInformacion);

                DateTime expiracion = _itogengenerator.ObtenerExpiracionRefreshToken();
                await _iloginCapaNegocios.GuardarRefreshTokenInicio(response.userId, response.refresh_token, expiracion);
                await _iloginCapaNegocios.RegistrarLoginExitoso(response.userId, login.email, ip, dispositivo);

                return Ok(response);
            }
            else
            {
                await _iloginCapaNegocios.RegistrarLoginFallido(login.email, ip, dispositivo, "Credenciales incorrectas");
                return Unauthorized();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.refreshToken))
                return BadRequest("Refresh token requerido.");

            try
            {
                AuthResponseDto response = await _iloginCapaNegocios.RefrescarToken(request.refreshToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Refresh token inválido o expirado.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("revoke")]
        public async Task<IActionResult> Revoke(RefreshTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.refreshToken))
                return BadRequest("Refresh token requerido.");

            bool resultado = await _iloginCapaNegocios.RevocarToken(request.refreshToken);

            if (resultado && User.Identity.IsAuthenticated)
            {
                int idUsuario = await _iloginCapaNegocios.iduser(User.Identity.Name);
                if (idUsuario > 0)
                    await _iloginCapaNegocios.CerrarSesionUsuario(idUsuario);
            }

            return resultado ? Ok("Sesión cerrada correctamente.") : BadRequest("Token no encontrado.");
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Recuperacion")]
        public async Task<IActionResult> Recuperacion(RecuperacionDto objRecuperacion)
        {
            if (objRecuperacion == null)
                return BadRequest(ModelState);

            try
            {
                bool Respuesta = await _iloginCapaNegocios.DatosRecuperacion(objRecuperacion.CorreoElectronico);
                return Respuesta ? Ok() : BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("current")]
        public async Task<IActionResult> userinformartion2()
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            string UserName = User.Identity.Name;
            CurrentUserInfo userinfo = await _iloginCapaNegocios.Isadmin(UserName);
            return Ok(userinfo);
        }

        [HttpPost]
        [Authorize]
        [Route("passwordchange")]
        public async Task<IActionResult> changepassword(PasswordChangeModel changepassword)
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            string UserName = User.Identity.Name;

            try
            {
                string RESPUESTA = await _iloginCapaNegocios.Cambiodepassword(changepassword.password, UserName, changepassword.newpassword);
                return Ok(new { Respuesta = RESPUESTA });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        [Route("passwordUpdate")]
        public async Task<IActionResult> passwordUpdate(PasswordUpdateModel changepassword)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (!int.TryParse(userIdClaim, out int idUsuario) || idUsuario <= 0)
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            if (string.IsNullOrWhiteSpace(changepassword?.newpassword))
                return BadRequest(new { mensaje = "La nueva contraseña no puede estar vacía." });

            try
            {
                string RESPUESTA = await _iloginCapaNegocios.Updtepassword(idUsuario, changepassword.newpassword);
                return Ok(new { Respuesta = RESPUESTA });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("registro")]
        public async Task<IActionResult> Registro(CapaDto.Peticiones.RegistroDto dto)
        {
            if (dto == null)
                return BadRequest("Datos de registro inválidos.");

            try
            {
                var (exito, mensaje) = await _iloginCapaNegocios.RegistrarUsuario(dto);
                return exito ? Ok(new { exito, mensaje }) : BadRequest(new { exito, mensaje });
            }
            catch (Exception ex)
            {
                return BadRequest(new { exito = false, mensaje = ex.Message });
            }
        }

        private int? IdEmpresaActual =>
            int.TryParse(User.Claims.FirstOrDefault(c => c.Type == "idEmpresa")?.Value, out var idEmpresa)
                ? idEmpresa : null;

        private IActionResult? ValidarEmpresaUsuario(UsuarioDto objRegistro, string rolActual)
        {
            if (objRegistro.IdTipoUsuario == 4 && rolActual != "Jefe del proceso")
                return StatusCode(403, new { mensaje = "Solo un Jefe del proceso puede crear o editar usuarios de tipo Jefe del proceso." });

            if (objRegistro.IdTipoUsuario == 4)
            {
                objRegistro.IdEmpresa = null;
                return null;
            }

            if (objRegistro.IdTipoUsuario == 1 || objRegistro.IdTipoUsuario == 3 || objRegistro.IdTipoUsuario == 5)
            {
                if (objRegistro.IdEmpresa == null)
                    return StatusCode(400, new { mensaje = "Debe indicar la empresa para este tipo de usuario." });

                if (rolActual == "Consultor")
                {
                    var miEmpresa = IdEmpresaActual;
                    if (miEmpresa == null)
                        return StatusCode(403, new { mensaje = "Su usuario no tiene empresa asignada. Contacte al administrador." });
                    if (objRegistro.IdEmpresa != miEmpresa)
                        return StatusCode(403, new { mensaje = "Solo puede crear/editar usuarios de su propia empresa." });
                }
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Consultor,Jefe del proceso")]
        [Route("addnewuser")]
        public async Task<IActionResult> addnewuser(UsuarioDto objRegistro)
        {
            string rolActual = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";

            if (rolActual == "Jefe del proceso" && objRegistro.IdTipoUsuario != 1 && objRegistro.IdTipoUsuario != 3 && objRegistro.IdTipoUsuario != 4 && objRegistro.IdTipoUsuario != 5)
                return StatusCode(403, new { mensaje = "Solo puede crear usuarios Consultor, Investigador, Jefe del proceso o Registrador." });

            var errorEmpresa = ValidarEmpresaUsuario(objRegistro, rolActual);
            if (errorEmpresa != null) return errorEmpresa;

            try
            {
                bool respuesta = await _iloginCapaNegocios.CrearUsuario(objRegistro);
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Consultor,Jefe del proceso")]
        [Route("edituser")]
        public async Task<IActionResult> edituser(UsuarioDto objRegistro)
        {
            string rolActual = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";

            if (rolActual == "Jefe del proceso" && objRegistro.IdTipoUsuario != 1 && objRegistro.IdTipoUsuario != 3 && objRegistro.IdTipoUsuario != 4 && objRegistro.IdTipoUsuario != 5)
                return StatusCode(403, new { mensaje = "Solo puede editar usuarios Consultor, Investigador, Jefe del proceso o Registrador." });

            var errorEmpresa = ValidarEmpresaUsuario(objRegistro, rolActual);
            if (errorEmpresa != null) return errorEmpresa;

            try
            {
                bool respuesta = await _iloginCapaNegocios.EditarUsuario(objRegistro);
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Consultor,Jefe del proceso")]
        [Route("listaUsuariosAPP")]
        public async Task<IActionResult> listaUsuariosAPP()
        {
            if (!User.Identity.IsAuthenticated)
                return BadRequest();

            try
            {
                string rol = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "";

                List<UsuariolistDto> lstUsuariosapp;
                if (rol == "Jefe del proceso")
                    lstUsuariosapp = (await _iloginCapaNegocios.ListaUsuariosapp())
                        .Where(u => u.IdTipoUsuario == 1 || u.IdTipoUsuario == 3 || u.IdTipoUsuario == 4 || u.IdTipoUsuario == 5)
                        .ToList();
                else
                {
                    var miEmpresa = IdEmpresaActual;
                    if (miEmpresa == null)
                        return StatusCode(403, new { mensaje = "Su usuario no tiene empresa asignada. Contacte al administrador." });
                    lstUsuariosapp = await _iloginCapaNegocios.ListaUsuariosapp(miEmpresa);
                }

                return Ok(lstUsuariosapp);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
