ALTER TABLE "entradas"."entradas" 
DROP COLUMN IF EXISTS "cupones_aplicados",
DROP COLUMN IF EXISTS "monto_descuento",
DROP COLUMN IF EXISTS "monto_original",
DROP COLUMN IF EXISTS "fecha_evento",
DROP COLUMN IF EXISTS "fila",
DROP COLUMN IF EXISTS "imagen_evento_url",
DROP COLUMN IF EXISTS "nombre_sector",
DROP COLUMN IF EXISTS "numero_asiento",
DROP COLUMN IF EXISTS "titulo_evento",
DROP COLUMN IF EXISTS "categoria";

DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" IN ('20260103041825_MigrationInitial', '20260108172602_AgregarCamposSnapshot', '20260108173035_UpdateEntradaSnapshotFields');
