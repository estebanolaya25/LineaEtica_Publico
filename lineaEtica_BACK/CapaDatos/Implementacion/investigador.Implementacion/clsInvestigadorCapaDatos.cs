using CapaDatos.Interfaz.investigador.interfaz;
using CapaDatos.Interfaz.notificaciones.interfaz;
using CapaDTO.Denuncia;
using CapaDTO.Investigador;
using CapaDTO.Notificaciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CapaDatos.Implementacion.investigador.Implementacion;

public class clsInvestigadorCapaDatos : IInvestigadorCapaDatos
{
    private readonly IConfiguration _config;
    private readonly ILogger<clsInvestigadorCapaDatos> _logger;
    private readonly INotificacionesCapaDatos _notif;

    private string ConnStr => _config.GetConnectionString("SQLConnectionString")!;

    public clsInvestigadorCapaDatos(
        IConfiguration config,
        ILogger<clsInvestigadorCapaDatos> logger,
        INotificacionesCapaDatos notif)
    {
        _config = config;
        _logger = logger;
        _notif  = notif;
    }

    public async Task<List<InvestigadorResumenDto>> GetInvestigadores(int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT id, ISNULL(nombre, usuario) AS nombre, ISNULL(correo, '') AS correo
                FROM gnl_usuario
                WHERE es_admin = 2 AND activo = 1
                  AND (@idEmpresaFiltro IS NULL OR id_empresa_fk = @idEmpresaFiltro)
                ORDER BY nombre", cnn);
            cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
            using var r = cmd.ExecuteReader();
            var lista = new List<InvestigadorResumenDto>();
            while (r.Read())
                lista.Add(new InvestigadorResumenDto
                {
                    Id     = r.GetInt32(0),
                    Nombre = r.GetString(1),
                    Correo = r.GetString(2)
                });
            return lista;
        });
    }

    public async Task<InfoNotificacionEmailDto> AsignarInvestigador(int idDenuncia, int idInvestigador, string adminUsuario, string? comentario, int? idEmpresaFiltro = null)
    {
        var info = await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int? idEmpresaDenuncia = null;
                object? estadoActual;
                int? consecutivo = null;
                using (var cmdEst = new SqlCommand(
                    "SELECT idgnl_estado_fk, id_empresa_fk, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEst.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEst.ExecuteReader();
                    estadoActual = null;
                    if (r.Read())
                    {
                        estadoActual = r.IsDBNull(0) ? null : r.GetInt32(0);
                        idEmpresaDenuncia = r.IsDBNull(1) ? null : r.GetInt32(1);
                        consecutivo = r.IsDBNull(2) ? null : r.GetInt32(2);
                    }
                }

                if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                    throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");

                int? idEmpresaInvestigador = null;
                string? correoInvestigador = null;
                using (var cmdInv = new SqlCommand(
                    "SELECT id_empresa_fk, correo FROM gnl_usuario WHERE id = @id AND es_admin = 2 AND activo = 1", cnn, tx))
                {
                    cmdInv.Parameters.AddWithValue("@id", idInvestigador);
                    using var r = cmdInv.ExecuteReader();
                    if (r.Read())
                    {
                        idEmpresaInvestigador = r.IsDBNull(0) ? null : r.GetInt32(0);
                        correoInvestigador = r.IsDBNull(1) ? null : r.GetString(1);
                    }
                }

                if (idEmpresaInvestigador == null || idEmpresaInvestigador != idEmpresaDenuncia)
                    throw new UnauthorizedAccessException("El investigador no pertenece a la empresa de la denuncia.");

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 8, id_investigador_fk = @inv, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id",  idDenuncia);
                cmdUpd.Parameters.AddWithValue("@inv", idInvestigador);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, @anterior, 8, @comentario, @admin)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id",        idDenuncia);
                cmdHist.Parameters.AddWithValue("@anterior",  estadoActual ?? (object)DBNull.Value);
                cmdHist.Parameters.AddWithValue("@comentario",(object?)comentario ?? DBNull.Value);
                cmdHist.Parameters.AddWithValue("@admin",     adminUsuario);
                cmdHist.ExecuteNonQuery();

                tx.Commit();

                return new InfoNotificacionEmailDto { Consecutivo = consecutivo, CorreoInvestigador = correoInvestigador };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error asignando investigador a denuncia {Id}", idDenuncia);
                throw;
            }
        });

        _ = _notif.Insertar(
            idInvestigador,
            "Se te ha asignado una nueva denuncia para investigar.",
            "info",
            idDenuncia);

        return info;
    }

    public async Task<List<DenunciaInvestigadorResumenDto>> GetMisDenuncias(string usuario)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT ffu.id, ffu.consecutivo, ff.nombre AS nombre_formulario,
                       ISNULL(ge.nombre, 'Desconocido') AS estado,
                       ffu.idgnl_estado_fk AS id_estado,
                       CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                       ffu.fecha_creacion,
                       ffu.fecha_actualizacion AS fecha_asignacion
                FROM form_formulario_usuario ffu
                INNER JOIN form_formulario ff ON ff.id = ffu.idform_formulario_fk
                LEFT  JOIN gnl_estado ge ON ge.id = ffu.idgnl_estado_fk
                INNER JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                WHERE inv.usuario = @usuario AND ffu.idgnl_estado_fk IN (8, 9, 10)
                ORDER BY ffu.fecha_actualizacion DESC", cnn);
            cmd.Parameters.AddWithValue("@usuario", usuario);

            using var r = cmd.ExecuteReader();
            var lista = new List<DenunciaInvestigadorResumenDto>();
            while (r.Read())
                lista.Add(new DenunciaInvestigadorResumenDto
                {
                    Id               = r.GetInt32(r.GetOrdinal("id")),
                    Consecutivo      = r.IsDBNull(r.GetOrdinal("consecutivo")) ? null : r.GetInt32(r.GetOrdinal("consecutivo")),
                    NombreFormulario = r.GetString(r.GetOrdinal("nombre_formulario")),
                    Estado           = r.GetString(r.GetOrdinal("estado")),
                    IdEstado         = r.GetInt32(r.GetOrdinal("id_estado")),
                    EsAnonimo        = r.GetInt32(r.GetOrdinal("es_anonimo")) == 1,
                    FechaCreacion    = r.IsDBNull(r.GetOrdinal("fecha_creacion")) ? null : r.GetDateTime(r.GetOrdinal("fecha_creacion")),
                    FechaAsignacion  = r.IsDBNull(r.GetOrdinal("fecha_asignacion")) ? null : r.GetDateTime(r.GetOrdinal("fecha_asignacion"))
                });
            return lista;
        });
    }

    public async Task<DenunciaInvestigadorDetalleDto?> GetDetalle(int idDenuncia, string usuario)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();

            using var cmdHead = new SqlCommand(@"
                SELECT ffu.id, ffu.idform_formulario_fk, ffu.idgnl_estado_fk,
                       ISNULL(ge.nombre, 'Desconocido') AS estado, ffu.fecha_creacion,
                       ffu.consecutivo,
                       CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo
                FROM form_formulario_usuario ffu
                LEFT  JOIN gnl_estado ge ON ge.id = ffu.idgnl_estado_fk
                INNER JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                WHERE ffu.id = @id AND inv.usuario = @usuario", cnn);
            cmdHead.Parameters.AddWithValue("@id",      idDenuncia);
            cmdHead.Parameters.AddWithValue("@usuario", usuario);

            DenunciaInvestigadorDetalleDto? detalle = null;
            using (var r = cmdHead.ExecuteReader())
            {
                if (!r.Read()) return null;
                detalle = new DenunciaInvestigadorDetalleDto
                {
                    Id           = r.GetInt32(r.GetOrdinal("id")),
                    IdFormulario = r.GetInt32(r.GetOrdinal("idform_formulario_fk")),
                    IdEstado     = r.IsDBNull(r.GetOrdinal("idgnl_estado_fk")) ? 0 : r.GetInt32(r.GetOrdinal("idgnl_estado_fk")),
                    Estado       = r.GetString(r.GetOrdinal("estado")),
                    EsAnonimo    = r.GetInt32(r.GetOrdinal("es_anonimo")) == 1,
                    FechaCreacion = r.IsDBNull(r.GetOrdinal("fecha_creacion")) ? null : r.GetDateTime(r.GetOrdinal("fecha_creacion")),
                    Consecutivo  = r.IsDBNull(r.GetOrdinal("consecutivo")) ? null : r.GetInt32(r.GetOrdinal("consecutivo"))
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
                        IdCampo        = r.GetInt32(r.GetOrdinal("IdCampo")),
                        IdOpcion       = r.IsDBNull(r.GetOrdinal("IdOpcion")) ? null : r.GetInt32(r.GetOrdinal("IdOpcion")),
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
                        IdCampoTabla   = r.GetInt32(r.GetOrdinal("IdCampoTabla")),
                        IdGrupo        = r.GetInt32(r.GetOrdinal("IdGrupo")),
                        IdCampoHijo    = r.GetInt32(r.GetOrdinal("IdCampoHijo")),
                        TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                    });
            }

            using var cmdCom = new SqlCommand(@"
                SELECT TOP 1 comentario FROM form_historial_estado
                WHERE id_formulario_usuario = @id ORDER BY fecha DESC", cnn);
            cmdCom.Parameters.AddWithValue("@id", idDenuncia);
            var com = cmdCom.ExecuteScalar();
            detalle.ComentarioAdmin = com == DBNull.Value ? null : (string?)com;

            detalle.Seguimientos = GetSeguimientosConAdjuntos(idDenuncia, cnn);

            return detalle;
        });
    }

    public async Task<List<SeguimientoDto>> GetSeguimientos(int idDenuncia)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            return GetSeguimientosConAdjuntos(idDenuncia, cnn);
        });
    }

    private List<SeguimientoDto> GetSeguimientosConAdjuntos(int idDenuncia, SqlConnection cnn)
    {
        var seguimientos = new List<SeguimientoDto>();
        using (var cmd = new SqlCommand(@"
            SELECT id, comentario, fecha, usuario
            FROM inv_seguimiento
            WHERE id_formulario_usuario_fk = @id
            ORDER BY fecha ASC", cnn))
        {
            cmd.Parameters.AddWithValue("@id", idDenuncia);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                seguimientos.Add(new SeguimientoDto
                {
                    Id         = r.GetInt32(0),
                    Comentario = r.GetString(1),
                    Fecha      = r.GetDateTime(2),
                    Usuario    = r.GetString(3)
                });
        }

        if (seguimientos.Count == 0) return seguimientos;

        var segIds = string.Join(",", seguimientos.Select(s => s.Id));
        using var cmdAdj = new SqlCommand($@"
            SELECT id, id_seguimiento_fk, nombre_original, nombre_archivo, tipo_mime, tamano_bytes, fecha_subida, usuario
            FROM inv_adjunto_investigador
            WHERE id_seguimiento_fk IN ({segIds})
            ORDER BY id_seguimiento_fk, fecha_subida", cnn);
        using var rAdj = cmdAdj.ExecuteReader();

        var adjMap = new Dictionary<int, List<AdjuntoInvestigadorDto>>();
        while (rAdj.Read())
        {
            int segId = rAdj.GetInt32(1);
            if (!adjMap.ContainsKey(segId)) adjMap[segId] = new List<AdjuntoInvestigadorDto>();
            adjMap[segId].Add(new AdjuntoInvestigadorDto
            {
                Id             = rAdj.GetInt32(0),
                NombreOriginal = rAdj.GetString(2),
                NombreArchivo  = rAdj.GetString(3),
                TipoMime       = rAdj.GetString(4),
                TamanoBytes    = rAdj.GetInt64(5),
                FechaSubida    = rAdj.GetDateTime(6),
                Usuario        = rAdj.GetString(7)
            });
        }

        foreach (var seg in seguimientos)
            if (adjMap.TryGetValue(seg.Id, out var adjs))
                seg.Adjuntos = adjs;

        return seguimientos;
    }

    public async Task<SeguimientoDto> CrearSeguimiento(
        int idDenuncia,
        string comentario,
        string usuario,
        List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int idSeg;
                DateTime fechaSeg;
                using (var cmd = new SqlCommand(@"
                    INSERT INTO inv_seguimiento (id_formulario_usuario_fk, comentario, usuario)
                    OUTPUT INSERTED.id, INSERTED.fecha
                    VALUES (@id, @comentario, @usuario)", cnn, tx))
                {
                    cmd.Parameters.AddWithValue("@id",         idDenuncia);
                    cmd.Parameters.AddWithValue("@comentario", comentario);
                    cmd.Parameters.AddWithValue("@usuario",    usuario);
                    using var r = cmd.ExecuteReader();
                    r.Read();
                    idSeg   = r.GetInt32(0);
                    fechaSeg = r.GetDateTime(1);
                }

                var adjuntos = new List<AdjuntoInvestigadorDto>();
                foreach (var arch in archivos)
                {
                    using var cmdAdj = new SqlCommand(@"
                        INSERT INTO inv_adjunto_investigador
                            (id_formulario_usuario_fk, id_seguimiento_fk, nombre_original, nombre_archivo, tipo_mime, tamano_bytes, usuario)
                        OUTPUT INSERTED.id, INSERTED.fecha_subida
                        VALUES (@ffu, @seg, @nomOrig, @nomArch, @mime, @tam, @usuario)", cnn, tx);
                    cmdAdj.Parameters.AddWithValue("@ffu",     idDenuncia);
                    cmdAdj.Parameters.AddWithValue("@seg",     idSeg);
                    cmdAdj.Parameters.AddWithValue("@nomOrig", arch.NombreOriginal);
                    cmdAdj.Parameters.AddWithValue("@nomArch", arch.NombreArchivo);
                    cmdAdj.Parameters.AddWithValue("@mime",    arch.TipoMime);
                    cmdAdj.Parameters.AddWithValue("@tam",     arch.TamanoBytes);
                    cmdAdj.Parameters.AddWithValue("@usuario", usuario);
                    using var rAdj = cmdAdj.ExecuteReader();
                    rAdj.Read();
                    adjuntos.Add(new AdjuntoInvestigadorDto
                    {
                        Id             = rAdj.GetInt32(0),
                        NombreOriginal = arch.NombreOriginal,
                        NombreArchivo  = arch.NombreArchivo,
                        TipoMime       = arch.TipoMime,
                        TamanoBytes    = arch.TamanoBytes,
                        FechaSubida    = rAdj.GetDateTime(1),
                        Usuario        = usuario
                    });
                }

                tx.Commit();

                return new SeguimientoDto
                {
                    Id         = idSeg,
                    Comentario = comentario,
                    Fecha      = fechaSeg,
                    Usuario    = usuario,
                    Adjuntos   = adjuntos
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error creando seguimiento para denuncia {Id}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<AdjuntoInvestigadorDto?> GetAdjuntoDetalle(int idAdjunto, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT a.id, a.nombre_original, a.nombre_archivo, a.tipo_mime, a.tamano_bytes, a.fecha_subida, a.usuario
                FROM inv_adjunto_investigador a
                INNER JOIN form_formulario_usuario ffu ON ffu.id = a.id_formulario_usuario_fk
                WHERE a.id = @id
                  AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)", cnn);
            cmd.Parameters.AddWithValue("@id", idAdjunto);
            cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;
            return new AdjuntoInvestigadorDto
            {
                Id             = r.GetInt32(0),
                NombreOriginal = r.GetString(1),
                NombreArchivo  = r.GetString(2),
                TipoMime       = r.GetString(3),
                TamanoBytes    = r.GetInt64(4),
                FechaSubida    = r.GetDateTime(5),
                Usuario        = r.GetString(6)
            };
        });
    }

    public async Task<InfoNotificacionEmailDto> CerrarInvestigacion(int idDenuncia, string usuario, string comentario)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                string? correoDenunciante = null;
                int? consecutivo = null;
                object? estadoActual;
                using (var cmdEst = new SqlCommand(
                    "SELECT idgnl_estado_fk, usuario, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEst.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEst.ExecuteReader();
                    estadoActual = null;
                    if (r.Read())
                    {
                        estadoActual = r.IsDBNull(0) ? null : r.GetInt32(0);
                        correoDenunciante = r.IsDBNull(1) ? null : r.GetString(1);
                        consecutivo = r.IsDBNull(2) ? null : r.GetInt32(2);
                    }
                }

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 9, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id", idDenuncia);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, @anterior, 9, @comentario, @usuario)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id",         idDenuncia);
                cmdHist.Parameters.AddWithValue("@anterior",   estadoActual ?? (object)DBNull.Value);
                cmdHist.Parameters.AddWithValue("@comentario", comentario);
                cmdHist.Parameters.AddWithValue("@usuario",    usuario);
                cmdHist.ExecuteNonQuery();

                tx.Commit();

                _ = _notif.InsertarADuenio(idDenuncia,
                    "La investigación de su denuncia ha sido cerrada.", "success");

                return new InfoNotificacionEmailDto { Consecutivo = consecutivo, CorreoDenunciante = correoDenunciante };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error cerrando investigacion {Id}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<(SeguimientoDto Seguimiento, InfoNotificacionEmailDto Info)> SolicitarInformacionAnalista(
        int idDenuncia, string usuario, string comentario,
        List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos)
    {
        SeguimientoDto seg = await CrearSeguimiento(idDenuncia, comentario, usuario, archivos);

        var info = await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 10, fecha_actualizacion = GETDATE()
                    WHERE id = @id AND idgnl_estado_fk = 8", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id", idDenuncia);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, 8, 10, @comentario, @usuario)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id",         idDenuncia);
                cmdHist.Parameters.AddWithValue("@comentario", $"Investigador solicita información: {comentario}");
                cmdHist.Parameters.AddWithValue("@usuario",    usuario);
                cmdHist.ExecuteNonQuery();

                int? idEmpresaDenuncia = null;
                int? consecutivo = null;
                using (var cmdEmp = new SqlCommand(
                    "SELECT id_empresa_fk, consecutivo FROM form_formulario_usuario WHERE id = @id", cnn, tx))
                {
                    cmdEmp.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdEmp.ExecuteReader();
                    if (r.Read())
                    {
                        idEmpresaDenuncia = r.IsDBNull(0) ? null : r.GetInt32(0);
                        consecutivo = r.IsDBNull(1) ? null : r.GetInt32(1);
                    }
                }

                var analistas = new List<int>();
                var correosAnalistas = new List<string>();
                using (var cmdAdm = new SqlCommand(
                    "SELECT id, correo FROM gnl_usuario WHERE es_admin = 1 AND activo = 1 AND (@idEmpresa IS NULL OR id_empresa_fk = @idEmpresa)", cnn, tx))
                {
                    cmdAdm.Parameters.AddWithValue("@idEmpresa", (object?)idEmpresaDenuncia ?? DBNull.Value);
                    using var rAdm = cmdAdm.ExecuteReader();
                    while (rAdm.Read())
                    {
                        analistas.Add(rAdm.GetInt32(0));
                        if (!rAdm.IsDBNull(1) && !string.IsNullOrWhiteSpace(rAdm.GetString(1)))
                            correosAnalistas.Add(rAdm.GetString(1));
                    }
                }

                tx.Commit();

                foreach (var idAnalista in analistas)
                    _ = _notif.Insertar(idAnalista, "Un investigador solicita información sobre una denuncia.", "warning", idDenuncia);

                return new InfoNotificacionEmailDto { Consecutivo = consecutivo, CorreosConsultores = correosAnalistas };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error al solicitar información al analista para denuncia {Id}", idDenuncia);
                throw;
            }
        });

        return (seg, info);
    }

    public async Task<(SeguimientoDto Seguimiento, InfoNotificacionEmailDto Info)> ResponderInvestigador(
        int idDenuncia, string adminUsuario, string comentario,
        List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos,
        int? idEmpresaFiltro = null)
    {
        SeguimientoDto seg = await CrearSeguimiento(idDenuncia, comentario, adminUsuario, archivos);

        var info = await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                int idInvestigador = 0;
                string? correoInvestigador = null;
                int? idEmpresaDenuncia = null;
                int? consecutivo = null;
                using (var cmdInv = new SqlCommand(@"
                    SELECT inv.id, inv.correo, ffu.id_empresa_fk, ffu.consecutivo FROM form_formulario_usuario ffu
                    LEFT JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                    WHERE ffu.id = @id", cnn, tx))
                {
                    cmdInv.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmdInv.ExecuteReader();
                    if (r.Read())
                    {
                        if (!r.IsDBNull(0)) idInvestigador = r.GetInt32(0);
                        correoInvestigador = r.IsDBNull(1) ? null : r.GetString(1);
                        idEmpresaDenuncia = r.IsDBNull(2) ? null : r.GetInt32(2);
                        consecutivo = r.IsDBNull(3) ? null : r.GetInt32(3);
                    }
                }

                if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                    throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");

                using var cmdUpd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET idgnl_estado_fk = 8, fecha_actualizacion = GETDATE()
                    WHERE id = @id AND idgnl_estado_fk = 10", cnn, tx);
                cmdUpd.Parameters.AddWithValue("@id", idDenuncia);
                cmdUpd.ExecuteNonQuery();

                using var cmdHist = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, 10, 8, @comentario, @usuario)", cnn, tx);
                cmdHist.Parameters.AddWithValue("@id",         idDenuncia);
                cmdHist.Parameters.AddWithValue("@comentario", $"Analista responde al investigador: {comentario}");
                cmdHist.Parameters.AddWithValue("@usuario",    adminUsuario);
                cmdHist.ExecuteNonQuery();

                tx.Commit();

                if (idInvestigador > 0)
                    _ = _notif.Insertar(idInvestigador, "El analista respondió a tu solicitud de información.", "info", idDenuncia);

                return new InfoNotificacionEmailDto
                {
                    Consecutivo = consecutivo,
                    CorreoInvestigador = idInvestigador > 0 ? correoInvestigador : null
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error al responder al investigador para denuncia {Id}", idDenuncia);
                throw;
            }
        });

        return (seg, info);
    }

    public async Task<SeguimientoDto> AgregarComentarioAdmin(
        int idDenuncia, string usuarioAdmin, string comentario,
        List<(string NombreOriginal, string NombreArchivo, string TipoMime, long TamanoBytes)> archivos,
        int? idEmpresaFiltro = null)
    {
        int? idInvestigador = null;
        await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var cmd = new SqlCommand(
                "SELECT id_investigador_fk, id_empresa_fk FROM form_formulario_usuario WHERE id = @id", cnn);
            cmd.Parameters.AddWithValue("@id", idDenuncia);
            using var r = cmd.ExecuteReader();
            int? idEmpresaDenuncia = null;
            if (r.Read())
            {
                idInvestigador = r.IsDBNull(0) ? null : r.GetInt32(0);
                idEmpresaDenuncia = r.IsDBNull(1) ? null : r.GetInt32(1);
            }
            r.Close();

            if (idEmpresaFiltro.HasValue && idEmpresaDenuncia != idEmpresaFiltro)
                throw new UnauthorizedAccessException("La denuncia no pertenece a su empresa.");
        });

        var seg = await CrearSeguimiento(idDenuncia, comentario, usuarioAdmin, archivos);

        if (idInvestigador.HasValue)
            _ = _notif.Insertar(
                idInvestigador.Value,
                "Se agregó un nuevo comentario de seguimiento a su investigación.",
                "info",
                idDenuncia);

        return seg;
    }

    public async Task<InfoNotificacionEmailDto> ReasignarInvestigador(int idDenuncia, int idNuevoInvestigador, string usuarioActual, string comentario)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var tx = cnn.BeginTransaction();
            try
            {
                string nombreAnterior;
                int? idEmpresaDenuncia;
                int? consecutivo;
                using (var cmd = new SqlCommand(@"
                    SELECT ISNULL(inv.nombre, inv.usuario), ffu.id_empresa_fk, ffu.consecutivo
                    FROM form_formulario_usuario ffu
                    INNER JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                    WHERE ffu.id = @id AND ffu.idgnl_estado_fk = 8", cnn, tx))
                {
                    cmd.Parameters.AddWithValue("@id", idDenuncia);
                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                        throw new InvalidOperationException("La denuncia no está en estado de investigación activa.");
                    nombreAnterior = r.GetString(0);
                    idEmpresaDenuncia = r.IsDBNull(1) ? null : r.GetInt32(1);
                    consecutivo = r.IsDBNull(2) ? null : r.GetInt32(2);
                }

                string nombreNuevo;
                string? correoNuevo;
                using (var cmd = new SqlCommand(
                    "SELECT ISNULL(nombre, usuario), id_empresa_fk, correo FROM gnl_usuario WHERE id = @id AND es_admin = 2 AND activo = 1", cnn, tx))
                {
                    cmd.Parameters.AddWithValue("@id", idNuevoInvestigador);
                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                        throw new InvalidOperationException("El investigador seleccionado no existe o no está activo.");
                    nombreNuevo = r.GetString(0);
                    int? idEmpresaNuevo = r.IsDBNull(1) ? null : r.GetInt32(1);
                    correoNuevo = r.IsDBNull(2) ? null : r.GetString(2);
                    if (idEmpresaNuevo == null || idEmpresaNuevo != idEmpresaDenuncia)
                        throw new UnauthorizedAccessException("El investigador no pertenece a la empresa de la denuncia.");
                }

                using (var cmd = new SqlCommand(@"
                    UPDATE form_formulario_usuario
                    SET id_investigador_fk = @nuevo, fecha_actualizacion = GETDATE()
                    WHERE id = @id", cnn, tx))
                {
                    cmd.Parameters.AddWithValue("@id",    idDenuncia);
                    cmd.Parameters.AddWithValue("@nuevo", idNuevoInvestigador);
                    cmd.ExecuteNonQuery();
                }

                var comentarioHistorial = $"Reasignado de '{nombreAnterior}' a '{nombreNuevo}'. Motivo: {comentario}";
                using (var cmd = new SqlCommand(@"
                    INSERT INTO form_historial_estado
                        (id_formulario_usuario, estado_anterior, estado_nuevo, comentario, usuario_admin)
                    VALUES (@id, 8, 8, @comentario, @usuario)", cnn, tx))
                {
                    cmd.Parameters.AddWithValue("@id",         idDenuncia);
                    cmd.Parameters.AddWithValue("@comentario", comentarioHistorial);
                    cmd.Parameters.AddWithValue("@usuario",    usuarioActual);
                    cmd.ExecuteNonQuery();
                }

                tx.Commit();

                _ = _notif.Insertar(
                    idNuevoInvestigador,
                    $"Se te ha reasignado una investigación anteriormente a cargo de {nombreAnterior}.",
                    "info",
                    idDenuncia);

                return new InfoNotificacionEmailDto
                {
                    Consecutivo = consecutivo,
                    CorreoInvestigador = correoNuevo,
                    NombreInvestigadorAnterior = nombreAnterior
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, "Error reasignando investigador en denuncia {Id}", idDenuncia);
                throw;
            }
        });
    }

    public async Task<List<InvestigacionAdminResumenDto>> GetInvestigacionesAdmin(int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();
            using var cmd = new SqlCommand(@"
                SELECT ffu.id, ffu.consecutivo, ff.nombre AS nombre_formulario,
                       ISNULL(ge.nombre, 'Desconocido') AS estado,
                       ffu.idgnl_estado_fk AS id_estado,
                       CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                       ffu.fecha_creacion,
                       ffu.fecha_actualizacion AS fecha_asignacion,
                       ISNULL(inv.nombre, inv.usuario) AS nombre_investigador,
                       ISNULL(inv.correo, '')           AS correo_investigador,
                       (SELECT COUNT(*) FROM inv_seguimiento s WHERE s.id_formulario_usuario_fk = ffu.id) AS cantidad_seguimientos,
                       ffu.id_empresa_fk, emp.nombre AS nombre_empresa
                FROM form_formulario_usuario ffu
                INNER JOIN form_formulario ff ON ff.id = ffu.idform_formulario_fk
                LEFT  JOIN gnl_estado ge ON ge.id = ffu.idgnl_estado_fk
                LEFT  JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                LEFT  JOIN gnl_empresa emp ON emp.id = ffu.id_empresa_fk
                WHERE ffu.idgnl_estado_fk IN (8, 9)
                  AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)
                ORDER BY ffu.fecha_actualizacion DESC", cnn);
            cmd.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

            using var r = cmd.ExecuteReader();
            var lista = new List<InvestigacionAdminResumenDto>();
            while (r.Read())
                lista.Add(new InvestigacionAdminResumenDto
                {
                    Id                  = r.GetInt32(r.GetOrdinal("id")),
                    Consecutivo         = r.IsDBNull(r.GetOrdinal("consecutivo"))       ? null : r.GetInt32(r.GetOrdinal("consecutivo")),
                    NombreFormulario    = r.GetString(r.GetOrdinal("nombre_formulario")),
                    Estado              = r.GetString(r.GetOrdinal("estado")),
                    IdEstado            = r.GetInt32(r.GetOrdinal("id_estado")),
                    EsAnonimo           = r.GetInt32(r.GetOrdinal("es_anonimo")) == 1,
                    FechaCreacion       = r.IsDBNull(r.GetOrdinal("fecha_creacion"))    ? null : r.GetDateTime(r.GetOrdinal("fecha_creacion")),
                    FechaAsignacion     = r.IsDBNull(r.GetOrdinal("fecha_asignacion"))  ? null : r.GetDateTime(r.GetOrdinal("fecha_asignacion")),
                    NombreInvestigador  = r.IsDBNull(r.GetOrdinal("nombre_investigador"))? null : r.GetString(r.GetOrdinal("nombre_investigador")),
                    CorreoInvestigador  = r.IsDBNull(r.GetOrdinal("correo_investigador"))? null : r.GetString(r.GetOrdinal("correo_investigador")),
                    CantidadSeguimientos = r.GetInt32(r.GetOrdinal("cantidad_seguimientos")),
                    IdEmpresa           = r.IsDBNull(r.GetOrdinal("id_empresa_fk")) ? null : r.GetInt32(r.GetOrdinal("id_empresa_fk")),
                    NombreEmpresa       = r.IsDBNull(r.GetOrdinal("nombre_empresa")) ? null : r.GetString(r.GetOrdinal("nombre_empresa"))
                });
            return lista;
        });
    }

    public async Task<DenunciaInvestigadorDetalleDto?> GetDetalleAdmin(int idDenuncia, int? idEmpresaFiltro = null)
    {
        return await Task.Run(() =>
        {
            using var cnn = new SqlConnection(ConnStr);
            cnn.Open();

            using var cmdHead = new SqlCommand(@"
                SELECT ffu.id, ffu.idform_formulario_fk, ffu.idgnl_estado_fk,
                       ISNULL(ge.nombre, 'Desconocido') AS estado, ffu.fecha_creacion,
                       ffu.consecutivo,
                       CASE WHEN ffu.idform_formulario_fk = 1 THEN 1 ELSE 0 END AS es_anonimo,
                       ISNULL(inv.nombre, inv.usuario) AS nombre_investigador,
                       ISNULL(inv.correo, '')           AS correo_investigador
                FROM form_formulario_usuario ffu
                LEFT JOIN gnl_estado ge ON ge.id = ffu.idgnl_estado_fk
                LEFT JOIN gnl_usuario inv ON inv.id = ffu.id_investigador_fk
                WHERE ffu.id = @id AND ffu.idgnl_estado_fk IN (8, 9, 10)
                  AND (@idEmpresaFiltro IS NULL OR ffu.id_empresa_fk = @idEmpresaFiltro)", cnn);
            cmdHead.Parameters.AddWithValue("@id", idDenuncia);
            cmdHead.Parameters.AddWithValue("@idEmpresaFiltro", (object?)idEmpresaFiltro ?? DBNull.Value);

            DenunciaInvestigadorDetalleDto? detalle = null;
            using (var r = cmdHead.ExecuteReader())
            {
                if (!r.Read()) return null;
                detalle = new DenunciaInvestigadorDetalleDto
                {
                    Id                 = r.GetInt32(r.GetOrdinal("id")),
                    IdFormulario       = r.GetInt32(r.GetOrdinal("idform_formulario_fk")),
                    IdEstado           = r.IsDBNull(r.GetOrdinal("idgnl_estado_fk")) ? 0 : r.GetInt32(r.GetOrdinal("idgnl_estado_fk")),
                    Estado             = r.GetString(r.GetOrdinal("estado")),
                    EsAnonimo          = r.GetInt32(r.GetOrdinal("es_anonimo")) == 1,
                    FechaCreacion      = r.IsDBNull(r.GetOrdinal("fecha_creacion")) ? null : r.GetDateTime(r.GetOrdinal("fecha_creacion")),
                    Consecutivo        = r.IsDBNull(r.GetOrdinal("consecutivo")) ? null : r.GetInt32(r.GetOrdinal("consecutivo")),
                    NombreInvestigador = r.IsDBNull(r.GetOrdinal("nombre_investigador")) ? null : r.GetString(r.GetOrdinal("nombre_investigador")),
                    CorreoInvestigador = r.IsDBNull(r.GetOrdinal("correo_investigador")) ? null : r.GetString(r.GetOrdinal("correo_investigador"))
                };
            }
            if (detalle == null) return null;

            using var cmdResp = new SqlCommand(@"
                SELECT idform_campo_fk AS IdCampo, id_opcion_respuesta AS IdOpcion, texto_respuesta AS TextoRespuesta
                FROM form_campo_respuesta WHERE idform_formulario_usuario_fk = @id", cnn);
            cmdResp.Parameters.AddWithValue("@id", idDenuncia);
            using (var r = cmdResp.ExecuteReader())
                while (r.Read())
                    detalle.Respuestas.Add(new RespuestaCampoDto
                    {
                        IdCampo        = r.GetInt32(r.GetOrdinal("IdCampo")),
                        IdOpcion       = r.IsDBNull(r.GetOrdinal("IdOpcion")) ? null : r.GetInt32(r.GetOrdinal("IdOpcion")),
                        TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                    });

            using var cmdTabla = new SqlCommand(@"
                SELECT id_campo_tabla AS IdCampoTabla, id_grupo AS IdGrupo,
                       idform_campo_fk AS IdCampoHijo, texto_respuesta AS TextoRespuesta
                FROM form_campo_tabla_respuesta WHERE idform_formulario_usuario_fk = @id
                ORDER BY id_campo_tabla, id_grupo, idform_campo_fk", cnn);
            cmdTabla.Parameters.AddWithValue("@id", idDenuncia);
            using (var r = cmdTabla.ExecuteReader())
                while (r.Read())
                    detalle.RespuestasTabla.Add(new RespuestaTablaDto
                    {
                        IdCampoTabla   = r.GetInt32(r.GetOrdinal("IdCampoTabla")),
                        IdGrupo        = r.GetInt32(r.GetOrdinal("IdGrupo")),
                        IdCampoHijo    = r.GetInt32(r.GetOrdinal("IdCampoHijo")),
                        TextoRespuesta = r.IsDBNull(r.GetOrdinal("TextoRespuesta")) ? null : r.GetString(r.GetOrdinal("TextoRespuesta"))
                    });

            using var cmdCom = new SqlCommand(@"
                SELECT TOP 1 comentario FROM form_historial_estado
                WHERE id_formulario_usuario = @id ORDER BY fecha DESC", cnn);
            cmdCom.Parameters.AddWithValue("@id", idDenuncia);
            var com = cmdCom.ExecuteScalar();
            detalle.ComentarioAdmin = com == DBNull.Value ? null : (string?)com;

            detalle.Seguimientos = GetSeguimientosConAdjuntos(idDenuncia, cnn);

            return detalle;
        });
    }
}
