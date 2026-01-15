-- Script para permitir entradas gratuitas
-- Eliminar la restricción antigua
ALTER TABLE entradas DROP CONSTRAINT IF EXISTS ck_entradas_monto_positivo;

-- Agregar nueva restricción que permite monto >= 0
ALTER TABLE entradas ADD CONSTRAINT ck_entradas_monto_no_negativo CHECK (monto >= 0);

-- Verificar las restricciones
SELECT conname, pg_get_constraintdef(oid) 
FROM pg_constraint 
WHERE conrelid = 'entradas'::regclass AND contype = 'c';
