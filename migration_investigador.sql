SET IDENTITY_INSERT gnl_estado ON;

IF NOT EXISTS (SELECT 1 FROM gnl_estado WHERE id = 8)
    INSERT INTO gnl_estado (id, nombre) VALUES (8, 'En Investigacion');

IF NOT EXISTS (SELECT 1 FROM gnl_estado WHERE id = 9)
    INSERT INTO gnl_estado (id, nombre) VALUES (9, 'Cerrado');

SET IDENTITY_INSERT gnl_estado OFF;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'form_formulario_usuario' AND COLUMN_NAME = 'id_investigador_fk'
)
    ALTER TABLE form_formulario_usuario
        ADD id_investigador_fk INT NULL
            CONSTRAINT FK_ffu_investigador REFERENCES gnl_usuario(id);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'inv_seguimiento')
CREATE TABLE inv_seguimiento (
    id                       INT IDENTITY(1,1) PRIMARY KEY,
    id_formulario_usuario_fk INT NOT NULL
        CONSTRAINT FK_inv_seg_ffu REFERENCES form_formulario_usuario(id),
    comentario               NVARCHAR(MAX) NOT NULL,
    fecha                    DATETIME NOT NULL DEFAULT GETDATE(),
    usuario                  NVARCHAR(200) NOT NULL
);

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'inv_adjunto_investigador')
CREATE TABLE inv_adjunto_investigador (
    id                       INT IDENTITY(1,1) PRIMARY KEY,
    id_formulario_usuario_fk INT NOT NULL
        CONSTRAINT FK_inv_adj_ffu REFERENCES form_formulario_usuario(id),
    nombre_original          NVARCHAR(500) NOT NULL,
    nombre_archivo           NVARCHAR(500) NOT NULL,
    tipo_mime                NVARCHAR(200) NOT NULL,
    tamano_bytes             BIGINT NOT NULL,
    fecha_subida             DATETIME NOT NULL DEFAULT GETDATE(),
    usuario                  NVARCHAR(200) NOT NULL
);

PRINT 'Migración investigador aplicada correctamente.';
