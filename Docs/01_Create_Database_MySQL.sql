-- 1. Crear la base de datos
CREATE DATABASE IF NOT EXISTS Api_GolMundial;
USE Api_GolMundial;

-- 2. Crear las tablas base

-- Tabla de Grupos
CREATE TABLE IF NOT EXISTS grupos (
    codigo VARCHAR(1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL
);

-- Tabla de Fases
CREATE TABLE IF NOT EXISTS fases (
    codigo VARCHAR(20) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    fechas VARCHAR(100)
);

-- Tabla de Sedes
CREATE TABLE IF NOT EXISTS sedes (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(150) NOT NULL,
    ciudad VARCHAR(100) NOT NULL,
    pais VARCHAR(100) NOT NULL,
    capacidad_aprox INT
);

-- Tabla de Grupos (Datos Iniciales)
INSERT INTO grupos (codigo, nombre) VALUES 
('A', 'Grupo A'), ('B', 'Grupo B'), ('C', 'Grupo C'), ('D', 'Grupo D'), 
('E', 'Grupo E'), ('F', 'Grupo F'), ('G', 'Grupo G'), ('H', 'Grupo H'), 
('I', 'Grupo I'), ('J', 'Grupo J'), ('K', 'Grupo K'), ('L', 'Grupo L')
ON DUPLICATE KEY UPDATE nombre=nombre;

-- Tabla de Selecciones
CREATE TABLE IF NOT EXISTS selecciones (
    id INT PRIMARY KEY AUTO_INCREMENT,
    codigo_fifa VARCHAR(3) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    grupo VARCHAR(1) NOT NULL,
    confederacion VARCHAR(50) NOT NULL,
    es_anfitrion BOOLEAN DEFAULT FALSE,
    clasificacion VARCHAR(150),
    FOREIGN KEY (grupo) REFERENCES grupos(codigo)
);

-- Tabla de Roles
CREATE TABLE IF NOT EXISTS roles (
    id INT PRIMARY KEY AUTO_INCREMENT,
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(200)
);

INSERT INTO roles (id, nombre, descripcion) VALUES
(1, 'ADMINISTRADOR', 'Gestiona torneo, resultados, cuotas y usuarios'),
(2, 'USUARIO', 'Consulta información y realiza predicciones con UTNGolCoin'),
(3, 'INVITADO', 'Solo consulta calendario, estadísticas y posiciones')
ON DUPLICATE KEY UPDATE nombre=nombre;

-- Tabla de Usuarios
CREATE TABLE IF NOT EXISTS usuarios (
    id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    rol_id INT NOT NULL,
    password VARCHAR(255) NOT NULL,
    FOREIGN KEY (rol_id) REFERENCES roles(id)
);

INSERT INTO usuarios (id, username, nombre, rol_id, password) VALUES
(1, 'admin', 'Administrador del Torneo', 1, '$2a$12$R.P96/9G.F4fJvB97X8vCOuYhQ/h4eI5R.UuX7yKqW3lMh.K2Jq7e') -- Contraseña encriptada para admin
ON DUPLICATE KEY UPDATE username=username;

-- Tabla de Partidos (Estructura sugerida)
CREATE TABLE IF NOT EXISTS partidos (
    id INT PRIMARY KEY AUTO_INCREMENT,
    fase_codigo VARCHAR(20) NOT NULL,
    grupo_codigo VARCHAR(1), -- Puede ser nulo si no es fase de grupos
    sede_id INT NOT NULL,
    equipo_local_id INT NOT NULL,
    equipo_visitante_id INT NOT NULL,
    fecha_hora_utc DATETIME NOT NULL,
    estado VARCHAR(20) DEFAULT 'PROGRAMADO', -- PROGRAMADO, EN_JUEGO, FINALIZADO
    goles_local INT DEFAULT NULL,
    goles_visitante INT DEFAULT NULL,
    FOREIGN KEY (fase_codigo) REFERENCES fases(codigo),
    FOREIGN KEY (grupo_codigo) REFERENCES grupos(codigo),
    FOREIGN KEY (sede_id) REFERENCES sedes(id),
    FOREIGN KEY (equipo_local_id) REFERENCES selecciones(id),
    FOREIGN KEY (equipo_visitante_id) REFERENCES selecciones(id)
);

-- Tabla de Auditoria
CREATE TABLE IF NOT EXISTS auditoria (
    id INT PRIMARY KEY AUTO_INCREMENT,
    usuario_id INT NOT NULL,
    accion VARCHAR(100) NOT NULL,
    entidad VARCHAR(50) NOT NULL,
    entidad_id INT,
    fecha_hora_utc DATETIME NOT NULL,
    detalles TEXT,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
);

-- Tabla de Calendario
CREATE TABLE IF NOT EXISTS calendario (
    id INT PRIMARY KEY AUTO_INCREMENT,
    partido_id INT NOT NULL,
    fecha_hora_local DATETIME NOT NULL,
    zona_horaria VARCHAR(50) NOT NULL,
    observaciones VARCHAR(255),
    FOREIGN KEY (partido_id) REFERENCES partidos(id) ON DELETE CASCADE
);

