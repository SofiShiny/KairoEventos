SELECT 
    id::text as entrada_id,
    usuario_id::text,
    evento_id::text,
    estado,
    CASE estado
        WHEN 1 THEN 'PendientePago'
        WHEN 2 THEN 'Pagada'
        WHEN 3 THEN 'Cancelada'
        WHEN 4 THEN 'Usada'
        ELSE 'Desconocido'
    END as estado_nombre,
    monto,
    codigo_qr,
    titulo_evento,
    fecha_evento,
    nombre_sector,
    fila,
    numero_asiento,
    fecha_compra,
    fecha_creacion
FROM entradas.entradas 
ORDER BY fecha_creacion DESC
LIMIT 3;
