using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDatos.Interfaz.registrador.interfaz;
using CapaDatos.util;
using CapaDTO.Registrador;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapaDatos.Implementacion.registrador.Implementacion;

public class clsRegistradorCapaDatos : IRegistradorCapaDatos
{
    private readonly IConfiguration _config;
    private readonly ILogger<clsRegistradorCapaDatos> _logger;
    private readonly INotificacionesCapaDatos _notif;

    private string ConnString => _config.GetConnectionString("SQLConnectionString")!;

    public clsRegistradorCapaDatos(
        IConfiguration config,
        ILogger<clsRegistradorCapaDatos> logger,
        INotificacionesCapaDatos notif)
    {
        _config = config;
        _logger = logger;
        _notif  = notif;
    }

    public async Task<BuscarUsuarioResponseDto> BuscarUsuarioPorCorreo(string correo)
    {
        return await Task.Run(() =>
        {
            using var connection = new SqlConnection(ConnString);
            connection.Open();
            using var cmd = new SqlCommand(
                "SELECT nombre, correo FROM gnl_usuario WHERE usuario = @Usuario", connection);
            cmd.Parameters.AddWithValue("@Usuario", correo);
            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                return new BuscarUsuarioResponseDto
                {
                    Existe = true,
                    Nombre = rdr.IsDBNull(0) ? null : rdr.GetString(0),
                    Correo = rdr.IsDBNull(1) ? null : rdr.GetString(1)
                };
            }
            return new BuscarUsuarioResponseDto { Existe = false, Correo = correo };
        });
    }

    public async Task<List<DenunciaRegistradorResumenDto>> GetMisDenunciasMontadas(int idRegistrador)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnString);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT ffu.id, ffu.consecutivo, ff.nombre AS nombre_formulario,
                       ISNULL(ge.nombre, 'Creado') AS estado, ffu.idgnl_estado_fk AS id_estado,
                       CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                       ffu.fecha_creacion, ffu.origen_canal
                FROM form_formulario_usuario ffu
                INNER JOIN form_formulario ff ON ffu.idform_formulario_fk = ff.id
                LEFT JOIN gnl_estado ge ON ffu.idgnl_estado_fk = ge.id
                WHERE ffu.id_registrador_fk = @idRegistrador
                ORDER BY ffu.fecha_creacion DESC", cnn);
            cmd.Parameters.AddWithValue("@idRegistrador", idRegistrador);

            using var r = cmd.ExecuteReader();
            var lista = new List<DenunciaRegistradorResumenDto>();
            while (r.Read())
            {
                lista.Add(new DenunciaRegistradorResumenDto
                {
                    Id = r.GetInt32(0),
                    Consecutivo = r.IsDBNull(1) ? null : r.GetInt32(1),
                    NombreFormulario = r.GetString(2),
                    Estado = r.GetString(3),
                    IdEstado = r.IsDBNull(4) ? 0 : r.GetInt32(4),
                    EsAnonimo = r.GetInt32(5) == 1,
                    FechaCreacion = r.IsDBNull(6) ? null : r.GetDateTime(6),
                    OrigenCanal = r.IsDBNull(7) ? null : r.GetString(7)
                });
            }
            return lista;
        });
    }

    public async Task<MontarDenunciaResponseDto> MontarDenuncia(MontarDenunciaRequestDto request, int idRegistrador)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnString);
            cnn.Open();
            using var tx = cnn.BeginTransaction();

            try
            {
                string correo = request.CorreoDestino.Trim();

                int? idUsuario = null;
                using (var cmdCheck = new SqlCommand(
                    "SELECT id FROM gnl_usuario WITH (UPDLOCK, HOLDLOCK) WHERE usuario = @Usuario", cnn, tx))
                {
                    cmdCheck.Parameters.AddWithValue("@Usuario", correo);
                    var result = cmdCheck.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        idUsuario = Convert.ToInt32(result);
                }

                bool usuarioCreado = false;
                string? passwordGenerada = null;

                if (idUsuario == null)
                {
                    passwordGenerada = GenerarPassword();
                    string claveEncriptada = cEncriptacion.CifradoData(passwordGenerada);
                    string nombre = string.IsNullOrWhiteSpace(request.NombreDestino) ? correo : request.NombreDestino.Trim();

                    using var cmdIns = new SqlCommand(@"
                        INSERT INTO gnl_usuario (usuario, nombre, correo, clave, es_admin, activo, actualizar_pass, fecha_creacion)
                        OUTPUT INSERTED.id
                        VALUES (@Usuario, @Nombre, @Correo, @Clave, 0, 1, 1, GETDATE())", cnn, tx);
                    cmdIns.Parameters.AddWithValue("@Usuario", correo);
                    cmdIns.Parameters.AddWithValue("@Nombre", nombre);
                    cmdIns.Parameters.AddWithValue("@Correo", correo);
                    cmdIns.Parameters.AddWithValue("@Clave", claveEncriptada);
                    idUsuario = Convert.ToInt32(cmdIns.ExecuteScalar());
                    usuarioCreado = true;
                }

                int? idEmpresaRegistrador;
                using (var cmdEmpresa = new SqlCommand(
                    "SELECT id_empresa_fk FROM gnl_usuario WHERE id = @idRegistrador", cnn, tx))
                {
                    cmdEmpresa.Parameters.AddWithValue("@idRegistrador", idRegistrador);
                    var val = cmdEmpresa.ExecuteScalar();
                    idEmpresaRegistrador = val == null || val == DBNull.Value ? null : Convert.ToInt32(val);
                }

                if (idEmpresaRegistrador == null)
                    throw new InvalidOperationException("El registrador no tiene una empresa asignada; contacte al administrador.");

                using var cmdNextCons = new SqlCommand(
                    "SELECT ISNULL(MAX(consecutivo), 0) + 1 FROM form_formulario_usuario WITH (UPDLOCK, HOLDLOCK)", cnn, tx);
                int consecutivo = Convert.ToInt32(cmdNextCons.ExecuteScalar());

                int idFormularioUsuario;
                using (var cmdHead = new SqlCommand(@"
                    INSERT INTO form_formulario_usuario
                        (idform_formulario_fk, usuario, idgnl_estado_fk, consecutivo, origen_canal, id_registrador_fk, id_empresa_fk, fecha_creacion)
                    OUTPUT INSERTED.id
                    VALUES (@idFormulario, @usuario, 2, @consecutivo, @origenCanal, @idRegistrador, @idEmpresa, GETDATE())", cnn, tx))
                {
                    cmdHead.Parameters.AddWithValue("@idFormulario", request.IdFormulario);
                    cmdHead.Parameters.AddWithValue("@usuario", correo);
                    cmdHead.Parameters.AddWithValue("@consecutivo", consecutivo);
                    cmdHead.Parameters.AddWithValue("@origenCanal", (object?)request.CanalOrigen ?? DBNull.Value);
                    cmdHead.Parameters.AddWithValue("@idRegistrador", idRegistrador);
                    cmdHead.Parameters.AddWithValue("@idEmpresa", idEmpresaRegistrador.Value);
                    idFormularioUsuario = Convert.ToInt32(cmdHead.ExecuteScalar());
                }

                foreach (var resp in request.Respuestas)
                {
                    using var cmdResp = new SqlCommand(@"
                        INSERT INTO form_campo_respuesta
                            (idform_formulario_fk, idform_campo_fk, idform_formulario_usuario_fk,
                             id_opcion_respuesta, texto_respuesta, usuario, fecha_creacion)
                        VALUES (@idForm, @idCampo, @idFormUser, @idOpcion, @texto, @usuario, GETDATE())", cnn, tx);
                    cmdResp.Parameters.AddWithValue("@idForm", request.IdFormulario);
                    cmdResp.Parameters.AddWithValue("@idCampo", resp.IdCampo);
                    cmdResp.Parameters.AddWithValue("@idFormUser", idFormularioUsuario);
                    cmdResp.Parameters.AddWithValue("@idOpcion", (object?)resp.IdOpcion ?? DBNull.Value);
                    cmdResp.Parameters.AddWithValue("@texto", (object?)resp.TextoRespuesta ?? DBNull.Value);
                    cmdResp.Parameters.AddWithValue("@usuario", correo);
                    cmdResp.ExecuteNonQuery();
                }

                foreach (var resp in request.RespuestasTabla)
                {
                    using var cmdTabla = new SqlCommand(@"
                        INSERT INTO form_campo_tabla_respuesta
                            (idform_formulario_fk, idform_formulario_usuario_fk, id_campo_tabla,
                             idform_campo_fk, texto_respuesta, usuario, id_grupo, fecha_creacion)
                        VALUES (@idForm, @idFormUser, @idCampoTabla, @idCampoHijo, @texto, @usuario, @idGrupo, GETDATE())", cnn, tx);
                    cmdTabla.Parameters.AddWithValue("@idForm", request.IdFormulario);
                    cmdTabla.Parameters.AddWithValue("@idFormUser", idFormularioUsuario);
                    cmdTabla.Parameters.AddWithValue("@idCampoTabla", resp.IdCampoTabla);
                    cmdTabla.Parameters.AddWithValue("@idCampoHijo", resp.IdCampoHijo);
                    cmdTabla.Parameters.AddWithValue("@texto", (object?)resp.TextoRespuesta ?? DBNull.Value);
                    cmdTabla.Parameters.AddWithValue("@usuario", correo);
                    cmdTabla.Parameters.AddWithValue("@idGrupo", resp.IdGrupo);
                    cmdTabla.ExecuteNonQuery();
                }

                tx.Commit();

                _ = _notif.InsertarAAdmins(
                    "Se registró una nueva denuncia a través de un canal externo.", "info", idFormularioUsuario, idEmpresaRegistrador);
                var correosConsultores = _notif.ObtenerCorreosConsultoresEmpresa(idEmpresaRegistrador).GetAwaiter().GetResult();

                return new MontarDenunciaResponseDto
                {
                    Exito = true,
                    Mensaje = "Denuncia registrada exitosamente.",
                    IdDenuncia = idFormularioUsuario,
                    Consecutivo = consecutivo,
                    UsuarioCreado = usuarioCreado,
                    PasswordGeneradaInterna = passwordGenerada,
                    CorreosConsultores = correosConsultores
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error in MontarDenuncia");
                return new MontarDenunciaResponseDto { Exito = false, Mensaje = "Error: " + ex.Message };
            }
        });
    }

    private static string GenerarPassword()
    {
        const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*-_=+";
        var random = new Random();
        return new string(Enumerable.Repeat(caracteres, 9).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
