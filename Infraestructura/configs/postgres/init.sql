-- Script de inicialización de PostgreSQL
-- Crea las bases de datos para cada microservicio

-- Base de datos para el microservicio de Eventos
CREATE DATABASE kairo_eventos;

-- Base de datos para el microservicio de Asientos
CREATE DATABASE kairo_asientos;

-- Base de datos para Keycloak (Identity and Access Management)
CREATE DATABASE keycloak;

-- Base de datos para el microservicio de Servicios Extras
CREATE DATABASE kairo_servicios;

-- Base de datos para el microservicio de Recomendaciones
CREATE DATABASE kairo_recomendaciones;

-- Base de datos para el microservicio de Encuestas de Satisfacción
CREATE DATABASE kairo_encuestas;

-- Base de datos para el microservicio de Usuarios
CREATE DATABASE kairo_usuarios;

-- Base de datos para el microservicio de Entradas
CREATE DATABASE kairo_entradas;

-- Base de datos para el microservicio de Foros
CREATE DATABASE kairo_foros;

-- Base de datos para el microservicio de Marketing
CREATE DATABASE kairo_marketing;

-- Base de datos para el microservicio de Notificaciones
CREATE DATABASE kairo_notificaciones;

-- Base de datos para el microservicio de Pagos
CREATE DATABASE kairo_pagos;

-- Base de datos para el microservicio de Streaming
CREATE DATABASE kairo_streaming;

-- Base de datos para el microservicio de Reportes (si usa PostgreSQL)
CREATE DATABASE kairo_reportes;

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE 'Bases de datos creadas exitosamente';
END $$;
