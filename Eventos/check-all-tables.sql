SELECT 'Eventos' as tabla, COUNT(*) as total FROM "Eventos"
UNION ALL
SELECT 'Asistentes' as tabla, COUNT(*) as total FROM "Asistentes";

SELECT "Id", "Estado", "Titulo", "CreadoEn" FROM "Eventos" ORDER BY "CreadoEn" DESC LIMIT 5;
