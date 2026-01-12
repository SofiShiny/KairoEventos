# Documento de Requisitos - Microservicio de Reportes

## Introducción

El microservicio de Reportes es un componente de solo lectura (Read Model) que consume eventos de dominio de otros microservicios (Eventos y Asientos) para generar reportes analíticos y métricas en tiempo real. Utiliza MongoDB como base de datos de lectura, RabbitMQ con MassTransit para mensajería asíncrona, y Hangfire para trabajos programados de consolidación.

## Glosario

- **Sistema_Reportes**: El microservicio completo de generación de reportes
- **Modelo_Lectura**: Colección de MongoDB optimizada para consultas de reportes
- **Consumidor_Eventos**: Componente que escucha eventos de RabbitMQ
- **Contrato_Espejo**: Definición local de un evento externo con namespace original
- **Job_Consolidacion**: Tarea programada de Hangfire para generar métricas agregadas
- **Endpoint_Reporte**: API REST que retorna datos en formato JSON
- **MongoDB**: Base de datos NoSQL para almacenamiento de modelos de lectura
- **MassTransit**: Framework de mensajería para consumir eventos
- **Hangfire**: Framework para trabajos en segundo plano

## Requisitos

### Requisito 1: Consumo de Eventos de Dominio

**User Story:** Como Sistema_Reportes, quiero consumir eventos de dominio de otros microservicios, para mantener actualizados los modelos de lectura en tiempo real.

#### Criterios de Aceptación

1. WHEN un evento `EventoPublicadoEventoDominio` es recibido, THEN THE Sistema_Reportes SHALL crear o actualizar el registro en la colección `MetricasEvento`
2. WHEN un evento `AsistenteRegistradoEventoDominio` es recibido, THEN THE Sistema_Reportes SHALL incrementar el contador de asistentes en `HistorialAsistencia`
3. WHEN un evento `AsientoReservadoEventoDominio` es recibido, THEN THE Sistema_Reportes SHALL registrar la reserva en `ReporteVentasDiarias`
4. WHEN un evento `AsientoLiberadoEventoDominio` es recibido, THEN THE Sistema_Reportes SHALL actualizar el estado de disponibilidad en los modelos de lectura
5. WHEN un evento es procesado exitosamente, THEN THE Sistema_Reportes SHALL registrar la operación en `LogAuditoria`

### Requisito 2: Contratos Espejo para Integración

**User Story:** Como desarrollador, quiero implementar contratos espejo de eventos externos, para consumir eventos sin dependencias compartidas entre microservicios.

#### Criterios de Aceptación

1. WHEN se define un contrato espejo, THEN THE Sistema_Reportes SHALL usar el namespace original del evento fuente
2. WHEN se define `EventoPublicadoEventoDominio`, THEN THE Sistema_Reportes SHALL usar el namespace `Eventos.Dominio.EventosDeDominio`
3. WHEN se define `AsientoReservadoEventoDominio`, THEN THE Sistema_Reportes SHALL usar el namespace `Asientos.Dominio.EventosDominio`
4. WHEN MassTransit procesa un evento, THEN THE Sistema_Reportes SHALL deserializar correctamente usando el contrato espejo
5. WHEN las propiedades del evento cambian en el origen, THEN THE Sistema_Reportes SHALL manejar la incompatibilidad sin fallar

### Requisito 3: Persistencia en MongoDB

**User Story:** Como Sistema_Reportes, quiero almacenar modelos de lectura en MongoDB, para optimizar consultas analíticas y reportes.

#### Criterios de Aceptación

1. WHEN el sistema inicia, THEN THE Sistema_Reportes SHALL conectarse a MongoDB usando la configuración de docker-compose
2. WHEN se recibe un evento, THEN THE Sistema_Reportes SHALL persistir los datos en la colección correspondiente
3. WHEN se consulta un reporte, THEN THE Sistema_Reportes SHALL recuperar datos de MongoDB en menos de 500ms
4. WHEN se actualiza un modelo de lectura, THEN THE Sistema_Reportes SHALL usar operaciones atómicas de MongoDB
5. WHEN MongoDB no está disponible, THEN THE Sistema_Reportes SHALL registrar el error y reintentar la conexión

### Requisito 4: Generación de Reportes Consolidados

**User Story:** Como administrador del sistema, quiero que se generen reportes consolidados automáticamente, para tener métricas agregadas sin consultas costosas en tiempo real.

#### Criterios de Aceptación

1. WHEN Hangfire ejecuta el job nocturno, THEN THE Job_Consolidacion SHALL calcular métricas diarias de ventas
2. WHEN se consolidan datos, THEN THE Job_Consolidacion SHALL agregar información de múltiples colecciones
3. WHEN el job finaliza exitosamente, THEN THE Job_Consolidacion SHALL actualizar la colección `ReportesConsolidados`
4. WHEN el job falla, THEN THE Job_Consolidacion SHALL registrar el error en `LogAuditoria` y programar un reintento
5. WHEN se ejecuta el job, THEN THE Job_Consolidacion SHALL completarse en menos de 5 minutos

### Requisito 5: Endpoint de Resumen de Ventas

**User Story:** Como usuario del sistema, quiero consultar un resumen de ventas, para conocer métricas generales de ingresos y reservas.

#### Criterios de Aceptación

1. WHEN se invoca `GET /api/reportes/resumen-ventas`, THEN THE Endpoint_Reporte SHALL retornar datos en formato JSON
2. WHEN se consulta el resumen, THEN THE Endpoint_Reporte SHALL incluir total de ventas, cantidad de reservas y promedio por evento
3. WHEN se especifica un rango de fechas, THEN THE Endpoint_Reporte SHALL filtrar los datos según el período solicitado
4. WHEN no hay datos disponibles, THEN THE Endpoint_Reporte SHALL retornar un objeto vacío con código 200
5. WHEN ocurre un error, THEN THE Endpoint_Reporte SHALL retornar un código de estado HTTP apropiado (400, 500)

### Requisito 6: Endpoint de Asistencia por Evento

**User Story:** Como organizador de eventos, quiero consultar la asistencia en tiempo real, para conocer el aforo actual y disponibilidad de asientos.

#### Criterios de Aceptación

1. WHEN se invoca `GET /api/reportes/asistencia/{eventoId}`, THEN THE Endpoint_Reporte SHALL retornar el aforo actual del evento
2. WHEN se consulta la asistencia, THEN THE Endpoint_Reporte SHALL incluir total de asistentes registrados y asientos reservados
3. WHEN el evento no existe, THEN THE Endpoint_Reporte SHALL retornar código 404 con mensaje descriptivo
4. WHEN se consulta un evento válido, THEN THE Endpoint_Reporte SHALL calcular el porcentaje de ocupación
5. WHEN hay asientos liberados, THEN THE Endpoint_Reporte SHALL reflejar la disponibilidad actualizada

### Requisito 7: Endpoint de Auditoría

**User Story:** Como administrador del sistema, quiero consultar logs de auditoría, para rastrear operaciones y detectar anomalías.

#### Criterios de Aceptación

1. WHEN se invoca `GET /api/reportes/auditoria`, THEN THE Endpoint_Reporte SHALL retornar logs ordenados por fecha descendente
2. WHEN se especifican filtros, THEN THE Endpoint_Reporte SHALL aplicar filtros por tipo de operación, fecha o entidad
3. WHEN se solicita paginación, THEN THE Endpoint_Reporte SHALL retornar resultados paginados con metadata
4. WHEN no hay logs disponibles, THEN THE Endpoint_Reporte SHALL retornar una lista vacía
5. WHEN se consultan logs, THEN THE Endpoint_Reporte SHALL incluir timestamp, tipo de operación y datos relevantes

### Requisito 8: Endpoint de Conciliación Financiera

**User Story:** Como contador del sistema, quiero consultar datos de conciliación financiera, para validar ingresos y generar reportes contables.

#### Criterios de Aceptación

1. WHEN se invoca `GET /api/reportes/conciliacion-financiera`, THEN THE Endpoint_Reporte SHALL retornar datos de transacciones procesadas
2. WHEN se consulta la conciliación, THEN THE Endpoint_Reporte SHALL incluir total de ingresos, cantidad de transacciones y desglose por categoría
3. WHEN se especifica un período, THEN THE Endpoint_Reporte SHALL filtrar transacciones por rango de fechas
4. WHEN hay discrepancias, THEN THE Endpoint_Reporte SHALL marcar las transacciones con estado de revisión
5. WHEN se exportan datos, THEN THE Endpoint_Reporte SHALL retornar formato JSON compatible con sistemas contables

### Requisito 9: Configuración de Infraestructura con Docker

**User Story:** Como DevOps, quiero desplegar el microservicio con Docker Compose, para facilitar la configuración y orquestación de servicios.

#### Criterios de Aceptación

1. WHEN se ejecuta `docker-compose up`, THEN THE Sistema_Reportes SHALL levantar MongoDB, RabbitMQ y la API
2. WHEN los servicios inician, THEN THE Sistema_Reportes SHALL verificar la conectividad con MongoDB y RabbitMQ
3. WHEN MongoDB no está listo, THEN THE Sistema_Reportes SHALL esperar usando health checks
4. WHEN RabbitMQ no está disponible, THEN THE Sistema_Reportes SHALL reintentar la conexión automáticamente
5. WHEN todos los servicios están listos, THEN THE Sistema_Reportes SHALL exponer el endpoint `/health` con estado 200

### Requisito 10: Manejo de Errores y Resiliencia

**User Story:** Como Sistema_Reportes, quiero manejar errores de forma resiliente, para garantizar la disponibilidad del servicio ante fallos temporales.

#### Criterios de Aceptación

1. WHEN un consumidor falla al procesar un evento, THEN THE Sistema_Reportes SHALL reintentar hasta 3 veces con backoff exponencial
2. WHEN un evento no puede ser procesado después de reintentos, THEN THE Sistema_Reportes SHALL moverlo a una cola de errores
3. WHEN MongoDB está temporalmente no disponible, THEN THE Sistema_Reportes SHALL encolar operaciones pendientes
4. WHEN un endpoint recibe parámetros inválidos, THEN THE Sistema_Reportes SHALL retornar código 400 con mensaje de validación
5. WHEN ocurre una excepción no controlada, THEN THE Sistema_Reportes SHALL registrar el error completo en logs y retornar código 500
