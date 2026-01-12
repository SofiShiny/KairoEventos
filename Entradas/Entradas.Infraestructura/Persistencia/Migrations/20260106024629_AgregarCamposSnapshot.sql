-- =====================================================
-- MIGRACIÓN: AgregarCamposSnapshot
-- Fecha: 2026-01-06 02:46:29
-- Descripción: Agrega campos snapshot (desnormalización)
--              para mejorar el rendimiento del historial
-- =====================================================

-- Agregar columnas snapshot a la tabla entradas
ALTER TABLE entradas.entradas 
ADD COLUMN IF NOT EXISTS titulo_evento VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS imagen_evento_url VARCHAR(500) NULL,
ADD COLUMN IF NOT EXISTS fecha_evento TIMESTAMP WITH TIME ZONE NULL,
ADD COLUMN IF NOT EXISTS nombre_sector VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS fila VARCHAR(10) NULL,
ADD COLUMN IF NOT EXISTS numero_asiento INTEGER NULL;

-- Comentarios para documentación
COMMENT ON COLUMN entradas.entradas.titulo_evento IS 'Snapshot del título del evento al momento de la compra';
COMMENT ON COLUMN entradas.entradas.imagen_evento_url IS 'Snapshot de la URL de la imagen del evento';
COMMENT ON COLUMN entradas.entradas.fecha_evento IS 'Snapshot de la fecha del evento';
COMMENT ON COLUMN entradas.entradas.nombre_sector IS 'Snapshot del nombre del sector del asiento';
COMMENT ON COLUMN entradas.entradas.fila IS 'Snapshot de la fila del asiento';
COMMENT ON COLUMN entradas.entradas.numero_asiento IS 'Snapshot del número del asiento';

-- Verificar que las columnas se agregaron correctamente
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_schema = 'entradas' 
  AND table_name = 'entradas'
  AND column_name IN ('titulo_evento', 'imagen_evento_url', 'fecha_evento', 
                      'nombre_sector', 'fila', 'numero_asiento')
ORDER BY ordinal_position;

-- =====================================================
-- ROLLBACK (en caso de necesitar revertir)
-- =====================================================
-- ALTER TABLE entradas.entradas 
-- DROP COLUMN IF EXISTS titulo_evento,
-- DROP COLUMN IF EXISTS imagen_evento_url,
-- DROP COLUMN IF EXISTS fecha_evento,
-- DROP COLUMN IF EXISTS nombre_sector,
-- DROP COLUMN IF EXISTS fila,
-- DROP COLUMN IF EXISTS numero_asiento;
