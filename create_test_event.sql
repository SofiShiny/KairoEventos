-- Script para crear evento de prueba
INSERT INTO "Eventos" (
    "Id", 
    "Titulo", 
    "Descripcion", 
    "FechaInicio", 
    "FechaFin", 
    "MaximoAsistentes", 
    "OrganizadorId", 
    "Categoria", 
    "Estado", 
    "PrecioBase",
    "NombreLugar",
    "Direccion",
    "Ciudad",
    "Pais"
) VALUES (
    '6e92cc27-e4df-4602-8819-1756a038a7ce'::uuid,
    'Concierto de Rock 2026',
    'Gran concierto de rock en vivo',
    '2026-02-15 20:00:00'::timestamp,
    '2026-02-15 23:00:00'::timestamp,
    1000,
    'org-001',
    'Musica',
    'Publicado',
    50.00,
    'Estadio Nacional',
    'Av. Grecia 2001',
    'Santiago',
    'Chile'
);
