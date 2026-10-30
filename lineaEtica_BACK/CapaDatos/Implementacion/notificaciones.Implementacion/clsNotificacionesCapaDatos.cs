using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDatos.util;
using CapaDTO.Notificaciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CapaDatos.Implementacion.notificaciones.Implementacion;

public class clsNotificacionesCapaDatos : INotificacionesCapaDatos
{
    private readonly string _connStr;

    public clsNotificacionesCapaDatos(IConfiguration config)
    {
        _connStr = config.GetConnectionString("SQLConnectionString")!;
    }

    public async Task<List<NotificacionDto>> ObtenerNoLeidas(int idUsuario)
    {
        var lista = new List<NotificacionDto>();
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();
        await using var cmd = new SqlCommand(@"
            SELECT id, mensaje, tipo, leida, fecha, id_formulario_usuario
            FROM tbl_Notificaciones
            WHERE id_usuario = @u AND leida = 0
            ORDER BY fecha DESC", cnn);
        cmd.Parameters.AddWithValue("@u", idUsuario);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
        {
            lista.Add(new NotificacionDto
            {
                Id                  = r.GetInt32(0),
                Mensaje             = r.GetString(1),
                Tipo                = r.GetString(2),
                Leida               = r.GetBoolean(3),
                Fecha               = r.GetDateTime(4),
                IdFormularioUsuario = r.IsDBNull(5) ? null : r.GetInt32(5)
            });
        }
        return lista;
    }

    public async Task MarcarLeidas(int idUsuario)
    {
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();
        await using var cmd = new SqlCommand(
            "UPDATE tbl_Notificaciones SET leida = 1 WHERE id_usuario = @u AND leida = 0", cnn);
        cmd.Parameters.AddWithValue("@u", idUsuario);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task MarcarLeida(int id, int idUsuario)
    {
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();
        await using var cmd = new SqlCommand(
            "UPDATE tbl_Notificaciones SET leida = 1 WHERE id = @id AND id_usuario = @u", cnn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@u",  idUsuario);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task Insertar(int idUsuario, string mensaje, string tipo, int? idFormularioUsuario)
    {
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();
        await using var cmd = new SqlCommand(@"
            INSERT INTO tbl_Notificaciones (id_usuario, mensaje, tipo, id_formulario_usuario)
            VALUES (@u, @msg, @tipo, @idForm)", cnn);
        cmd.Parameters.AddWithValue("@u",      idUsuario);
        cmd.Parameters.AddWithValue("@msg",    mensaje);
        cmd.Parameters.AddWithValue("@tipo",   tipo);
        cmd.Parameters.AddWithValue("@idForm", (object?)idFormularioUsuario ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task InsertarAAdmins(string mensaje, string tipo, int? idFormularioUsuario, int? idEmpresa)
    {
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();

        await using var cmdIds = new SqlCommand(
            "SELECT id FROM gnl_usuario WHERE es_admin = 1 AND activo = 1 AND (@idEmpresa IS NULL OR id_empresa_fk = @idEmpresa)", cnn);
        cmdIds.Parameters.AddWithValue("@idEmpresa", (object?)idEmpresa ?? DBNull.Value);
        var ids = new List<int>();
        await using (var r = await cmdIds.ExecuteReaderAsync())
            while (await r.ReadAsync()) ids.Add(r.GetInt32(0));

        foreach (var id in ids)
            await Insertar(id, mensaje, tipo, idFormularioUsuario);
    }

    public async Task<List<string>> ObtenerCorreosConsultoresEmpresa(int? idEmpresa)
    {
        var correos = new List<string>();
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();

        await using var cmd = new SqlCommand(@"
            SELECT correo FROM gnl_usuario
            WHERE es_admin = 1 AND activo = 1 AND correo IS NOT NULL AND correo <> ''
              AND (@idEmpresa IS NULL OR id_empresa_fk = @idEmpresa)", cnn);
        cmd.Parameters.AddWithValue("@idEmpresa", (object?)idEmpresa ?? DBNull.Value);
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) correos.Add(r.GetString(0));
        return correos;
    }

    public async Task InsertarADuenio(int idFormularioUsuario, string mensaje, string tipo)
    {
        await using var cnn = new SqlConnection(_connStr);
        await cnn.OpenAsync();

        await using var cmdUser = new SqlCommand(
            "SELECT usuario FROM form_formulario_usuario WHERE id = @id", cnn);
        cmdUser.Parameters.AddWithValue("@id", idFormularioUsuario);
        var email = (await cmdUser.ExecuteScalarAsync())?.ToString();

        if (string.IsNullOrWhiteSpace(email)) return;

        await using var cmdId = new SqlCommand(
            "SELECT id FROM gnl_usuario WHERE usuario = @email AND activo = 1", cnn);
        cmdId.Parameters.AddWithValue("@email", email);
        var idObj = await cmdId.ExecuteScalarAsync();

        if (idObj == null || idObj == DBNull.Value) return;
        await Insertar(Convert.ToInt32(idObj), mensaje, tipo, idFormularioUsuario);
    }
}
