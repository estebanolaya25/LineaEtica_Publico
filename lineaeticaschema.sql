USE [LineaEtica]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_campo](
	[id] [int] IDENTITY(325,1) NOT NULL,
	[nombre] [varchar](250) NULL,
	[descripcion] [varchar](max) NULL,
	[ayuda] [varchar](250) NULL,
	[obligatorio] [int] NULL,
	[activo] [int] NULL,
	[orden] [int] NULL,
	[idform_seccion_fk] [int] NULL,
	[idform_tipo_campo_fk] [int] NULL,
	[idgnl_tipo_parametro_fk] [int] NULL,
	[idgnl_tipo_parametro_hijo_fk] [int] NULL,
	[col] [int] NULL,
	[id_campo] [int] NULL,
	[cant_adjuntos] [int] NULL,
	[ws] [varchar](45) NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
	[funcion_javascript] [varchar](250) NULL,
 CONSTRAINT [PK_form_campo_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_campo_respuesta](
	[id] [int] IDENTITY(682,1) NOT NULL,
	[idform_formulario_fk] [int] NULL,
	[idform_campo_fk] [int] NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[id_opcion_respuesta] [int] NULL,
	[texto_respuesta] [varchar](500) NULL,
	[rechazado] [int] NULL,
	[usuario] [varchar](45) NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
 CONSTRAINT [PK_form_campo_respuesta_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_campo_tabla_respuesta](
	[id] [int] IDENTITY(771,1) NOT NULL,
	[idform_formulario_fk] [int] NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[id_campo_tabla] [int] NULL,
	[idform_campo_fk] [int] NULL,
	[id_opcion_respuesta] [int] NULL,
	[texto_respuesta] [varchar](500) NULL,
	[usuario] [varchar](45) NULL,
	[id_grupo] [int] NULL,
	[rechazado] [int] NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
 CONSTRAINT [PK_form_campo_tabla_respuesta_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_formulario](
	[id] [int] IDENTITY(4,1) NOT NULL,
	[nombre] [varchar](45) NULL,
	[descripcion] [varchar](max) NULL,
	[activo] [int] NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
 CONSTRAINT [PK_form_formulario_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_formulario_usuario](
	[id] [int] IDENTITY(52,1) NOT NULL,
	[idform_formulario_fk] [int] NULL,
	[usuario] [varchar](45) NULL,
	[idgnl_estado_fk] [int] NULL,
	[observacion] [varchar](max) NULL,
	[consecutivo] [int] NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
	[fecha_cierre] [datetime] NULL,
 CONSTRAINT [PK_form_formulario_usuario_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_seccion](
	[id] [int] IDENTITY(25,1) NOT NULL,
	[nombre] [varchar](300) NULL,
	[descripcion] [varchar](max) NULL,
	[idform_formulario_fk] [int] NULL,
	[mostrar_nombre_seccion] [int] NULL,
	[orden] [int] NULL,
	[activo] [int] NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
 CONSTRAINT [PK_form_seccion_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[form_tipo_campo](
	[id] [int] IDENTITY(10,1) NOT NULL,
	[nombre] [varchar](250) NULL,
	[descripcion] [varchar](max) NULL,
	[activo] [int] NULL,
 CONSTRAINT [PK_form_tipo_campo_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_archivo](
	[id] [int] IDENTITY(110,1) NOT NULL,
	[nombre_tabla] [varchar](45) NOT NULL,
	[id_tabla] [varchar](45) NOT NULL,
	[id_valor] [int] NOT NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[idform_campo_fk] [int] NULL,
	[campo] [varchar](45) NOT NULL,
	[nombre_archivo] [varchar](500) NULL,
	[archivo_blob] [varbinary](max) NULL,
	[tipo_archivo] [varchar](100) NULL,
	[fecha_creacion] [datetime] NOT NULL,
 CONSTRAINT [PK_gnl_archivo_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_archivo1](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre_tabla] [varchar](45) NOT NULL,
	[id_tabla] [varchar](45) NOT NULL,
	[id_valor] [int] NOT NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[idform_campo_fk] [int] NULL,
	[campo] [varchar](45) NOT NULL,
	[nombre_archivo] [varchar](500) NULL,
	[archivo_blob] [nvarchar](max) NULL,
	[tipo_archivo] [varchar](100) NULL,
	[fecha_creacion] [datetime] NOT NULL,
 CONSTRAINT [PK_gnl_archivo1_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_clave_config](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[dias_vigencia_clave] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_estado](
	[id] [int] IDENTITY(6,1) NOT NULL,
	[idgnl_modulo_fk] [int] NULL,
	[codigo] [varchar](4) NULL,
	[nombre] [varchar](45) NULL,
	[activo] [int] NULL,
 CONSTRAINT [PK_gnl_estado_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_Medio](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[idgnl_parametro_fk] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_parametro](
	[id] [int] IDENTITY(1248,1) NOT NULL,
	[codigo] [varchar](20) NULL,
	[nombre] [varchar](200) NULL,
	[descripcion] [varchar](max) NULL,
	[aplica_otro] [int] NULL,
	[depende_de] [int] NULL,
	[idgnl_tipo_parametro_fk] [int] NULL,
	[activo] [int] NULL,
 CONSTRAINT [PK_gnl_parametro_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_perfil](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](45) NULL,
	[activo] [int] NULL,
	[menu_inicio] [varchar](45) NULL,
 CONSTRAINT [PK_gnl_perfil_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_tipo_parametro](
	[id] [int] IDENTITY(26,1) NOT NULL,
	[codigo] [varchar](45) NOT NULL,
	[nombre] [varchar](45) NOT NULL,
	[descripcion] [varchar](400) NULL,
	[activo] [int] NOT NULL,
	[parametro_hijo] [int] NULL,
 CONSTRAINT [PK_gnl_tipo_parametro_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gnl_usuario](
	[id] [int] IDENTITY(15,1) NOT NULL,
	[usuario] [varchar](45) NULL,
	[nombre] [varchar](150) NULL,
	[tipo_documento] [int] NULL,
	[numero_documento] [varchar](20) NULL,
	[correo] [varchar](150) NULL,
	[clave] [varchar](200) NULL,
	[es_admin] [int] NULL,
	[activo] [int] NULL,
	[fecha_creacion] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
 CONSTRAINT [PK_gnl_usuario_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[inspektor_respuesta_ws](
	[id] [int] IDENTITY(46,1) NOT NULL,
	[idform_formulario_fk] [int] NULL,
	[idform_formulario_usuario_fk] [int] NULL,
	[id_tipo] [int] NULL,
	[tipo] [varchar](45) NULL,
	[numero_documento] [varchar](45) NULL,
	[nombre] [varchar](150) NULL,
	[numero_consulta] [varchar](20) NULL,
	[coincidencias] [varchar](10) NULL,
	[fecha_creacion] [datetime] NULL,
 CONSTRAINT [PK_inspektor_respuesta_ws_id] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [ayuda]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((1)) FOR [obligatorio]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((0)) FOR [orden]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [idform_seccion_fk]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [idform_tipo_campo_fk]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((0)) FOR [idgnl_tipo_parametro_fk]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((0)) FOR [idgnl_tipo_parametro_hijo_fk]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((3)) FOR [col]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((0)) FOR [id_campo]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT ((0)) FOR [cant_adjuntos]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [ws]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_campo] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [idform_formulario_fk]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [idform_campo_fk]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [idform_formulario_usuario_fk]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [id_opcion_respuesta]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [texto_respuesta]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT ((0)) FOR [rechazado]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [usuario]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_campo_respuesta] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [idform_formulario_fk]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [idform_formulario_usuario_fk]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [id_campo_tabla]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [idform_campo_fk]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [id_opcion_respuesta]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [texto_respuesta]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [usuario]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT ((0)) FOR [id_grupo]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT ((0)) FOR [rechazado]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_campo_tabla_respuesta] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_formulario] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[form_formulario] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[form_formulario] ADD  DEFAULT (NULL) FOR [activo]
GO
ALTER TABLE [dbo].[form_formulario] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_formulario] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [idform_formulario_fk]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [usuario]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [idgnl_estado_fk]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [observacion]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [consecutivo]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_formulario_usuario] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (NULL) FOR [idform_formulario_fk]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (NULL) FOR [mostrar_nombre_seccion]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT ((0)) FOR [orden]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[form_seccion] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[form_tipo_campo] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[form_tipo_campo] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[form_tipo_campo] ADD  DEFAULT ((0)) FOR [activo]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT ((0)) FOR [idform_formulario_usuario_fk]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT ((0)) FOR [idform_campo_fk]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT (NULL) FOR [nombre_archivo]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT (NULL) FOR [archivo_blob]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT (NULL) FOR [tipo_archivo]
GO
ALTER TABLE [dbo].[gnl_archivo] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT ((0)) FOR [idform_formulario_usuario_fk]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT ((0)) FOR [idform_campo_fk]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT (NULL) FOR [nombre_archivo]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT (NULL) FOR [archivo_blob]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT (NULL) FOR [tipo_archivo]
GO
ALTER TABLE [dbo].[gnl_archivo1] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[gnl_estado] ADD  DEFAULT ((0)) FOR [idgnl_modulo_fk]
GO
ALTER TABLE [dbo].[gnl_estado] ADD  DEFAULT (NULL) FOR [codigo]
GO
ALTER TABLE [dbo].[gnl_estado] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[gnl_estado] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT (NULL) FOR [codigo]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT ((0)) FOR [aplica_otro]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT ((0)) FOR [depende_de]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT (NULL) FOR [idgnl_tipo_parametro_fk]
GO
ALTER TABLE [dbo].[gnl_parametro] ADD  DEFAULT (NULL) FOR [activo]
GO
ALTER TABLE [dbo].[gnl_perfil] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[gnl_perfil] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[gnl_perfil] ADD  DEFAULT (NULL) FOR [menu_inicio]
GO
ALTER TABLE [dbo].[gnl_tipo_parametro] ADD  DEFAULT (NULL) FOR [descripcion]
GO
ALTER TABLE [dbo].[gnl_tipo_parametro] ADD  DEFAULT ((0)) FOR [parametro_hijo]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [usuario]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [tipo_documento]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [numero_documento]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [correo]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [clave]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT ((0)) FOR [es_admin]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[gnl_usuario] ADD  DEFAULT (NULL) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [idform_formulario_fk]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [idform_formulario_usuario_fk]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [id_tipo]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [tipo]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [numero_documento]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [nombre]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [numero_consulta]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (NULL) FOR [coincidencias]
GO
ALTER TABLE [dbo].[inspektor_respuesta_ws] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_campo' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_campo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_campo_respuesta' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_campo_respuesta'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_campo_tabla_respuesta' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_campo_tabla_respuesta'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_formulario' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_formulario'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_formulario_usuario' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_formulario_usuario'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_seccion' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_seccion'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.form_tipo_campo' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'form_tipo_campo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_archivo' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_archivo'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios2.gnl_archivo1' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_archivo1'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_estado' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_estado'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_parametro' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_parametro'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_perfil' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_perfil'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_tipo_parametro' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_tipo_parametro'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.gnl_usuario' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'gnl_usuario'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_SSMA_SOURCE', @value=N'formularios.inspektor_respuesta_ws' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'inspektor_respuesta_ws'
GO
