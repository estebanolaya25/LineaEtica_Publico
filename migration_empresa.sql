IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'gnl_empresa')
BEGIN
    CREATE TABLE gnl_empresa (
        id             INT IDENTITY(1,1) PRIMARY KEY,
        nombre         NVARCHAR(150) NOT NULL,
        activo         BIT NOT NULL DEFAULT 1,
        fecha_creacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'gnl_usuario' AND COLUMN_NAME = 'id_empresa_fk'
)
    ALTER TABLE gnl_usuario
        ADD id_empresa_fk INT NULL
            CONSTRAINT FK_gnl_usuario_empresa REFERENCES gnl_empresa(id);

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'form_formulario_usuario' AND COLUMN_NAME = 'id_empresa_fk'
)
    ALTER TABLE form_formulario_usuario
        ADD id_empresa_fk INT NULL
            CONSTRAINT FK_ffu_empresa REFERENCES gnl_empresa(id);

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_form_formulario_usuario_empresa' AND object_id = OBJECT_ID('form_formulario_usuario')
)
    CREATE INDEX IX_form_formulario_usuario_empresa ON form_formulario_usuario(id_empresa_fk);

PRINT 'Migración empresa aplicada correctamente.';
