CREATE TABLE fases (
    codigo varchar(20) NOT NULL,
    nombre varchar(100) NOT NULL,
    fechas varchar(100) NULL,
    CONSTRAINT PK_fases PRIMARY KEY (codigo)
);

CREATE TABLE grupos (
    codigo varchar(1) NOT NULL,
    nombre varchar(50) NOT NULL,
    CONSTRAINT PK_grupos PRIMARY KEY (codigo)
);

CREATE TABLE roles (
    id serial NOT NULL,
    nombre varchar(50) NOT NULL,
    descripcion varchar(200) NULL,
    CONSTRAINT PK_roles PRIMARY KEY (id)
);

CREATE TABLE sedes (
    id serial NOT NULL,
    nombre varchar(150) NOT NULL,
    ciudad varchar(100) NOT NULL,
    pais varchar(100) NOT NULL,
    capacidad_aprox int NULL,
    CONSTRAINT PK_sedes PRIMARY KEY (id)
);

CREATE TABLE selecciones (
    id serial NOT NULL,
    codigo_fifa varchar(3) NOT NULL,
    nombre varchar(100) NOT NULL,
    grupo varchar(1) NOT NULL,
    confederacion varchar(50) NOT NULL,
    es_anfitrion boolean NOT NULL,
    clasificacion varchar(150) NULL,
    CONSTRAINT PK_selecciones PRIMARY KEY (id),
    CONSTRAINT FK_selecciones_grupos_grupo FOREIGN KEY (grupo) REFERENCES grupos (codigo) ON DELETE CASCADE
);

CREATE TABLE usuarios (
    id serial NOT NULL,
    username varchar(50) NOT NULL,
    nombre varchar(100) NOT NULL,
    rol_id int NOT NULL,
    password varchar(255) NOT NULL,
    CONSTRAINT PK_usuarios PRIMARY KEY (id),
    CONSTRAINT FK_usuarios_roles_rol_id FOREIGN KEY (rol_id) REFERENCES roles (id) ON DELETE CASCADE
);

CREATE TABLE partidos (
    id serial NOT NULL,
    fase_codigo varchar(20) NOT NULL,
    grupo_codigo varchar(1) NULL,
    sede_id int NOT NULL,
    equipo_local_id int NOT NULL,
    equipo_visitante_id int NOT NULL,
    fecha_hora_utc timestamp NOT NULL,
    estado varchar(20) NULL,
    goles_local int NULL,
    goles_visitante int NULL,
    CONSTRAINT PK_partidos PRIMARY KEY (id),
    CONSTRAINT FK_partidos_fases_fase_codigo FOREIGN KEY (fase_codigo) REFERENCES fases (codigo) ON DELETE CASCADE,
    CONSTRAINT FK_partidos_grupos_grupo_codigo FOREIGN KEY (grupo_codigo) REFERENCES grupos (codigo),
    CONSTRAINT FK_partidos_sedes_sede_id FOREIGN KEY (sede_id) REFERENCES sedes (id) ON DELETE CASCADE,
    CONSTRAINT FK_partidos_selecciones_equipo_local_id FOREIGN KEY (equipo_local_id) REFERENCES selecciones (id) ON DELETE CASCADE,
    CONSTRAINT FK_partidos_selecciones_equipo_visitante_id FOREIGN KEY (equipo_visitante_id) REFERENCES selecciones (id) ON DELETE CASCADE
);

CREATE TABLE auditoria (
    id serial NOT NULL,
    usuario_id int NOT NULL,
    accion varchar(100) NOT NULL,
    entidad varchar(50) NOT NULL,
    entidad_id int NULL,
    fecha_hora_utc timestamp NOT NULL,
    detalles text NULL,
    CONSTRAINT PK_auditoria PRIMARY KEY (id),
    CONSTRAINT FK_auditoria_usuarios_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE
);

CREATE TABLE calendario (
    id serial NOT NULL,
    partido_id int NOT NULL,
    fecha_hora_local timestamp NOT NULL,
    zona_horaria varchar(50) NOT NULL,
    observaciones varchar(255) NULL,
    CONSTRAINT PK_calendario PRIMARY KEY (id),
    CONSTRAINT FK_calendario_partidos_partido_id FOREIGN KEY (partido_id) REFERENCES partidos (id) ON DELETE CASCADE
);

CREATE INDEX IX_auditoria_usuario_id ON auditoria (usuario_id);
CREATE INDEX IX_calendario_partido_id ON calendario (partido_id);
CREATE INDEX IX_partidos_equipo_local_id ON partidos (equipo_local_id);
CREATE INDEX IX_partidos_equipo_visitante_id ON partidos (equipo_visitante_id);
CREATE INDEX IX_partidos_fase_codigo ON partidos (fase_codigo);
CREATE INDEX IX_partidos_grupo_codigo ON partidos (grupo_codigo);
CREATE INDEX IX_partidos_sede_id ON partidos (sede_id);
CREATE INDEX IX_selecciones_grupo ON selecciones (grupo);
CREATE INDEX IX_usuarios_rol_id ON usuarios (rol_id);
