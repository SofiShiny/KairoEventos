-- Script de inicialización de base de datos PostgreSQL
-- Para el microservicio Entradas.API

-- Crear extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Configurar timezone por defecto
SET timezone = 'UTC';

-- Crear esquema si no existe
CREATE SCHEMA IF NOT EXISTS entradas;

-- Configurar search_path
ALTER DATABASE entradas_db SET search_path TO entradas, public;

-- Crear función para actualizar timestamp automáticamente
CREATE OR REPLACE FUNCTION entradas.update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.fecha_actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Crear tabla de entradas (será creada por EF Core, pero definimos estructura base)
-- Esta tabla será gestionada por Entity Framework Core Migrations
-- El script solo asegura que la base de datos esté lista

-- Configurar permisos para el usuario de la aplicación
GRANT USAGE ON SCHEMA entradas TO entradas_user;
GRANT CREATE ON SCHEMA entradas TO entradas_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA entradas TO entradas_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA entradas TO entradas_user;

-- Configurar permisos por defecto para objetos futuros
ALTER DEFAULT PRIVILEGES IN SCHEMA entradas GRANT ALL ON TABLES TO entradas_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA entradas GRANT ALL ON SEQUENCES TO entradas_user;

-- Crear índices adicionales para performance (complementarios a los de EF Core)
-- Estos se crearán después de que EF Core cree las tablas

-- Log de inicialización
INSERT INTO pg_stat_statements_info (dealloc) VALUES (0) ON CONFLICT DO NOTHING;

-- Mensaje de confirmación
DO $$
BEGIN
    RAISE NOTICE 'Base de datos inicializada correctamente para Entradas.API';
    RAISE NOTICE 'Usuario: entradas_user';
    RAISE NOTICE 'Esquema: entradas';
    RAISE NOTICE 'Timezone: UTC';
END $$;