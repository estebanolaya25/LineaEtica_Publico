IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'gnl_rol')
BEGIN
    CREATE TABLE gnl_rol (
        id          INT PRIMARY KEY,
        nombre      VARCHAR(50) NOT NULL,
        descripcion VARCHAR(300) NULL
    );
END

IF NOT EXISTS (SELECT 1 FROM gnl_rol WHERE id = 0)
    INSERT INTO gnl_rol (id, nombre, descripcion) VALUES
    (0, 'Denunciante', 'Reporta denuncias. No pertenece a una empresa fija: la selecciona en cada denuncia que crea.');

IF NOT EXISTS (SELECT 1 FROM gnl_rol WHERE id = 1)
    INSERT INTO gnl_rol (id, nombre, descripcion) VALUES
    (1, 'Consultor', 'Gestiona las denuncias de su empresa: aprueba, rechaza, devuelve y asigna investigador. Pertenece a una empresa fija.');

IF NOT EXISTS (SELECT 1 FROM gnl_rol WHERE id = 2)
    INSERT INTO gnl_rol (id, nombre, descripcion) VALUES
    (2, 'Investigador', 'Investiga las denuncias que le asigna el Consultor de su empresa. Pertenece a una empresa fija.');

IF NOT EXISTS (SELECT 1 FROM gnl_rol WHERE id = 3)
    INSERT INTO gnl_rol (id, nombre, descripcion) VALUES
    (3, 'Jefe del proceso', 'Ve el flujo de TODAS las empresas para sacar indicadores globales. Solo lectura, no puede aprobar/rechazar/asignar. No tiene empresa asignada.');

IF NOT EXISTS (SELECT 1 FROM gnl_rol WHERE id = 4)
    INSERT INTO gnl_rol (id, nombre, descripcion) VALUES
    (4, 'Registrador', 'Monta denuncias recibidas por canales externos (correo, WhatsApp) a nombre de su empresa. La empresa de la denuncia se toma automáticamente de la suya.');

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_gnl_usuario_rol'
)
    ALTER TABLE gnl_usuario
        ADD CONSTRAINT FK_gnl_usuario_rol FOREIGN KEY (es_admin) REFERENCES gnl_rol(id);

PRINT 'Migración de roles aplicada correctamente.';
