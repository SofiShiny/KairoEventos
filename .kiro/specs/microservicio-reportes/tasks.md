# Plan de Implementación - Microservicio de Reportes

## Resumen

Este plan desglosa la implementación del microservicio de Reportes en tareas incrementales. Cada tarea construye sobre las anteriores y termina con código integrado y funcional. Todas las tareas son obligatorias para garantizar una cobertura completa de testing y calidad del código.

## Tareas

- [x] 1. Configurar estructura del proyecto y dependencias
  - Crear solución .NET 8 con arquitectura hexagonal (API, Aplicacion, Dominio, Infraestructura, Pruebas)
  - Agregar paquetes NuGet: MongoDB.Driver, MassTransit.RabbitMQ, Hangfire.Mongo, FsCheck.Xunit
  - Configurar docker-compose.yml con MongoDB, RabbitMQ y reportes-api
  - Crear Dockerfile para el microservicio
  - _Requisitos: 9.1, 9.2_

- [x] 2. Implementar modelos de dominio y contratos espejo
  - [x] 2.1 Crear modelos de lectura en Reportes.Dominio/ModelosLectura
    - Implementar `ReporteVentasDiarias`, `HistorialAsistencia`, `MetricasEvento`, `LogAuditoria`, `ReporteConsolidado`
    - Agregar atributos de MongoDB (BsonId, BsonElement)
    - _Requisitos: 3.2_
  
  - [x] 2.2 Crear contratos espejo en Reportes.Dominio/ContratosExternos
    - Implementar eventos con namespace `Eventos.Dominio.EventosDeDominio`: `EventoPublicadoEventoDominio`, `AsistenteRegistradoEventoDominio`, `EventoCanceladoEventoDominio`
    - Implementar eventos con namespace `Asientos.Dominio.EventosDominio`: `AsientoReservadoEventoDominio`, `AsientoLiberadoEventoDominio`, `MapaAsientosCreadoEventoDominio`
    - **CRÍTICO:** Usar namespaces originales de los microservicios fuente
    - _Requisitos: 2.1, 2.2, 2.3_
  
  - [x] 2.3 Escribir property test para modelos de dominio
    - **Propiedad 3: Invariante de disponibilidad de asientos**
    - **Valida: Requisitos 1.4, 6.5**

- [x] 3. Implementar capa de infraestructura con MongoDB
  - [x] 3.1 Crear MongoDbContext y configuración
    - Implementar `ReportesMongoDbContext` con colecciones tipadas
    - Configurar índices para optimización de consultas
    - Agregar health checks para MongoDB
    - _Requisitos: 3.1, 9.2_
  
  - [x] 3.2 Implementar repositorio de lectura
    - Crear interfaz `IRepositorioReportesLectura`
    - Implementar `RepositorioReportesLecturaMongo` con operaciones CRUD
    - Usar operaciones atómicas de MongoDB (UpdateOne, FindOneAndUpdate)
    - _Requisitos: 3.2, 3.4_
  
  - [x] 3.3 Escribir property test para persistencia
    - **Propiedad 1: Persistencia de eventos consumidos**
    - **Valida: Requisitos 1.1, 1.3, 3.2**
  
  - [x] 3.4 Escribir unit tests para repositorio
    - Test de conexión a MongoDB
    - Test de operaciones CRUD básicas
    - Test de manejo de errores cuando MongoDB no está disponible
    - _Requisitos: 3.5_

- [x] 4. Checkpoint - Verificar infraestructura
  - Ejecutar docker-compose up y verificar que MongoDB inicia correctamente
  - Ejecutar tests de integración con MongoDB
  - Asegurar que todos los tests pasan, preguntar al usuario si hay dudas

- [x] 5. Implementar consumidores de eventos con MassTransit
  - [x] 5.1 Configurar MassTransit con RabbitMQ
    - Agregar configuración de MassTransit en Program.cs
    - Configurar política de reintentos (3 intentos, backoff exponencial)
    - Configurar dead-letter queue para eventos fallidos
    - _Requisitos: 10.1, 10.2_
  
  - [x] 5.2 Implementar EventoPublicadoConsumer
    - Consumir evento y crear/actualizar `MetricasEvento`
    - Registrar operación en `LogAuditoria`
    - Manejar excepciones con logging apropiado
    - _Requisitos: 1.1, 1.5_
  
  - [x] 5.3 Implementar AsistenteRegistradoConsumer
    - Consumir evento e incrementar contador en `HistorialAsistencia`
    - Agregar asistente a la lista de registros
    - _Requisitos: 1.2_
  
  - [x] 5.4 Implementar AsientoReservadoConsumer
    - Consumir evento y actualizar `ReporteVentasDiarias`
    - Actualizar contadores de asientos en `HistorialAsistencia`
    - Recalcular porcentaje de ocupación
    - _Requisitos: 1.3, 6.4_
  
  - [x] 5.5 Implementar AsientoLiberadoConsumer
    - Consumir evento y actualizar disponibilidad de asientos
    - Mantener invariante: AsientosDisponibles + AsientosReservados = CapacidadTotal
    - _Requisitos: 1.4_
  
  - [x] 5.6 Escribir property test para consumidores
    - **Propiedad 2: Incremento atómico de contadores**
    - **Propiedad 4: Auditoría completa de operaciones**
    - **Valida: Requisitos 1.2, 1.5**
  
  - [x] 5.7 Escribir property test para deserialización
    - **Propiedad 5: Deserialización resiliente de eventos**
    - **Valida: Requisitos 2.4, 2.5**

- [x] 6. Checkpoint - Verificar consumidores
  - Levantar RabbitMQ y publicar eventos de prueba
  - Verificar que los consumidores procesan eventos correctamente
  - Verificar que los datos se persisten en MongoDB
  - Asegurar que todos los tests pasan, preguntar al usuario si hay dudas

- [x] 7. Implementar jobs de consolidación con Hangfire
  - [x] 7.1 Configurar Hangfire con MongoDB
    - Agregar configuración de Hangfire en Program.cs
    - Usar MongoDB como storage para Hangfire
    - Configurar dashboard de Hangfire (opcional)
    - _Requisitos: 4.5_
  
  - [x] 7.2 Implementar JobGenerarReportesConsolidados
    - Crear job que se ejecuta diariamente a las 2 AM
    - Agregar datos de múltiples colecciones (ventas, asistencia, métricas)
    - Calcular totales y promedios
    - Guardar en colección `ReportesConsolidados`
    - Registrar éxito/fallo en `LogAuditoria`
    - _Requisitos: 4.1, 4.2, 4.3, 4.4_
  
  - [x] 7.3 Escribir property test para consolidación
    - **Propiedad 6: Cálculo correcto de métricas consolidadas**
    - **Propiedad 7: Persistencia de reportes consolidados**
    - **Valida: Requisitos 4.1, 4.2, 4.3**
  
  - [x] 7.4 Escribir unit tests para job
    - Test de ejecución exitosa
    - Test de manejo de errores
    - Test de registro en auditoría
    - _Requisitos: 4.4_

- [x] 8. Implementar endpoints de API REST
  - [x] 8.1 Crear DTOs de respuesta
    - Implementar `ResumenVentasDto`, `AsistenciaEventoDto`, `LogAuditoriaDto`, `ConciliacionFinancieraDto`, `PaginacionDto`
    - _Requisitos: 5.1_
  
  - [x] 8.2 Implementar ReportesController
    - Crear controlador base con inyección de dependencias
    - Agregar atributos de routing y documentación Swagger
    - _Requisitos: 5.1_
  
  - [x] 8.3 Implementar endpoint GET /api/reportes/resumen-ventas
    - Validar parámetros de entrada (fechaInicio, fechaFin)
    - Consultar repositorio y mapear a DTO
    - Manejar errores con códigos HTTP apropiados
    - _Requisitos: 5.1, 5.2, 5.3, 5.4, 5.5_
  
  - [x] 8.4 Implementar endpoint GET /api/reportes/asistencia/{eventoId}
    - Validar eventoId
    - Retornar 404 si evento no existe
    - Calcular porcentaje de ocupación
    - _Requisitos: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [x] 8.5 Implementar endpoint GET /api/reportes/auditoria
    - Implementar filtros (fechaInicio, fechaFin, tipoOperacion)
    - Implementar paginación (pagina, tamañoPagina)
    - Ordenar por timestamp descendente
    - _Requisitos: 7.1, 7.2, 7.3, 7.4, 7.5_
  
  - [x] 8.6 Implementar endpoint GET /api/reportes/conciliacion-financiera
    - Filtrar por período
    - Calcular totales y desglose por categoría
    - Marcar discrepancias si existen
    - _Requisitos: 8.1, 8.2, 8.3, 8.4, 8.5_
  
  - [x] 8.7 Escribir property tests para endpoints
    - **Propiedad 8: Formato JSON válido en respuestas**
    - **Propiedad 9: Completitud de campos en resumen de ventas**
    - **Propiedad 10: Filtrado correcto por rango de fechas**
    - **Propiedad 11: Códigos HTTP apropiados para errores**
    - **Propiedad 12: Completitud de datos de asistencia**
    - **Propiedad 13: Cálculo correcto de porcentaje de ocupación**
    - **Propiedad 14: Ordenamiento descendente de logs**
    - **Propiedad 15: Filtrado correcto de logs de auditoría**
    - **Propiedad 16: Paginación correcta de resultados**
    - **Propiedad 17: Completitud de campos en logs**
    - **Propiedad 18: Completitud de datos de conciliación**
    - **Propiedad 19: Marcado de discrepancias financieras**
    - **Propiedad 20: Esquema JSON válido para exportación**
    - **Valida: Requisitos 5.1-5.5, 6.1-6.5, 7.1-7.5, 8.1-8.5**
  
  - [x] 8.8 Escribir unit tests para endpoints
    - Test de validación de parámetros inválidos (retorna 400)
    - Test de evento no encontrado (retorna 404)
    - Test de error interno (retorna 500)
    - Test de respuesta exitosa con datos válidos
    - _Requisitos: 5.4, 5.5, 6.3, 10.4, 10.5_

- [x] 9. Checkpoint - Verificar API completa
  - Ejecutar docker-compose up con todos los servicios
  - Probar endpoints con Postman/curl
  - Verificar que los datos se retornan correctamente
  - Asegurar que todos los tests pasan, preguntar al usuario si hay dudas

- [x] 10. Implementar manejo de errores y resiliencia
  - [x] 10.1 Agregar middleware de manejo de excepciones global
    - Capturar excepciones no controladas
    - Retornar respuestas JSON consistentes
    - Registrar errores en logs
    - _Requisitos: 10.5_
  
  - [x] 10.2 Implementar health checks
    - Agregar health check para MongoDB
    - Agregar health check para RabbitMQ
    - Exponer endpoint /health con estado de servicios
    - _Requisitos: 9.5_
  
  - [x] 10.3 Configurar logging estructurado con Serilog
    - Configurar Serilog para escribir a consola y MongoDB
    - Agregar contexto de correlación para trazabilidad
    - _Requisitos: 10.5_
  
  - [x] 10.4 Escribir property test para manejo de errores
    - **Propiedad 21: Movimiento a cola de errores tras reintentos**
    - **Valida: Requisitos 10.2**
  
  - [x] 10.5 Escribir unit tests para resiliencia
    - Test de reintentos con backoff exponencial
    - Test de movimiento a dead-letter queue
    - Test de encolamiento cuando MongoDB no está disponible
    - _Requisitos: 10.1, 10.3_

- [x] 11. Integración y pruebas end-to-end
  - [x] 11.1 Crear script de inicialización de datos de prueba
    - Publicar eventos de prueba en RabbitMQ
    - Verificar que los consumidores procesan correctamente
    - Verificar que los endpoints retornan datos esperados
    - _Requisitos: 1.1, 1.2, 1.3_
  
  - [x] 11.2 Escribir tests de integración end-to-end
    - Test de flujo completo: evento → consumidor → MongoDB → API
    - Test de consolidación nocturna
    - Test de manejo de errores en escenarios reales
    - _Requisitos: 1.1-1.5, 4.1-4.3_

- [x] 12. Documentación y finalización
  - [x] 12.1 Generar documentación de API con Swagger
    - Agregar descripciones a endpoints
    - Agregar ejemplos de request/response
    - Documentar códigos de error
  
  - [x] 12.2 Crear README.md del proyecto
    - Instrucciones de instalación y ejecución
    - Descripción de arquitectura
    - Guía de desarrollo y testing
  
  - [x] 12.3 Crear script de deployment
    - Script para build de imágenes Docker
    - Script para despliegue en ambiente de producción
    - Configuración de variables de entorno

- [x] 13. Checkpoint final - Verificación completa
  - Ejecutar suite completa de tests (unit + property + integration)
  - Verificar cobertura de código (objetivo: >80%)
  - Ejecutar docker-compose y probar todos los endpoints
  - Verificar que el job de consolidación se ejecuta correctamente
  - Revisar logs y health checks
  - Asegurar que todo funciona correctamente, preguntar al usuario si hay dudas

## Notas

- Todas las tareas son obligatorias para garantizar cobertura completa de testing
- Cada tarea referencia los requisitos específicos que implementa
- Los checkpoints permiten validación incremental y feedback del usuario
- Los property tests validan propiedades universales con 100+ iteraciones cada uno
- Los unit tests validan casos específicos y edge cases
- La implementación sigue el orden: Dominio → Infraestructura → Aplicación → API
- Todas las tareas terminan con código integrado y funcional

## Resumen de Testing

- **Property-Based Tests:** 21 propiedades distribuidas en las tareas
- **Unit Tests:** ~30 tests para casos específicos
- **Integration Tests:** ~10 tests end-to-end
- **Total de Casos:** >2,500 (considerando 100 iteraciones por property test)
