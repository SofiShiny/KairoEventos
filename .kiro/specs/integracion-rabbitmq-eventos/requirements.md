# Requirements Document - Integración RabbitMQ en Microservicio de Eventos

## Introduction

Este documento especifica los requisitos para completar la integración de RabbitMQ en el microservicio de Eventos, incluyendo verificación, pruebas y mejoras de producción.

## Glossary

- **Sistema_Eventos**: Microservicio de gestión de eventos
- **RabbitMQ**: Message broker para comunicación asíncrona
- **MassTransit**: Librería .NET para abstracción de message brokers
- **Evento_Dominio**: Evento que representa un cambio de estado en el dominio
- **PostgreSQL**: Base de datos relacional para persistencia
- **Consumidor**: Microservicio que recibe y procesa eventos publicados
- **Publisher**: Componente que publica eventos a RabbitMQ
- **Handler**: Componente que maneja comandos y publica eventos

## Requirements

### Requirement 1: Verificación Local de Integración

**User Story:** Como desarrollador, quiero verificar que la integración con RabbitMQ funciona correctamente en mi entorno local, para asegurarme de que los eventos se publican correctamente.

#### Acceptance Criteria

1. WHEN el desarrollador ejecuta el script de pruebas, THEN THE Sistema_Eventos SHALL publicar eventos a RabbitMQ sin errores
2. WHEN un evento es publicado, THEN THE Sistema_Eventos SHALL persistir los cambios en PostgreSQL antes de publicar
3. WHEN se ejecutan las pruebas automatizadas, THEN THE Sistema_Eventos SHALL verificar que los 3 tipos de eventos se publican correctamente
4. WHEN se consulta RabbitMQ Management UI, THEN THE Sistema_Eventos SHALL mostrar los mensajes publicados en las colas correspondientes
5. WHEN ocurre un error en la publicación, THEN THE Sistema_Eventos SHALL registrar el error en los logs

### Requirement 2: Pruebas End-to-End con Microservicio de Reportes

**User Story:** Como arquitecto de software, quiero verificar que la comunicación entre Eventos y Reportes funciona correctamente, para asegurar la integridad del sistema distribuido.

#### Acceptance Criteria

1. WHEN un evento es publicado en Sistema_Eventos, THEN THE Sistema_Reportes SHALL consumir el mensaje de RabbitMQ
2. WHEN Sistema_Reportes consume EventoPublicadoEventoDominio, THEN THE Sistema_Reportes SHALL crear un registro de MetricasEvento en MongoDB
3. WHEN Sistema_Reportes consume AsistenteRegistradoEventoDominio, THEN THE Sistema_Reportes SHALL actualizar el HistorialAsistencia en MongoDB
4. WHEN Sistema_Reportes consume EventoCanceladoEventoDominio, THEN THE Sistema_Reportes SHALL actualizar el estado del evento en MongoDB
5. WHEN se consulta la API de Reportes, THEN THE Sistema_Reportes SHALL retornar los datos actualizados correctamente

### Requirement 3: Actualización de Contratos en Microservicio de Reportes

**User Story:** Como desarrollador del microservicio de Reportes, quiero que los contratos de eventos estén sincronizados con el namespace correcto, para evitar errores de deserialización.

#### Acceptance Criteria

1. WHEN se revisan los contratos en Reportes, THEN THE Sistema_Reportes SHALL usar el namespace `Eventos.Dominio.EventosDeDominio`
2. WHEN se comparan las propiedades de EventoPublicadoEventoDominio, THEN THE Sistema_Reportes SHALL tener las mismas propiedades que Sistema_Eventos
3. WHEN se comparan las propiedades de AsistenteRegistradoEventoDominio, THEN THE Sistema_Reportes SHALL tener las mismas propiedades que Sistema_Eventos
4. WHEN se comparan las propiedades de EventoCanceladoEventoDominio, THEN THE Sistema_Reportes SHALL tener las mismas propiedades que Sistema_Eventos
5. WHEN se compila el proyecto de Reportes, THEN THE Sistema_Reportes SHALL compilar sin errores

### Requirement 4: Consumidor para EventoCancelado

**User Story:** Como desarrollador del microservicio de Reportes, quiero un consumidor para EventoCanceladoEventoDominio, para mantener sincronizado el estado de eventos cancelados.

#### Acceptance Criteria

1. WHEN se crea EventoCanceladoConsumer, THEN THE Sistema_Reportes SHALL implementar IConsumer<EventoCanceladoEventoDominio>
2. WHEN EventoCanceladoConsumer recibe un mensaje, THEN THE Sistema_Reportes SHALL actualizar el estado del evento en MongoDB
3. WHEN EventoCanceladoConsumer recibe un mensaje, THEN THE Sistema_Reportes SHALL registrar un LogAuditoria de la cancelación
4. WHEN EventoCanceladoConsumer está registrado, THEN THE Sistema_Reportes SHALL incluirlo en la configuración de MassTransit
5. WHEN ocurre un error en el consumidor, THEN THE Sistema_Reportes SHALL manejar la excepción y registrarla en logs

### Requirement 5: Pruebas de Resiliencia

**User Story:** Como ingeniero de confiabilidad, quiero verificar que el sistema se recupera de fallos temporales, para asegurar la disponibilidad del servicio.

#### Acceptance Criteria

1. WHEN RabbitMQ se detiene temporalmente, THEN THE Sistema_Eventos SHALL registrar errores de conexión en logs
2. WHEN RabbitMQ se reinicia, THEN THE Sistema_Eventos SHALL reconectarse automáticamente
3. WHEN se publican 100 eventos consecutivos, THEN THE Sistema_Eventos SHALL procesarlos todos sin pérdida de mensajes
4. WHEN se monitorea el uso de recursos, THEN THE Sistema_Eventos SHALL mantener un uso de CPU y memoria estable
5. WHEN se verifica RabbitMQ Management UI, THEN THE Sistema_Eventos SHALL mostrar todas las colas procesadas correctamente

### Requirement 6: Configuración Docker Compose Completa

**User Story:** Como DevOps engineer, quiero un docker-compose que levante todo el entorno, para facilitar el despliegue y las pruebas.

#### Acceptance Criteria

1. WHEN se ejecuta docker-compose up, THEN THE Sistema SHALL levantar RabbitMQ, PostgreSQL, MongoDB y las APIs
2. WHEN todos los servicios están corriendo, THEN THE Sistema SHALL verificar health checks de cada servicio
3. WHEN se configuran las redes Docker, THEN THE Sistema SHALL permitir comunicación entre todos los servicios
4. WHEN se configuran volúmenes persistentes, THEN THE Sistema SHALL mantener los datos después de reiniciar contenedores
5. WHEN se detienen los servicios, THEN THE Sistema SHALL hacer shutdown graceful de todos los contenedores

### Requirement 7: Outbox Pattern (Opcional)

**User Story:** Como arquitecto de software, quiero implementar Outbox Pattern, para garantizar consistencia eventual entre PostgreSQL y RabbitMQ.

#### Acceptance Criteria

1. WHEN se guarda un evento en PostgreSQL, THEN THE Sistema_Eventos SHALL guardar también en tabla outbox
2. WHEN un worker procesa la tabla outbox, THEN THE Sistema_Eventos SHALL publicar los eventos pendientes a RabbitMQ
3. WHEN un evento se publica exitosamente, THEN THE Sistema_Eventos SHALL marcar el registro de outbox como procesado
4. WHEN ocurre un error en la publicación, THEN THE Sistema_Eventos SHALL reintentar según la política configurada
5. WHEN se consulta la tabla outbox, THEN THE Sistema_Eventos SHALL mostrar el estado de cada mensaje

### Requirement 8: Retry Policies (Opcional)

**User Story:** Como ingeniero de confiabilidad, quiero políticas de reintento configuradas, para manejar fallos temporales de RabbitMQ.

#### Acceptance Criteria

1. WHEN falla la publicación de un evento, THEN THE Sistema_Eventos SHALL reintentar según la política configurada
2. WHEN se configura backoff exponencial, THEN THE Sistema_Eventos SHALL aumentar el tiempo entre reintentos
3. WHEN se alcanza el máximo de reintentos, THEN THE Sistema_Eventos SHALL registrar el error y notificar
4. WHEN se configura circuit breaker, THEN THE Sistema_Eventos SHALL abrir el circuito después de N fallos consecutivos
5. WHEN el circuito está abierto, THEN THE Sistema_Eventos SHALL rechazar publicaciones temporalmente

### Requirement 9: Dead Letter Queues (Opcional)

**User Story:** Como ingeniero de confiabilidad, quiero que los mensajes fallidos vayan a una Dead Letter Queue, para poder analizarlos y reprocesarlos.

#### Acceptance Criteria

1. WHEN un mensaje falla después de todos los reintentos, THEN THE Sistema_Eventos SHALL enviarlo a la DLQ
2. WHEN se consulta la DLQ en RabbitMQ, THEN THE Sistema_Eventos SHALL mostrar los mensajes fallidos
3. WHEN se analiza un mensaje en DLQ, THEN THE Sistema_Eventos SHALL incluir información del error original
4. WHEN se decide reprocesar un mensaje, THEN THE Sistema_Eventos SHALL permitir moverlo de vuelta a la cola principal
5. WHEN se configuran alertas, THEN THE Sistema_Eventos SHALL notificar cuando hay mensajes en DLQ

### Requirement 10: Observabilidad (Opcional)

**User Story:** Como ingeniero de operaciones, quiero métricas y logs estructurados, para monitorear el sistema en producción.

#### Acceptance Criteria

1. WHEN se implementa logging estructurado, THEN THE Sistema_Eventos SHALL usar Serilog con formato JSON
2. WHEN se publican métricas, THEN THE Sistema_Eventos SHALL exponer endpoint /metrics para Prometheus
3. WHEN se configura tracing, THEN THE Sistema_Eventos SHALL usar OpenTelemetry para distributed tracing
4. WHEN se crean dashboards, THEN THE Sistema_Eventos SHALL mostrar métricas clave en Grafana
5. WHEN se configuran alertas, THEN THE Sistema_Eventos SHALL notificar cuando se superan umbrales definidos

## Priority Matrix

| Requirement | Priority | Status |
|-------------|----------|--------|
| 1. Verificación Local | 🔴 Alta | ⏳ Pendiente |
| 2. Pruebas E2E | 🔴 Alta | ⏳ Pendiente |
| 3. Actualización Contratos | 🔴 Alta | ⏳ Pendiente |
| 4. Consumidor EventoCancelado | 🔴 Alta | ⏳ Pendiente |
| 5. Pruebas de Resiliencia | 🟡 Media | ⏳ Pendiente |
| 6. Docker Compose | 🟡 Media | ⏳ Pendiente |
| 7. Outbox Pattern | 🟢 Baja | ⏳ Pendiente |
| 8. Retry Policies | 🟢 Baja | ⏳ Pendiente |
| 9. Dead Letter Queues | 🟢 Baja | ⏳ Pendiente |
| 10. Observabilidad | 🟢 Baja | ⏳ Pendiente |

## Dependencies

- .NET 8 SDK
- Docker Desktop
- RabbitMQ 3.x
- PostgreSQL 15
- MongoDB 6.x (para Reportes)
- MassTransit 8.1.3

## Notes

- Los requirements 1-6 son necesarios para completar la integración básica
- Los requirements 7-10 son mejoras opcionales para producción
- La implementación debe seguir el patrón ya establecido en el código existente
- Todos los cambios deben ser compatibles con la arquitectura hexagonal actual
