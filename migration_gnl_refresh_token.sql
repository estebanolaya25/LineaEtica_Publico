IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'gnl_refresh_token')
BEGIN
    CREATE TABLE [dbo].[gnl_refresh_token] (
        [id]               INT          IDENTITY(1,1) NOT NULL,
        [id_usuario]       INT          NOT NULL,
        [token]            VARCHAR(500) NOT NULL,
        [fecha_expiracion] DATETIME     NOT NULL,
        [fecha_creacion]   DATETIME     NOT NULL CONSTRAINT DF_gnl_refresh_token_fecha_creacion DEFAULT GETDATE(),
        [revocado]         INT          NOT NULL CONSTRAINT DF_gnl_refresh_token_revocado DEFAULT 0,
        [fecha_revocacion] DATETIME     NULL,
        CONSTRAINT PK_gnl_refresh_token PRIMARY KEY CLUSTERED ([id] ASC)
    )
    PRINT 'Tabla gnl_refresh_token creada correctamente.'
END
ELSE
    PRINT 'La tabla gnl_refresh_token ya existe.'
GO
