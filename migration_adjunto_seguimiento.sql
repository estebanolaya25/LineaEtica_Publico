IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'inv_adjunto_investigador' AND COLUMN_NAME = 'id_seguimiento_fk'
)
    ALTER TABLE inv_adjunto_investigador
        ADD id_seguimiento_fk INT NULL
            CONSTRAINT FK_inv_adj_seg REFERENCES inv_seguimiento(id);

PRINT 'Migración adjunto_seguimiento aplicada correctamente.';
