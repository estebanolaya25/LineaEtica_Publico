CREATE TABLE tbl_Notificaciones (
    id                    INT           IDENTITY(1,1) PRIMARY KEY,
    id_usuario            INT           NOT NULL,
    mensaje               NVARCHAR(500) NOT NULL,
    tipo                  NVARCHAR(20)  NOT NULL DEFAULT 'info',
    leida                 BIT           NOT NULL DEFAULT 0,
    fecha                 DATETIME      NOT NULL DEFAULT GETDATE(),
    id_formulario_usuario INT           NULL,
    CONSTRAINT FK_Notif_Usuario    FOREIGN KEY (id_usuario)            REFERENCES gnl_usuario(id),
    CONSTRAINT FK_Notif_Formulario FOREIGN KEY (id_formulario_usuario) REFERENCES form_formulario_usuario(id)
);

CREATE INDEX IX_Notif_Usuario_Leida ON tbl_Notificaciones (id_usuario, leida);
