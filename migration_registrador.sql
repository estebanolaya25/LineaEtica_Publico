IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'form_formulario_usuario' AND COLUMN_NAME = 'origen_canal'
)
    ALTER TABLE form_formulario_usuario
        ADD origen_canal VARCHAR(30) NULL;

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'form_formulario_usuario' AND COLUMN_NAME = 'id_registrador_fk'
)
    ALTER TABLE form_formulario_usuario
        ADD id_registrador_fk INT NULL
            CONSTRAINT FK_ffu_registrador REFERENCES gnl_usuario(id);

UPDATE form_formulario_usuario
SET origen_canal = 'Plataforma'
WHERE origen_canal IS NULL;

PRINT 'Migración registrador aplicada correctamente.';
