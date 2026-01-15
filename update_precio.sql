-- Actualizar el precio base del evento
UPDATE "Eventos" 
SET "PrecioBase" = 50.00 
WHERE "Id" = '6e92cc27-e4df-4602-8819-1756a038a7ce';

-- Verificar la actualización
SELECT "Id", "Titulo", "PrecioBase", "Estado" 
FROM "Eventos" 
WHERE "Id" = '6e92cc27-e4df-4602-8819-1756a038a7ce';
