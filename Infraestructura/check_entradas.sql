SELECT id, usuario_id, evento_id, asiento_id, estado, monto, fecha_compra, fecha_creacion 
FROM entradas.entradas 
ORDER BY fecha_creacion DESC 
LIMIT 5;
