using CapaDatos.Interfaz.denuncia.interfaz;
using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDatos.util;
using CapaDTO.Denuncia;
using CapaDTO.Notificaciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CapaDatos.Implementacion.denuncia.Implementacion;

public class clsDenunciaCapaDatos : IDenunciaCapaDatos
{
    private readonly cDataBase _db;
    private readonly IConfiguration _config;
    private readonly ILogger<clsDenunciaCapaDatos> _logger;
    private readonly INotificacionesCapaDatos _notif;

    public clsDenunciaCapaDatos(
        IConfiguration config,
        ILogger<clsDenunciaCapaDatos> logger,
        INotificacionesCapaDatos notif)
    {
        _config = config;
        _logger = logger;
        _notif  = notif;
        _db = new cDataBase(config);
    }

    public async Task<FormularioDto?> GetFormulario(int idFormulario)
    {
        return await Task.Run(() =>
        {
            try
            {
                _db.conectar();

                string sql = @"
                    SELECT f.id AS form_id, f.nombre AS form_nombre,
                           s.id AS sec_id, s.nombre AS sec_nombre, s.descripcion AS sec_desc,
                           s.mostrar_nombre_seccion, s.orden AS sec_orden,
                           c.id AS campo_id, c.nombre AS campo_nombre, c.descripcion AS campo_desc,
                           c.ayuda, c.obligatorio, c.orden AS campo_orden,
                           c.idform_tipo_campo_fk AS tipo_campo,
                           c.idgnl_tipo_parametro_fk AS tipo_parametro,
                           c.idgnl_tipo_parametro_hijo_fk AS tipo_parametro_hijo,
                           c.col, c.id_campo, c.cant_adjuntos
                    FROM form_formulario f
                    INNER JOIN form_seccion s ON s.idform_formulario_fk = f.id AND s.activo = 1
                    LEFT JOIN form_campo c ON c.idform_seccion_fk = s.id AND c.activo = 1
                    WHERE f.id = " + idFormulario + @" AND f.activo = 1
                    ORDER BY s.orden, c.orden";

                DataTable dt = _db.ejecutarConsulta(sql);

                if (dt.Rows.Count == 0) return null;

                var formulario = new FormularioDto
                {
                    Id = idFormulario,
                    Nombre = dt.Rows[0]["form_nombre"].ToString()!
                };

                var seccionesDict = new Dictionary<int, SeccionDto>();
                var camposDict = new Dictionary<int, CampoDto>();

                foreach (DataRow row in dt.Rows)
                {
                    if (row["sec_id"] == DBNull.Value) continue;

                    int secId = Convert.ToInt32(row["sec_id"]);
                    if (!seccionesDict.TryGetValue(secId, out var seccion))
                    {
                        seccion = new SeccionDto
                        {
                            Id = secId,
                            Nombre = row["sec_nombre"]?.ToString() ?? string.Empty,
                            Descripcion = row["sec_desc"]?.ToString(),
                            MostrarNombre = row["mostrar_nombre_seccion"] != DBNull.Value && Convert.ToInt32(row["mostrar_nombre_seccion"]) == 1,
                            Orden = row["sec_orden"] != DBNull.Value ? Convert.ToInt32(row["sec_orden"]) : 0
                        };
                        seccionesDict[secId] = seccion;
                        formulario.Secciones.Add(seccion);
                    }

                    if (row["campo_id"] == DBNull.Value) continue;

                    int campoId = Convert.ToInt32(row["campo_id"]);
                    int idCampo = row["id_campo"] != DBNull.Value ? Convert.ToInt32(row["id_campo"]) : 0;

                    var campo = new CampoDto
                    {
                        Id = campoId,
                        Nombre = row["campo_nombre"]?.ToString(),
                        Descripcion = row["campo_desc"]?.ToString(),
                        Ayuda = row["ayuda"]?.ToString(),
                        Obligatorio = row["obligatorio"] != DBNull.Value && Convert.ToInt32(row["obligatorio"]) == 1,
                        Orden = row["campo_orden"] != DBNull.Value ? Convert.ToInt32(row["campo_orden"]) : 0,
                        TipoCampo = row["tipo_campo"] != DBNull.Value ? Convert.ToInt32(row["tipo_campo"]) : 0,
                        TipoParametro = row["tipo_parametro"] != DBNull.Value ? Convert.ToInt32(row["tipo_parametro"]) : 0,
                        TipoParametroHijo = row["tipo_parametro_hijo"] != DBNull.Value ? Convert.ToInt32(row["tipo_parametro_hijo"]) : 0,
                        Col = row["col"] != DBNull.Value ? Convert.ToInt32(row["col"]) : 12,
                        IdCampo = idCampo,
                        CantAdjuntos = row["cant_adjuntos"] != DBNull.Value ? Convert.ToInt32(row["cant_adjuntos"]) : 0
                    };

                    camposDict[campoId] = campo;

                    if (idCampo == 0)
                        seccion.Campos.Add(campo);
                }

                foreach (var (campoId, campo) in camposDict)
                {
                    if (campo.IdCampo != 0 && camposDict.TryGetValue(campo.IdCampo, out var padre))
                        padre.CamposHijo.Add(campo);
                }

                string sqlCondiciones = @"
                    SELECT id, id_campo_origen, condicion, valor_condicion,
                           id_seccion_destino, id_campo_destino, accion
                    FROM form_condicion
                    WHERE idform_formulario_fk = " + idFormulario + @" AND activo = 1
                    ORDER BY id";

                DataTable dtCond = _db.ejecutarConsulta(sqlCondiciones);
                foreach (DataRow row in dtCond.Rows)
                {
                    formulario.Condiciones.Add(new CondicionDto
                    {
                        Id = Convert.ToInt32(row["id"]),
                        IdCampoOrigen = row["id_campo_origen"] != DBNull.Value ? Convert.ToInt32(row["id_campo_origen"]) : null,
                        Condicion = row["condicion"]?.ToString() ?? string.Empty,
                        ValorCondicion = row["valor_condicion"]?.ToString(),
                        IdSeccionDestino = Convert.ToInt32(row["id_seccion_destino"]),
                        IdCampoDestino = row["id_campo_destino"] != DBNull.Value ? Convert.ToInt32(row["id_campo_destino"]) : null,
                        Accion = row["accion"]?.ToString() ?? string.Empty
                    });
                }

                return formulario;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting formulario {IdFormulario}", idFormulario);
                throw;
            }
            finally
            {
                _db.desconectar();
            }
        });
    }

    public async Task<List<ParametroDto>> GetParametros(int idTipoParametro)
    {
        return await Task.Run(() =>
        {
            try
            {
                _db.conectar();
                string sql = $@"
                    SELECT id, nombre, codigo, ISNULL(depende_de,0) AS depende_de
                    FROM gnl_parametro
                    WHERE idgnl_tipo_parametro_fk = {idTipoParametro} AND activo = 1
                    ORDER BY nombre";

                DataTable dt = _db.ejecutarConsulta(sql);
                var lista = new List<ParametroDto>();

                foreach (DataRow row in dt.Rows)
                {
                    lista.Add(new ParametroDto
                    {
                        Id = Convert.ToInt32(row["id"]),
                        Nombre = row["nombre"]?.ToString() ?? string.Empty,
                        Codigo = row["codigo"]?.ToString(),
                        DependeDe = row["depende_de"] != DBNull.Value ? Convert.ToInt32(row["depende_de"]) : 0
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parametros for tipo {IdTipo}", idTipoParametro);
                throw;
            }
            finally
            {
                _db.desconectar();
            }
        });
    }

    public async Task<DenunciaResponseDto> CrearDenuncia(DenunciaRequestDto request, string usuario)
        => await UpsertDenuncia(request, usuario, estado: 2);

    public async Task<DenunciaResponseDto> GuardarDenuncia(DenunciaRequestDto request, string usuario)
        => await UpsertDenuncia(request, usuario, estado: 1);

    private async Task<DenunciaResponseDto> UpsertDenuncia(DenunciaRequestDto request, string usuario, int estado)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();

            try
            {
                int idFormularioUsuario;
                int consecutivo = 0;

                if (request.IdDenuncia.HasValue)
                {
                    idFormularioUsuario = request.IdDenuncia.Value;

                    int estadoActualInt = -1;
                    string? ownerUsuario = null;
                    using (var cmdCheck = new SqlCommand(
                        "SELECT idgnl_estado_fk, usuario FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", idFormularioUsuario);
                        using var rdr = cmdCheck.ExecuteReader();
                        if (rdr.Read())
                        {
                            estadoActualInt = rdr.IsDBNull(0) ? -1 : rdr.GetInt32(0);
                            ownerUsuario    = rdr.IsDBNull(1) ? null : rdr.GetString(1);
                        }
                    }

                    if (ownerUsuario == null)
                        throw new Exception($"Denuncia no encontrada (id={idFormularioUsuario}).");

                    bool propietarioValido = ownerUsuario == usuario
                                         || ownerUsuario == "ANONIMO"
                                         || ownerUsuario == "DESCONOCIDO";
                    if (!propietarioValido)
                        throw new Exception($"No tiene permiso para modificar esta denuncia.");

                    if (estadoActualInt == 7 && estado == 2) estado = 3;
                    if (estadoActualInt == 7 && estado == 1) estado = 7;

                    using var cmdUpd = new SqlCommand(@"
                        UPDATE form_formulario_usuario
                        SET idgnl_estado_fk = @estado, fecha_actualizacion = GETDATE()
                        WHERE id = @id", cnn, tx);
                    cmdUpd.Parameters.AddWithValue("@estado", estado);
                    cmdUpd.Parameters.AddWithValue("@id", idFormularioUsuario);
                    int rowsUpdated = cmdUpd.ExecuteNonQuery();
                    if (rowsUpdated == 0)
                        throw new Exception($"No se pudo actualizar la denuncia (id={idFormularioUsuario}).");

                    if (estadoActualInt == 7)
                    {
                        using var cmdDelRev = new SqlCommand(
                            "DELETE FROM form_revision_campo WHERE id_formulario_usuario = @id", cnn, tx);
                        cmdDelRev.Parameters.AddWithValue("@id", idFormularioUsuario);
                        cmdDelRev.ExecuteNonQuery();
                    }

                    using var cmdDelR = new SqlCommand(
                        "DELETE FROM form_campo_respuesta WHERE idform_formulario_usuario_fk = @id", cnn, tx);
                    cmdDelR.Parameters.AddWithValue("@id", idFormularioUsuario);
                    cmdDelR.ExecuteNonQuery();

                    using var cmdDelT = new SqlCommand(
                        "DELETE FROM form_campo_tabla_respuesta WHERE idform_formulario_usuario_fk = @id", cnn, tx);
                    cmdDelT.Parameters.AddWithValue("@id", idFormularioUsuario);
                    cmdDelT.ExecuteNonQuery();

                    using var cmdCons = new SqlCommand(
                        "SELECT consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx);
                    cmdCons.Parameters.AddWithValue("@id", idFormularioUsuario);
                    var consVal = cmdCons.ExecuteScalar();
                    consecutivo = consVal != null && consVal != DBNull.Value ? Convert.ToInt32(consVal) : 0;

                    if ((estado == 2 || estado == 3) && consecutivo == 0)
                    {
                        using var cmdNextCons = new SqlCommand(
                            "SELECT ISNULL(MAX(consecutivo), 0) + 1 FROM form_formulario_usuario WITH (UPDLOCK, HOLDLOCK)", cnn, tx);
                        consecutivo = Convert.ToInt32(cmdNextCons.ExecuteScalar());
                        using var cmdSetCons = new SqlCommand(
                            "UPDATE form_formulario_usuario SET consecutivo = @cons WHERE id = @id", cnn, tx);
                        cmdSetCons.Parameters.AddWithValue("@cons", consecutivo);
                        cmdSetCons.Parameters.AddWithValue("@id", idFormularioUsuario);
                        cmdSetCons.ExecuteNonQuery();
                    }
                }
                else
                {
                    int? nuevoConsecutivo = null;
                    if (estado == 2)
                    {
                        using var cmdNextCons = new SqlCommand(
                            "SELECT ISNULL(MAX(consecutivo), 0) + 1 FROM form_formulario_usuario WITH (UPDLOCK, HOLDLOCK)", cnn, tx);
                        nuevoConsecutivo = Convert.ToInt32(cmdNextCons.ExecuteScalar());
                    }

                    string colsCons = nuevoConsecutivo.HasValue ? ", consecutivo" : "";
                    string valsCons = nuevoConsecutivo.HasValue ? ", @consecutivo" : "";
                    using var cmdIns = new SqlCommand($@"
                        INSERT INTO form_formulario_usuario
                            (idform_formulario_fk, usuario, idgnl_estado_fk{colsCons}, origen_canal, id_empresa_fk, fecha_creacion)
                        OUTPUT INSERTED.id, INSERTED.consecutivo
                        VALUES (@idFormulario, @usuario, @estado{valsCons}, 'Plataforma', @idEmpresa, GETDATE())", cnn, tx);
                    cmdIns.Parameters.AddWithValue("@idFormulario", request.IdFormulario);
                    cmdIns.Parameters.AddWithValue("@usuario", usuario);
                    cmdIns.Parameters.AddWithValue("@estado", estado);
                    cmdIns.Parameters.AddWithValue("@idEmpresa", request.IdEmpresa);
                    if (nuevoConsecutivo.HasValue)
                        cmdIns.Parameters.AddWithValue("@consecutivo", nuevoConsecutivo.Value);

                    using var reader = cmdIns.ExecuteReader();
                    if (reader.Read())
                    {
                        idFormularioUsuario = reader.GetInt32(0);
                        consecutivo = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    }
                    else
                    {
                        throw new Exception("No se pudo crear el encabezado de la denuncia.");
                    }
                    reader.Close();
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
                    cmdResp.Parameters.AddWithValue("@usuario", usuario);
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
                    cmdTabla.Parameters.AddWithValue("@usuario", usuario);
                    cmdTabla.Parameters.AddWithValue("@idGrupo", resp.IdGrupo);
                    cmdTabla.ExecuteNonQuery();
                }

                tx.Commit();

                var correosNotificar = new List<string>();
                if (estado == 2 || estado == 3)
                {
                    _ = _notif.InsertarAAdmins(
                        estado == 2 ? "Se recibió una nueva denuncia para revisar." : "Una denuncia devuelta fue corregida y está pendiente de revisión.",
                        "info", idFormularioUsuario, request.IdEmpresa);
                    correosNotificar = _notif.ObtenerCorreosConsultoresEmpresa(request.IdEmpresa).GetAwaiter().GetResult();
                }

                string msg = estado == 7 ? "Avance guardado. La denuncia sigue pendiente de corrección."
                           : estado == 1 ? "Borrador guardado correctamente."
                           : "Denuncia enviada exitosamente.";
                return new DenunciaResponseDto
                {
                    Exito = true,
                    Mensaje = msg,
                    IdDenuncia = idFormularioUsuario,
                    Consecutivo = consecutivo,
                    CorreosNotificar = correosNotificar,
                    EsCorreccion = estado == 3
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error in UpsertDenuncia (estado={Estado})", estado);
                return new DenunciaResponseDto { Exito = false, Mensaje = "Error: " + ex.Message };
            }
        });
    }

    public async Task<BorradorDto?> GetBorrador(int idDenuncia, string usuario)
    {
        return await Task.Run(() =>
        {
            try
            {
                string connStr = _config.GetConnectionString("SQLConnectionString")!;
                using var cnn = new SqlConnection(connStr);
                cnn.Open();

                using var cmdHead = new SqlCommand(@"
                    SELECT id, idform_formulario_fk, idgnl_estado_fk
                    FROM form_formulario_usuario
                    WHERE id = @id AND (usuario = @usuario OR usuario = 'ANONIMO')", cnn);
                cmdHead.Parameters.AddWithValue("@id", idDenuncia);
                cmdHead.Parameters.AddWithValue("@usuario", usuario);

                BorradorDto? borrador = null;
                using (var r = cmdHead.ExecuteReader())
                {
                    if (r.Read())
                    {
                        borrador = new BorradorDto
                        {
                            Id = r.GetInt32(0),
                            IdFormulario = r.GetInt32(1),
                            IdEstado = r.IsDBNull(2) ? -1 : r.GetInt32(2)
                        };
                    }
                }

                if (borrador == null) return null;

                using var cmdResp = new SqlCommand(@"
                    SELECT idform_campo_fk AS IdCampo, id_opcion_respuesta AS IdOpcion, texto_respuesta AS TextoRespuesta
                    FROM form_campo_respuesta
                    WHERE idform_formulario_usuario_fk = @id", cnn);
                cmdResp.Parameters.AddWithValue("@id", idDenuncia);

                using (var r = cmdResp.ExecuteReader())
                {
                    while (r.Read())
                    {
                        borrador.Respuestas.Add(new RespuestaCampoDto
                        {
                            IdCampo = r.GetInt32(r.GetOrdinal("IdCampo")),
                            IdOpcion = r.IsDBNull(r.GetOrdinal("IdOpcion")) ? null : r.GetInt32(r.GetOrdinal("IdOpcion")),
                            TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                        });
                    }
                }

                using var cmdTabla = new SqlCommand(@"
                    SELECT id_campo_tabla AS IdCampoTabla, id_grupo AS IdGrupo,
                           idform_campo_fk AS IdCampoHijo, texto_respuesta AS TextoRespuesta
                    FROM form_campo_tabla_respuesta
                    WHERE idform_formulario_usuario_fk = @id
                    ORDER BY id_campo_tabla, id_grupo, idform_campo_fk", cnn);
                cmdTabla.Parameters.AddWithValue("@id", idDenuncia);

                using (var r = cmdTabla.ExecuteReader())
                {
                    while (r.Read())
                    {
                        borrador.RespuestasTabla.Add(new RespuestaTablaDto
                        {
                            IdCampoTabla = r.GetInt32(r.GetOrdinal("IdCampoTabla")),
                            IdGrupo = r.GetInt32(r.GetOrdinal("IdGrupo")),
                            IdCampoHijo = r.GetInt32(r.GetOrdinal("IdCampoHijo")),
                            TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                        });
                    }
                }

                if (borrador.IdEstado == 7)
                {
                    using var cmdRechazados = new SqlCommand(@"
                        SELECT id_seccion, id_campo, id_fila
                        FROM form_revision_campo
                        WHERE id_formulario_usuario = @id", cnn);
                    cmdRechazados.Parameters.AddWithValue("@id", idDenuncia);
                    using (var r = cmdRechazados.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            borrador.CamposRechazados.Add(new CampoRechazadoDto
                            {
                                IdSeccion = r.GetInt32(0),
                                IdCampo   = r.GetInt32(1),
                                IdFila    = r.IsDBNull(2) ? null : r.GetInt32(2)
                            });
                        }
                    }

                    using var cmdComentario = new SqlCommand(@"
                        SELECT TOP 1 comentario FROM form_historial_estado
                        WHERE id_formulario_usuario = @id AND estado_nuevo = 7
                        ORDER BY fecha DESC", cnn);
                    cmdComentario.Parameters.AddWithValue("@id", idDenuncia);
                    var comentario = cmdComentario.ExecuteScalar();
                    borrador.ComentarioAdmin = comentario == DBNull.Value ? null : (string?)comentario;
                }

                return borrador;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting borrador {IdDenuncia}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<List<MisDenunciaDto>> GetMisDenuncias(string usuario)
    {
        return await Task.Run(() =>
        {
            try
            {
                string connStr = _config.GetConnectionString("SQLConnectionString")!;
                using var cnn = new SqlConnection(connStr);
                cnn.Open();

                string sql = @"
                    SELECT ffu.id, ffu.consecutivo, ff.nombre AS nombre_formulario,
                           ISNULL(ge.nombre, 'Creado') AS estado,
                           ffu.fecha_creacion,
                           CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo
                    FROM form_formulario_usuario ffu
                    INNER JOIN form_formulario ff ON ffu.idform_formulario_fk = ff.id
                    LEFT JOIN gnl_estado ge ON ffu.idgnl_estado_fk = ge.id
                    WHERE ffu.usuario = @Usuario
                    ORDER BY ffu.fecha_creacion DESC";

                using var cmd = new SqlCommand(sql, cnn);
                cmd.Parameters.AddWithValue("@Usuario", usuario);

                using var reader = cmd.ExecuteReader();
                var lista = new List<MisDenunciaDto>();

                while (reader.Read())
                {
                    lista.Add(new MisDenunciaDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Consecutivo = reader.IsDBNull(reader.GetOrdinal("consecutivo")) ? null : reader.GetInt32(reader.GetOrdinal("consecutivo")),
                        NombreFormulario = reader["nombre_formulario"]?.ToString() ?? string.Empty,
                        Estado = reader["estado"]?.ToString() ?? "Creado",
                        FechaCreacion = reader.IsDBNull(reader.GetOrdinal("fecha_creacion")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                        EsAnonimo = Convert.ToInt32(reader["es_anonimo"]) == 1
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting denuncias for user {Usuario}", usuario);
                throw;
            }
        });
    }

    public async Task<EstadisticasDto> GetEstadisticas(int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                string connStr = _config.GetConnectionString("SQLConnectionString")!;
                using var cnn = new SqlConnection(connStr);
                cnn.Open();

                var dto = new EstadisticasDto();

                using (var cmd = new SqlCommand(@"
                    SELECT
                        COUNT(*) AS total,
                        SUM(CASE WHEN idgnl_estado_fk IN (2,3,7) THEN 1 ELSE 0 END) AS pendientes,
                        SUM(CASE WHEN idgnl_estado_fk = 4        THEN 1 ELSE 0 END) AS aprobadas,
                        SUM(CASE WHEN idgnl_estado_fk = 5        THEN 1 ELSE 0 END) AS rechazadas,
                        SUM(CASE WHEN idgnl_estado_fk = 7        THEN 1 ELSE 0 END) AS devueltas,
                        SUM(CASE WHEN idgnl_estado_fk = 8        THEN 1 ELSE 0 END) AS en_investigacion,
                        SUM(CASE WHEN idgnl_estado_fk = 9        THEN 1 ELSE 0 END) AS cerradas,
                        SUM(CASE WHEN idform_formulario_fk = 1   THEN 1 ELSE 0 END) AS anonimas,
                        SUM(CASE WHEN idform_formulario_fk != 1  THEN 1 ELSE 0 END) AS no_anonimas
                    FROM form_formulario_usuario
                    WHERE idgnl_estado_fk IN (2, 3, 4, 5, 7, 8, 9)
                      AND (@idEmpresaFiltro IS NULL OR id_empresa_fk = @idEmpresaFiltro)", cnn))
                {
                    cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        dto.Total           = r.IsDBNull(0) ? 0 : r.GetInt32(0);
                        dto.Pendientes      = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                        dto.Aprobadas       = r.IsDBNull(2) ? 0 : r.GetInt32(2);
                        dto.Rechazadas      = r.IsDBNull(3) ? 0 : r.GetInt32(3);
                        dto.Devueltas       = r.IsDBNull(4) ? 0 : r.GetInt32(4);
                        dto.EnInvestigacion = r.IsDBNull(5) ? 0 : r.GetInt32(5);
                        dto.Cerradas        = r.IsDBNull(6) ? 0 : r.GetInt32(6);
                        dto.Anonimas        = r.IsDBNull(7) ? 0 : r.GetInt32(7);
                        dto.NoAnonimas      = r.IsDBNull(8) ? 0 : r.GetInt32(8);
                    }
                }

                using (var cmd = new SqlCommand(@"
                    SELECT ffu.idgnl_estado_fk,
                           ISNULL(ge.nombre, 'Desconocido') AS estado,
                           COUNT(*) AS cantidad
                    FROM form_formulario_usuario ffu
                    LEFT JOIN gnl_estado ge ON ffu.idgnl_estado_fk = ge.id
                    WHERE ffu.idgnl_estado_fk IN (2, 3, 4, 5, 7, 8, 9)
                      AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                    GROUP BY ffu.idgnl_estado_fk, ge.nombre
                    ORDER BY ffu.idgnl_estado_fk", cnn))
                {
                    cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        dto.PorEstado.Add(new EstEstadoDto
                        {
                            IdEstado = r.GetInt32(0),
                            Estado   = r.GetString(1),
                            Cantidad = r.GetInt32(2)
                        });
                }

                using (var cmd = new SqlCommand(@"
                    SELECT FORMAT(fecha_creacion, 'yyyy-MM') AS mes,
                           COUNT(*) AS cantidad
                    FROM form_formulario_usuario
                    WHERE idgnl_estado_fk IN (2, 3, 4, 5, 7, 8, 9)
                      AND fecha_creacion >= DATEADD(MONTH, -5,
                            DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                      AND (@idEmpresaFiltro IS NULL OR id_empresa_fk = @idEmpresaFiltro)
                    GROUP BY FORMAT(fecha_creacion, 'yyyy-MM')
                    ORDER BY mes", cnn))
                {
                    cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        dto.PorMes.Add(new EstMesDto
                        {
                            Mes      = r.GetString(0),
                            Cantidad = r.GetInt32(1)
                        });
                }

                using (var cmd = new SqlCommand(@"
                    SELECT ffu.id_empresa_fk, ISNULL(e.nombre, 'Sin empresa') AS nombre_empresa,
                           COUNT(*) AS cantidad
                    FROM form_formulario_usuario ffu
                    LEFT JOIN gnl_empresa e ON e.id = ffu.id_empresa_fk
                    WHERE ffu.idgnl_estado_fk IN (2, 3, 4, 5, 7, 8, 9)
                      AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                    GROUP BY ffu.id_empresa_fk, e.nombre
                    ORDER BY nombre_empresa", cnn))
                {
                    cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        dto.PorEmpresa.Add(new EstEmpresaDto
                        {
                            IdEmpresa     = r.IsDBNull(0) ? null : r.GetInt32(0),
                            NombreEmpresa = r.GetString(1),
                            Cantidad      = r.GetInt32(2)
                        });
                }

                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(NULLIF(ffu.origen_canal, ''), 'Plataforma') AS canal,
                           COUNT(*) AS cantidad
                    FROM form_formulario_usuario ffu
                    WHERE ffu.idgnl_estado_fk IN (2, 3, 4, 5, 7, 8, 9)
                      AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                    GROUP BY ISNULL(NULLIF(ffu.origen_canal, ''), 'Plataforma')
                    ORDER BY cantidad DESC", cnn))
                {
                    cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
                    using var r = cmd.ExecuteReader();
                    while (r.Read())
                        dto.PorCanal.Add(new EstCanalDto
                        {
                            Canal    = r.GetString(0),
                            Cantidad = r.GetInt32(1)
                        });
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting estadisticas");
                throw;
            }
        });
    }

    public async Task<List<DenunciaAdminResumenDto>> GetTodasDenuncias(int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                string connStr = _config.GetConnectionString("SQLConnectionString")!;
                using var cnn = new SqlConnection(connStr);
                cnn.Open();

                using var cmd = new SqlCommand(@"
                    SELECT ffu.id, ffu.consecutivo, ff.nombre AS nombre_formulario,
                           ISNULL(ge.nombre, 'Creado') AS estado, ffu.idgnl_estado_fk AS id_estado,
                           ffu.fecha_creacion,
                           CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                           ffu.usuario, ffu.id_empresa_fk, e.nombre AS nombre_empresa
                    FROM form_formulario_usuario ffu
                    INNER JOIN form_formulario ff ON ffu.idform_formulario_fk = ff.id
                    LEFT JOIN gnl_estado ge ON ffu.idgnl_estado_fk = ge.id
                    LEFT JOIN gnl_empresa e ON ffu.id_empresa_fk = e.id
                    WHERE ffu.idgnl_estado_fk IN (2, 3, 4, 5, 7, 10)
                      AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                    ORDER BY ffu.id DESC", cnn);
                cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

                using var reader = cmd.ExecuteReader();
                var lista = new List<DenunciaAdminResumenDto>();

                while (reader.Read())
                {
                    bool esAnonimo = Convert.ToInt32(reader["es_anonimo"]) == 1;
                    lista.Add(new DenunciaAdminResumenDto
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Consecutivo = reader.IsDBNull(reader.GetOrdinal("consecutivo")) ? null : reader.GetInt32(reader.GetOrdinal("consecutivo")),
                        NombreFormulario = reader["nombre_formulario"]?.ToString() ?? string.Empty,
                        Estado = reader["estado"]?.ToString() ?? string.Empty,
                        IdEstado = reader.IsDBNull(reader.GetOrdinal("id_estado")) ? 0 : Convert.ToInt32(reader["id_estado"]),
                        FechaCreacion = reader.IsDBNull(reader.GetOrdinal("fecha_creacion")) ? null : reader.GetDateTime(reader.GetOrdinal("fecha_creacion")),
                        EsAnonimo = esAnonimo,
                        UsuarioReportante = esAnonimo ? null : reader["usuario"]?.ToString(),
                        IdEmpresa = reader.IsDBNull(reader.GetOrdinal("id_empresa_fk")) ? null : Convert.ToInt32(reader["id_empresa_fk"]),
                        NombreEmpresa = reader.IsDBNull(reader.GetOrdinal("nombre_empresa")) ? null : reader["nombre_empresa"]?.ToString()
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all denuncias for admin");
                throw;
            }
        });
    }

    public async Task<DenunciaDetalleAdminDto?> GetDenunciaDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                string connStr = _config.GetConnectionString("SQLConnectionString")!;
                using var cnn = new SqlConnection(connStr);
                cnn.Open();

                using var cmdHead = new SqlCommand(@"
                    SELECT ffu.id, ffu.idform_formulario_fk, ffu.idgnl_estado_fk,
                           ISNULL(ge.nombre, 'Creado') AS estado, ffu.fecha_creacion,
                           ffu.consecutivo, ffu.usuario,
                           CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                           ffu.id_empresa_fk, e.nombre AS nombre_empresa
                    FROM form_formulario_usuario ffu
                    LEFT JOIN gnl_estado ge ON ffu.idgnl_estado_fk = ge.id
                    LEFT JOIN gnl_empresa e ON ffu.id_empresa_fk = e.id
                    WHERE ffu.id = @id
                      AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)", cnn);
                cmdHead.Parameters.AddWithValue("@id", idDenuncia);
                cmdHead.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

                DenunciaDetalleAdminDto? detalle = null;
                using (var r = cmdHead.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    bool esAnonimo = Convert.ToInt32(r["es_anonimo"]) == 1;
                    detalle = new DenunciaDetalleAdminDto
                    {
                        Id = Convert.ToInt32(r["id"]),
                        IdFormulario = Convert.ToInt32(r["idform_formulario_fk"]),
                        IdEstado = r.IsDBNull(r.GetOrdinal("idgnl_estado_fk")) ? 0 : Convert.ToInt32(r["idgnl_estado_fk"]),
                        Estado = r["estado"]?.ToString() ?? string.Empty,
                        EsAnonimo = esAnonimo,
                        UsuarioReportante = esAnonimo ? null : r["usuario"]?.ToString(),
                        FechaCreacion = r.IsDBNull(r.GetOrdinal("fecha_creacion")) ? null : r.GetDateTime(r.GetOrdinal("fecha_creacion")),
                        Consecutivo = r.IsDBNull(r.GetOrdinal("consecutivo")) ? null : Convert.ToInt32(r["consecutivo"]),
                        IdEmpresa = r.IsDBNull(r.GetOrdinal("id_empresa_fk")) ? null : Convert.ToInt32(r["id_empresa_fk"]),
                        NombreEmpresa = r.IsDBNull(r.GetOrdinal("nombre_empresa")) ? null : r["nombre_empresa"]?.ToString()
                    };
                }
                if (detalle == null) return null;

                using var cmdResp = new SqlCommand(@"
                    SELECT idform_campo_fk AS IdCampo, id_opcion_respuesta AS IdOpcion, texto_respuesta AS TextoRespuesta
                    FROM form_campo_respuesta WHERE idform_formulario_usuario_fk = @id", cnn);
                cmdResp.Parameters.AddWithValue("@id", idDenuncia);
                using (var r = cmdResp.ExecuteReader())
                {
                    while (r.Read())
                        detalle.Respuestas.Add(new RespuestaCampoDto
                        {
                            IdCampo = r.GetInt32(r.GetOrdinal("IdCampo")),
                            IdOpcion = r.IsDBNull(r.GetOrdinal("IdOpcion")) ? null : r.GetInt32(r.GetOrdinal("IdOpcion")),
                            TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                        });
                }

                using var cmdTabla = new SqlCommand(@"
                    SELECT id_campo_tabla AS IdCampoTabla, id_grupo AS IdGrupo,
                           idform_campo_fk AS IdCampoHijo, texto_respuesta AS TextoRespuesta
                    FROM form_campo_tabla_respuesta WHERE idform_formulario_usuario_fk = @id
                    ORDER BY id_campo_tabla, id_grupo, idform_campo_fk", cnn);
                cmdTabla.Parameters.AddWithValue("@id", idDenuncia);
                using (var r = cmdTabla.ExecuteReader())
                {
                    while (r.Read())
                        detalle.RespuestasTabla.Add(new RespuestaTablaDto
                        {
                            IdCampoTabla = r.GetInt32(r.GetOrdinal("IdCampoTabla")),
                            IdGrupo = r.GetInt32(r.GetOrdinal("IdGrupo")),
                            IdCampoHijo = r.GetInt32(r.GetOrdinal("IdCampoHijo")),
                            TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                        });
                }

                using var cmdRechazados = new SqlCommand(@"
                    SELECT id_seccion, id_campo, id_fila
                    FROM form_revision_campo WHERE id_formulario_usuario = @id", cnn);
                cmdRechazados.Parameters.AddWithValue("@id", idDenuncia);
                using (var r = cmdRechazados.ExecuteReader())
                {
                    while (r.Read())
                        detalle.CamposRechazados.Add(new CampoRechazadoDto
                        {
                            IdSeccion = r.GetInt32(0),
                            IdCampo = r.GetInt32(1),
                            IdFila = r.IsDBNull(2) ? null : r.GetInt32(2)
                        });
                }

                using var cmdComentario = new SqlCommand(@"
                    SELECT TOP 1 comentario FROM form_historial_estado
                    WHERE id_formulario_usuario = @id ORDER BY fecha DESC", cnn);
                cmdComentario.Parameters.AddWithValue("@id", idDenuncia);
                var comentario = cmdComentario.ExecuteScalar();
                detalle.ComentarioAdmin = comentario == DBNull.Value ? null : (string?)comentario;

                return detalle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting denuncia detail for admin {IdDenuncia}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<InfoNotificacionEmailDto> AprobarDenuncia(int idDenuncia, string adminUsuario, string? comentario, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int estadoActual = 0;
                int idInvestigador = 0;
                int? idEmpresaDenuncia = null;
                string? correoDenunciante = null;
                int? consecutivo = null;
                using (var cmdEstado = new SqlCommand(
                    "SELECT idgnl_estado_fk, ISNULL(id_investigador_fk, 0), id_empresa_fk, usuario, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEstado.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEstado.ExecuteReader();
                    if (r.Read())
                    {
                        estadoActual = r.GetInt32(0);
                        idInvestigador = r.GetInt32(1);
                        idEmpresaDenuncia = r.IsDBNull(2) ? null : r.GetInt32(2);
                        correoDenunciante = r.IsDBNull(3) ? null : r.GetString(3);
                        consecutivo = r.IsDBNull(4) ? null : r.GetInt32(4);
                    }
                }

                if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                    throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");

                int nuevoEstado = idInvestigador > 0 ? 8 : 4;

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = @nuevo, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id",    idDenuncia);
                cmdUpd.Parameters.AddWithValue("@nuevo", nuevoEstado);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, @anterior, @nuevo, @comentario, @admin)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id",         idDenuncia);
                cmdHist.Parameters.AddWithValue("@anterior",   estadoActual);
                cmdHist.Parameters.AddWithValue("@nuevo",      nuevoEstado);
                cmdHist.Parameters.AddWithValue("@comentario", (object?)comentario ?? DBNull.Value);
                cmdHist.Parameters.AddWithValue("@admin",      adminUsuario);
                cmdHist.ExecuteNonQuery();

                string? correoInvestigador = null;
                if (idInvestigador > 0)
                {
                    using var cmdInv = new SqlCommand("SELECT correo FROM gnl_usuario WHERE id = @id", cnn, tx);
                    cmdInv.Parameters.AddWithValue("@id", idInvestigador);
                    var val = cmdInv.ExecuteScalar();
                    correoInvestigador = val == null || val == DBNull.Value ? null : val.ToString();
                }

                tx.Commit();

                _ = _notif.InsertarADuenio(idDenuncia,
                    "Tu respuesta fue aceptada y la investigación ha sido retomada.", "success");

                if (idInvestigador > 0)
                    _ = _notif.Insertar(idInvestigador,
                        "El denunciante aportó nueva información. Tu investigación ha sido reactivada.",
                        "info", idDenuncia);

                return new InfoNotificacionEmailDto
                {
                    Consecutivo = consecutivo,
                    CorreoDenunciante = correoDenunciante,
                    CorreoInvestigador = idInvestigador > 0 ? correoInvestigador : null
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error approving denuncia {IdDenuncia}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<InfoNotificacionEmailDto> DevolverDenuncia(int idDenuncia, string adminUsuario, string comentario, List<CampoRechazadoDto> campos, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int? idEmpresaDenuncia = null;
                object? estadoActual;
                string? correoDenunciante = null;
                int? consecutivo = null;
                using (var cmdEstado = new SqlCommand(
                    "SELECT idgnl_estado_fk, id_empresa_fk, usuario, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEstado.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEstado.ExecuteReader();
                    estadoActual = null;
                    if (r.Read())
                    {
                        estadoActual = r.IsDBNull(0) ? null : r.GetInt32(0);
                        idEmpresaDenuncia = r.IsDBNull(1) ? null : r.GetInt32(1);
                        correoDenunciante = r.IsDBNull(2) ? null : r.GetString(2);
                        consecutivo = r.IsDBNull(3) ? null : r.GetInt32(3);
                    }
                }

                if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                    throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 7, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id", idDenuncia);
                cmdUpd.ExecuteNonQuery();

                using var cmdDel = new SqlCommand(
                    "DELETE FROM form_revision_campo WHERE id_formulario_usuario = @id", cnn, tx);
                cmdDel.Parameters.AddWithValue("@id", idDenuncia);
                cmdDel.ExecuteNonQuery();

                foreach (var campo in campos)
                {
                    using var cmdIns = new SqlCommand(@"
                        INSERT INTO form_revision_campo (id_formulario_usuario, id_seccion, id_campo, id_fila)
                        VALUES (@id, @seccion, @campo, @fila)", cnn, tx);
                    cmdIns.Parameters.AddWithValue("@id", idDenuncia);
                    cmdIns.Parameters.AddWithValue("@seccion", campo.IdSeccion);
                    cmdIns.Parameters.AddWithValue("@campo", campo.IdCampo);
                    cmdIns.Parameters.AddWithValue("@fila", (object?)campo.IdFila ?? DBNull.Value);
                    cmdIns.ExecuteNonQuery();
                }

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, @anterior, 7, @comentario, @admin)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id", idDenuncia);
                cmdHist.Parameters.AddWithValue("@anterior", estadoActual ?? (object)DBNull.Value);
                cmdHist.Parameters.AddWithValue("@comentario", comentario);
                cmdHist.Parameters.AddWithValue("@admin", adminUsuario);
                cmdHist.ExecuteNonQuery();

                tx.Commit();

                _ = _notif.InsertarADuenio(idDenuncia,
                    "Su denuncia fue revisada y tiene campos que requieren corrección.", "warning");

                return new InfoNotificacionEmailDto
                {
                    Consecutivo = consecutivo,
                    CorreoDenunciante = correoDenunciante
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error returning denuncia {IdDenuncia}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<AdjuntoDto> RegistrarAdjunto(int idDenuncia, int idCampo, string nombreOriginal, string nombreArchivo, string tipoMime, long tamanoBytes)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO form_campo_adjunto
                    (id_formulario_usuario, id_campo, nombre_original, nombre_archivo, tipo_mime, tamano_bytes)
                OUTPUT INSERTED.id, INSERTED.fecha_subida
                VALUES (@ffu, @campo, @nomOrig, @nomArch, @mime, @tam)", cnn);
            cmd.Parameters.AddWithValue("@ffu",     idDenuncia);
            cmd.Parameters.AddWithValue("@campo",   idCampo);
            cmd.Parameters.AddWithValue("@nomOrig", nombreOriginal);
            cmd.Parameters.AddWithValue("@nomArch", nombreArchivo);
            cmd.Parameters.AddWithValue("@mime",    tipoMime);
            cmd.Parameters.AddWithValue("@tam",     tamanoBytes);

            using var r = cmd.ExecuteReader();
            r.Read();
            return new AdjuntoDto
            {
                Id            = r.GetInt32(0),
                IdCampo       = idCampo,
                NombreOriginal = nombreOriginal,
                TipoMime      = tipoMime,
                TamanoBytes   = tamanoBytes,
                FechaSubida   = r.GetDateTime(1)
            };
        });
    }

    public async Task<List<AdjuntoDto>> GetAdjuntos(int idDenuncia, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT a.id, a.id_campo, a.nombre_original, a.tipo_mime, a.tamano_bytes, a.fecha_subida
                FROM form_campo_adjunto a
                INNER JOIN form_formulario_usuario ffu ON ffu.id = a.id_formulario_usuario
                WHERE a.id_formulario_usuario = @id
                  AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                ORDER BY a.id_campo, a.fecha_subida", cnn);
            cmd.Parameters.AddWithValue("@id", idDenuncia);
            cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

            using var r = cmd.ExecuteReader();
            var lista = new List<AdjuntoDto>();
            while (r.Read())
                lista.Add(new AdjuntoDto
                {
                    Id             = r.GetInt32(0),
                    IdCampo        = r.GetInt32(1),
                    NombreOriginal = r.GetString(2),
                    TipoMime       = r.GetString(3),
                    TamanoBytes    = r.GetInt64(4),
                    FechaSubida    = r.GetDateTime(5)
                });
            return lista;
        });
    }

    public async Task<AdjuntoDetalleInterno?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT a.id, a.id_campo, a.id_formulario_usuario,
                       ISNULL(ffu.usuario,'') AS usuario,
                       a.nombre_original, a.nombre_archivo, a.tipo_mime, a.tamano_bytes
                FROM form_campo_adjunto a
                INNER JOIN form_formulario_usuario ffu ON ffu.id = a.id_formulario_usuario
                WHERE a.id = @id
                  AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)", cnn);
            cmd.Parameters.AddWithValue("@id", idAdjunto);
            cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new AdjuntoDetalleInterno
            {
                Id                    = r.GetInt32(0),
                IdCampo               = r.GetInt32(1),
                IdFormularioUsuario   = r.GetInt32(2),
                UsuarioPropietario    = r.GetString(3),
                NombreOriginal        = r.GetString(4),
                NombreArchivo         = r.GetString(5),
                TipoMime              = r.GetString(6),
                TamanoBytes           = r.GetInt64(7)
            };
        });
    }

    public async Task EliminarAdjunto(int idAdjunto)
    {
        await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var cmd = new SqlCommand("DELETE FROM form_campo_adjunto WHERE id = @id", cnn);
            cmd.Parameters.AddWithValue("@id", idAdjunto);
            cmd.ExecuteNonQuery();
        });
    }

    public async Task<InfoNotificacionEmailDto> RechazarDenuncia(int idDenuncia, string adminUsuario, string comentario, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            string connStr = _config.GetConnectionString("SQLConnectionString")!;
            using var cnn = new SqlConnection(connStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int? idEmpresaDenuncia = null;
                object? estadoActual;
                string? correoDenunciante = null;
                int? consecutivo = null;
                using (var cmdEstado = new SqlCommand(
                    "SELECT idgnl_estado_fk, id_empresa_fk, usuario, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEstado.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEstado.ExecuteReader();
                    estadoActual = null;
                    if (r.Read())
                    {
                        estadoActual = r.IsDBNull(0) ? null : r.GetInt32(0);
                        idEmpresaDenuncia = r.IsDBNull(1) ? null : r.GetInt32(1);
                        correoDenunciante = r.IsDBNull(2) ? null : r.GetString(2);
                        consecutivo = r.IsDBNull(3) ? null : r.GetInt32(3);
                    }
                }

                if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                    throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 5, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id", idDenuncia);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, @anterior, 5, @comentario, @admin)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id", idDenuncia);
                cmdHist.Parameters.AddWithValue("@anterior", estadoActual ?? (object)DBNull.Value);
                cmdHist.Parameters.AddWithValue("@comentario", comentario);
                cmdHist.Parameters.AddWithValue("@admin", adminUsuario);
                cmdHist.ExecuteNonQuery();

                tx.Commit();

                _ = _notif.InsertarADuenio(idDenuncia,
                    "Su denuncia ha sido rechazada.", "danger");

                return new InfoNotificacionEmailDto
                {
                    Consecutivo = consecutivo,
                    CorreoDenunciante = correoDenunciante
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error rejecting denuncia {IdDenuncia}", idDenuncia);
                throw;
            }
        });
    }
}
