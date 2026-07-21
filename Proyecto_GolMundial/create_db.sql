ALTER DATABASE CHARACTER SET utf8mb4;


CREATE TABLE `fases` (
    `codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `fechas` varchar(100) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_fases` PRIMARY KEY (`codigo`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `grupos` (
    `codigo` varchar(1) CHARACTER SET utf8mb4 NOT NULL,
    `nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_grupos` PRIMARY KEY (`codigo`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `roles` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `descripcion` varchar(200) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_roles` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `sedes` (
    `id` int NOT NULL AUTO_INCREMENT,
    `nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ciudad` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `pais` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `capacidad_aprox` int NULL,
    CONSTRAINT `PK_sedes` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;


CREATE TABLE `selecciones` (
    `id` int NOT NULL AUTO_INCREMENT,
    `codigo_fifa` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `grupo` varchar(1) CHARACTER SET utf8mb4 NOT NULL,
    `confederacion` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `es_anfitrion` tinyint(1) NOT NULL,
    `clasificacion` varchar(150) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_selecciones` PRIMARY KEY (`id`),
    CONSTRAINT `FK_selecciones_grupos_grupo` FOREIGN KEY (`grupo`) REFERENCES `grupos` (`codigo`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `usuarios` (
    `id` int NOT NULL AUTO_INCREMENT,
    `username` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `rol_id` int NOT NULL,
    `password` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_usuarios` PRIMARY KEY (`id`),
    CONSTRAINT `FK_usuarios_roles_rol_id` FOREIGN KEY (`rol_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `partidos` (
    `id` int NOT NULL AUTO_INCREMENT,
    `fase_codigo` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `grupo_codigo` varchar(1) CHARACTER SET utf8mb4 NULL,
    `sede_id` int NOT NULL,
    `equipo_local_id` int NOT NULL,
    `equipo_visitante_id` int NOT NULL,
    `fecha_hora_utc` datetime(6) NOT NULL,
    `estado` varchar(20) CHARACTER SET utf8mb4 NULL,
    `goles_local` int NULL,
    `goles_visitante` int NULL,
    CONSTRAINT `PK_partidos` PRIMARY KEY (`id`),
    CONSTRAINT `FK_partidos_fases_fase_codigo` FOREIGN KEY (`fase_codigo`) REFERENCES `fases` (`codigo`) ON DELETE CASCADE,
    CONSTRAINT `FK_partidos_grupos_grupo_codigo` FOREIGN KEY (`grupo_codigo`) REFERENCES `grupos` (`codigo`),
    CONSTRAINT `FK_partidos_sedes_sede_id` FOREIGN KEY (`sede_id`) REFERENCES `sedes` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_partidos_selecciones_equipo_local_id` FOREIGN KEY (`equipo_local_id`) REFERENCES `selecciones` (`id`) ON DELETE CASCADE,
    CONSTRAINT `FK_partidos_selecciones_equipo_visitante_id` FOREIGN KEY (`equipo_visitante_id`) REFERENCES `selecciones` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `auditoria` (
    `id` int NOT NULL AUTO_INCREMENT,
    `usuario_id` int NOT NULL,
    `accion` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `entidad` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `entidad_id` int NULL,
    `fecha_hora_utc` datetime(6) NOT NULL,
    `detalles` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_auditoria` PRIMARY KEY (`id`),
    CONSTRAINT `FK_auditoria_usuarios_usuario_id` FOREIGN KEY (`usuario_id`) REFERENCES `usuarios` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE TABLE `calendario` (
    `id` int NOT NULL AUTO_INCREMENT,
    `partido_id` int NOT NULL,
    `fecha_hora_local` datetime(6) NOT NULL,
    `zona_horaria` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `observaciones` varchar(255) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_calendario` PRIMARY KEY (`id`),
    CONSTRAINT `FK_calendario_partidos_partido_id` FOREIGN KEY (`partido_id`) REFERENCES `partidos` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;


CREATE INDEX `IX_auditoria_usuario_id` ON `auditoria` (`usuario_id`);


CREATE INDEX `IX_calendario_partido_id` ON `calendario` (`partido_id`);


CREATE INDEX `IX_partidos_equipo_local_id` ON `partidos` (`equipo_local_id`);


CREATE INDEX `IX_partidos_equipo_visitante_id` ON `partidos` (`equipo_visitante_id`);


CREATE INDEX `IX_partidos_fase_codigo` ON `partidos` (`fase_codigo`);


CREATE INDEX `IX_partidos_grupo_codigo` ON `partidos` (`grupo_codigo`);


CREATE INDEX `IX_partidos_sede_id` ON `partidos` (`sede_id`);


CREATE INDEX `IX_selecciones_grupo` ON `selecciones` (`grupo`);


CREATE INDEX `IX_usuarios_rol_id` ON `usuarios` (`rol_id`);


