IF NOT EXISTS (SELECT 1 FROM gnl_estado WHERE id = 7)
BEGIN
    SET IDENTITY_INSERT gnl_estado ON;
    INSERT INTO gnl_estado (id, idgnl_modulo_fk, codigo, nombre, activo)
    VALUES (7, 1, '7', 'Devuelto', 1);
    SET IDENTITY_INSERT gnl_estado OFF;
END;

IF OBJECT_ID('dbo.form_revision_campo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.form_revision_campo (
        id                      INT IDENTITY(1,1) NOT NULL,
        id_formulario_usuario   INT NOT NULL,
        id_seccion              INT NOT NULL,
        id_campo                INT NOT NULL,
        id_fila                 INT NULL,
        fecha_revision          DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_form_revision_campo PRIMARY KEY CLUSTERED (id),
        CONSTRAINT FK_rev_campo_formulario FOREIGN KEY (id_formulario_usuario)
            REFERENCES dbo.form_formulario_usuario (id),
        CONSTRAINT FK_rev_campo_seccion FOREIGN KEY (id_seccion)
            REFERENCES dbo.form_seccion (id),
        CONSTRAINT FK_rev_campo_campo FOREIGN KEY (id_campo)
            REFERENCES dbo.form_campo (id)
    );
    CREATE NONCLUSTERED INDEX IX_rev_campo_formulario
        ON dbo.form_revision_campo (id_formulario_usuario);
END;

IF OBJECT_ID('dbo.form_historial_estado', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.form_historial_estado (
        id                      INT IDENTITY(1,1) NOT NULL,
        id_formulario_usuario   INT NOT NULL,
        estado_anterior         INT NULL,
        estado_nuevo            INT NOT NULL,
        comentario              NVARCHAR(2000) NULL,
        usuario_admin           NVARCHAR(200) NOT NULL,
        fecha                   DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_form_historial_estado PRIMARY KEY CLUSTERED (id),
        CONSTRAINT FK_historial_formulario FOREIGN KEY (id_formulario_usuario)
            REFERENCES dbo.form_formulario_usuario (id)
    );
    CREATE NONCLUSTERED INDEX IX_historial_formulario
        ON dbo.form_historial_estado (id_formulario_usuario);
END;

PRINT 'Migration admin completed successfully.';
