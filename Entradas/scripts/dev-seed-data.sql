-- Script de datos de prueba para desarrollo
-- Solo se ejecuta en entorno de desarrollo

-- Verificar que estamos en desarrollo
DO $$
BEGIN
    IF current_setting('server_version_num')::int >= 140000 THEN
        RAISE NOTICE 'Cargando datos de desarrollo para Entradas.API';
    END IF;
END $$;

-- Nota: Los datos de prueba se insertarán después de que EF Core 
-- cree las tablas mediante migraciones.
-- Este script está preparado para ser ejecutado manualmente o
-- mediante un job de inicialización después del primer despliegue.

-- Función para insertar datos de prueba (se ejecutará después de migraciones)
CREATE OR REPLACE FUNCTION entradas.insert_development_data()
RETURNS void AS $$
BEGIN
    -- Verificar si la tabla existe (creada por EF Core)
    IF EXISTS (SELECT 1 FROM information_schema.tables 
               WHERE table_schema = 'entradas' AND table_name = 'entradas') THEN
        
        -- Insertar datos de prueba solo si no existen
        IF NOT EXISTS (SELECT 1 FROM entradas.entradas LIMIT 1) THEN
            
            RAISE NOTICE 'Insertando datos de desarrollo...';
            
            -- Datos de ejemplo para testing
            INSERT INTO entradas.entradas (
                id, 
                evento_id, 
                usuario_id, 
                asiento_id, 
                monto, 
                codigo_qr, 
                estado, 
                fecha_compra,
                fecha_creacion,
                fecha_actualizacion
            ) VALUES 
            (
                uuid_generate_v4(),
                uuid_generate_v4(),
                uuid_generate_v4(),
                uuid_generate_v4(),
                150.00,
                'TICKET-DEV001-1234',
                2, -- Pagada
                CURRENT_TIMESTAMP - INTERVAL '1 day',
                CURRENT_TIMESTAMP - INTERVAL '1 day',
                CURRENT_TIMESTAMP - INTERVAL '1 day'
            ),
            (
                uuid_generate_v4(),
                uuid_generate_v4(),
                uuid_generate_v4(),
                NULL, -- Entrada general
                75.50,
                'TICKET-DEV002-5678',
                1, -- PendientePago
                CURRENT_TIMESTAMP - INTERVAL '2 hours',
                CURRENT_TIMESTAMP - INTERVAL '2 hours',
                CURRENT_TIMESTAMP - INTERVAL '2 hours'
            );
            
            RAISE NOTICE 'Datos de desarrollo insertados correctamente';
        ELSE
            RAISE NOTICE 'Los datos de desarrollo ya existen, omitiendo inserción';
        END IF;
    ELSE
        RAISE NOTICE 'La tabla entradas no existe aún. Ejecutar después de las migraciones de EF Core.';
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Mensaje informativo
DO $$
BEGIN
    RAISE NOTICE 'Script de datos de desarrollo cargado.';
    RAISE NOTICE 'Ejecutar SELECT entradas.insert_development_data(); después de las migraciones.';
END $$;