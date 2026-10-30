IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'form_condicion')
BEGIN
    CREATE TABLE [dbo].[form_condicion] (
        [id]                   INT           IDENTITY(1,1) NOT NULL,
        [idform_formulario_fk] INT           NOT NULL,
        [id_campo_origen]      INT           NULL,
        [condicion]            VARCHAR(50)   NOT NULL,
        [valor_condicion]      VARCHAR(500)  NULL,
        [id_seccion_destino]   INT           NOT NULL,
        [id_campo_destino]     INT           NULL,
        [accion]               VARCHAR(50)   NOT NULL,
        [activo]               INT           NOT NULL CONSTRAINT DF_form_condicion_activo DEFAULT 1,
        CONSTRAINT PK_form_condicion PRIMARY KEY CLUSTERED ([id] ASC)
    );
    PRINT 'Tabla form_condicion creada correctamente.';
END
ELSE
    PRINT 'La tabla form_condicion ya existe.';
