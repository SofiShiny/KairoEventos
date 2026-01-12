SELECT id, usuario_id, estado, fecha_creacion, fecha_compra 
FROM entradas.entradas 
ORDER BY fecha_creacion DESC
LIMIT 5;
