# 📋 Plan de Siguientes Pasos - Integración RabbitMQ

## Fase 1: Verificación y Pruebas Locales

### Task 1.1: Configurar Entorno Local
**Objetivo:** Preparar el entorno de desarrollo para pruebas

**Subtareas:**
- [ ] 1.1.1 Instalar/Verificar Docker Desktop
- [ ] 1.1.2 Levantar RabbitMQ en Docker
  ```bash
  docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
  ```
- [ ] 1.1.3 Verificar acceso a RabbitMQ Management UI (http://localhost:15672)
- [ ] 1.1.4 Levantar PostgreSQL en Docker
  ```bash
  docker run -d --name postgres -e POSTGRES_DB=eventsdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:15
  ```
- [ ] 1.1.5 Configurar variables de entorno
  ```bash
  $env:RabbitMq:Host="localhost"
  $env:POSTGRES_HOST="localhost"
  ```

**Tiempo estimado:** 15 minutos

---

### Task 1.2: Ejecutar API de Eventos
**Objetivo:** Levantar el microservicio de Eventos

**Subtareas:**
- [ ] 1.2.1 Navegar al directorio de la API
  ```bash
  cd Eventos/backend/src/Services/Eventos/Eventos.API
  ```
- [ ] 1.2.2 Restaurar dependencias
  ```bash
  dotnet restore
  ```
- [ ] 1.2.3 Ejecutar la API
  ```bash
  dotnet run
  ```
- [ ] 1.2.4 Verificar que la API está corriendo (http://localhost:5000/swagger)
- [ ] 1.2.5 Verificar endpoint de health (http://localhost:5000/health)

**Tiempo estimado:** 5 minutos

---

### Task 1.3: Pruebas de Publicación de Eventos
**Objetivo:** Verificar que los eventos se publican correctamente a RabbitMQ

**Subtareas:**
- [ ] 1.3.1 Crear un evento de prueba (POST /api/eventos)
- [ ] 1.3.2 Guardar el ID del evento creado
- [ ] 1.3.3 Publicar el evento (PATCH /api/eventos/{id}/publicar)
- [ ] 1.3.4 Verificar en RabbitMQ UI que se publicó EventoPublicadoEventoDominio
- [ ] 1.3.5 Registrar un asistente (POST /api/eventos/{id}/asistentes)
- [ ] 1.3.6 Verificar en RabbitMQ UI que se publicó AsistenteRegistradoEventoDominio
- [ ] 1.3.7 Cancelar el evento (PATCH /api/eventos/{id}/cancelar)
- [ ] 1.3.8 Verificar en RabbitMQ UI que se publicó EventoCanceladoEventoDominio
- [ ] 1.3.9 Inspeccionar estructura de los mensajes en RabbitMQ
- [ ] 1.3.10 Documentar resultados en VERIFICACION-INTEGRACION.md

**Tiempo estimado:** 20 minutos

**Criterios de éxito:**
- ✅ 3 tipos de eventos publicados correctamente
- ✅ Mensajes visibles en RabbitMQ Management UI
- ✅ Estructura de mensajes correcta
- ✅ Sin errores en logs

---

## Fase 2: Actualización del Microservicio de Reportes

### Task 2.1: Actualizar Contratos de Eventos en Reportes
**Objetivo:** Sincronizar los contratos de eventos con el namespace correcto

**Subtareas:**
- [ ] 2.1.1 Abrir archivo `Reportes/backend/src/Services/Reportes/Reportes.Dominio/ContratosExternos/EventosContratos.cs`
- [ ] 2.1.2 Verificar que el namespace sea `Eventos.Dominio.EventosDeDominio`
- [ ] 2.1.3 Verificar que las propiedades coincidan exactamente:
  - EventoPublicadoEventoDominio: EventoId, TituloEvento, FechaInicio
  - AsistenteRegistradoEventoDominio: EventoId, UsuarioId, NombreUsuario
  - EventoCanceladoEventoDominio: EventoId, TituloEvento
- [ ] 2.1.4 Actualizar si es necesario
- [ ] 2.1.5 Compilar proyecto de Reportes para verificar

**Tiempo estimado:** 10 minutos

---

### Task 2.2: Verificar Consumidores en Reportes
**Objetivo:** Asegurar que los consumidores están configurados correctamente

**Subtareas:**
- [ ] 2.2.1 Revisar `EventoPublicadoConsumer.cs`
- [ ] 2.2.2 Revisar `AsistenteRegistradoConsumer.cs`
- [ ] 2.2.3 Verificar que consumen los tipos correctos del namespace correcto
- [ ] 2.2.4 Verificar configuración en `InyeccionDependencias.cs`
- [ ] 2.2.5 Verificar configuración de MassTransit en `Program.cs` de Reportes

**Tiempo estimado:** 15 minutos

---

### Task 2.3: Crear Consumidor para EventoCancelado (si no existe)
**Objetivo:** Agregar soporte para el nuevo evento EventoCanceladoEventoDominio

**Subtareas:**
- [ ] 2.3.1 Verificar si existe `EventoCanceladoConsumer.cs`
- [ ] 2.3.2 Si no existe, crear el consumidor
- [ ] 2.3.3 Implementar lógica de negocio (actualizar métricas, logs, etc.)
- [ ] 2.3.4 Registrar el consumidor en `InyeccionDependencias.cs`
- [ ] 2.3.5 Agregar pruebas unitarias para el consumidor
- [ ] 2.3.6 Compilar y verificar

**Tiempo estimado:** 30 minutos

---

## Fase 3: Pruebas de Integración End-to-End

### Task 3.1: Configurar Entorno Completo
**Objetivo:** Levantar todos los microservicios y dependencias

**Subtareas:**
- [ ] 3.1.1 Crear docker-compose unificado (opcional)
- [ ] 3.1.2 Levantar RabbitMQ
- [ ] 3.1.3 Levantar PostgreSQL (Eventos)
- [ ] 3.1.4 Levantar MongoDB (Reportes)
- [ ] 3.1.5 Levantar API de Eventos
- [ ] 3.1.6 Levantar API de Reportes
- [ ] 3.1.7 Verificar que todos los servicios están healthy

**Tiempo estimado:** 20 minutos

---

### Task 3.2: Prueba End-to-End: Publicar Evento
**Objetivo:** Verificar flujo completo desde Eventos hasta Reportes

**Subtareas:**
- [ ] 3.2.1 Crear un evento en el microservicio de Eventos
- [ ] 3.2.2 Publicar el evento
- [ ] 3.2.3 Esperar 5 segundos para procesamiento
- [ ] 3.2.4 Verificar en RabbitMQ que el mensaje fue consumido
- [ ] 3.2.5 Consultar API de Reportes para verificar que se creó MetricasEvento
- [ ] 3.2.6 Verificar en MongoDB que los datos están persistidos
- [ ] 3.2.7 Verificar logs de ambos microservicios
- [ ] 3.2.8 Documentar resultados

**Tiempo estimado:** 15 minutos

---

### Task 3.3: Prueba End-to-End: Registrar Asistente
**Objetivo:** Verificar flujo completo de registro de asistente

**Subtareas:**
- [ ] 3.3.1 Usar el evento creado en Task 3.2
- [ ] 3.3.2 Registrar un asistente
- [ ] 3.3.3 Esperar 5 segundos para procesamiento
- [ ] 3.3.4 Verificar en RabbitMQ que el mensaje fue consumido
- [ ] 3.3.5 Consultar API de Reportes para verificar HistorialAsistencia
- [ ] 3.3.6 Verificar que las métricas se actualizaron (contador de asistentes)
- [ ] 3.3.7 Verificar en MongoDB que los datos están persistidos
- [ ] 3.3.8 Documentar resultados

**Tiempo estimado:** 15 minutos

---

### Task 3.4: Prueba End-to-End: Cancelar Evento
**Objetivo:** Verificar flujo completo de cancelación de evento

**Subtareas:**
- [ ] 3.4.1 Usar el evento creado en Task 3.2
- [ ] 3.4.2 Cancelar el evento
- [ ] 3.4.3 Esperar 5 segundos para procesamiento
- [ ] 3.4.4 Verificar en RabbitMQ que el mensaje fue consumido
- [ ] 3.4.5 Consultar API de Reportes para verificar actualización de estado
- [ ] 3.4.6 Verificar LogAuditoria de la cancelación
- [ ] 3.4.7 Verificar en MongoDB que los datos están persistidos
- [ ] 3.4.8 Documentar resultados

**Tiempo estimado:** 15 minutos

---

## Fase 4: Pruebas de Resiliencia

### Task 4.1: Prueba de Reconexión a RabbitMQ
**Objetivo:** Verificar que el sistema se recupera de fallos de RabbitMQ

**Subtareas:**
- [ ] 4.1.1 Levantar todos los servicios
- [ ] 4.1.2 Detener RabbitMQ
  ```bash
  docker stop rabbitmq
  ```
- [ ] 4.1.3 Intentar publicar un evento (debería fallar o quedar en cola)
- [ ] 4.1.4 Verificar logs de error
- [ ] 4.1.5 Reiniciar RabbitMQ
  ```bash
  docker start rabbitmq
  ```
- [ ] 4.1.6 Esperar reconexión automática
- [ ] 4.1.7 Publicar otro evento (debería funcionar)
- [ ] 4.1.8 Documentar comportamiento

**Tiempo estimado:** 20 minutos

---

### Task 4.2: Prueba de Carga Básica
**Objetivo:** Verificar comportamiento bajo carga moderada

**Subtareas:**
- [ ] 4.2.1 Crear script para publicar 100 eventos
- [ ] 4.2.2 Ejecutar script
- [ ] 4.2.3 Monitorear RabbitMQ Management UI
- [ ] 4.2.4 Verificar que todos los mensajes se procesan
- [ ] 4.2.5 Verificar tiempos de respuesta
- [ ] 4.2.6 Verificar uso de recursos (CPU, memoria)
- [ ] 4.2.7 Documentar resultados

**Tiempo estimado:** 30 minutos

---

## Fase 5: Documentación y Mejoras

### Task 5.1: Actualizar Documentación
**Objetivo:** Documentar la integración completa

**Subtareas:**
- [ ] 5.1.1 Actualizar README.md de Eventos con resultados de pruebas
- [ ] 5.1.2 Actualizar README.md de Reportes con información de integración
- [ ] 5.1.3 Crear diagrama de secuencia de la integración
- [ ] 5.1.4 Documentar casos de uso probados
- [ ] 5.1.5 Documentar problemas encontrados y soluciones
- [ ] 5.1.6 Crear guía de troubleshooting

**Tiempo estimado:** 45 minutos

---

### Task 5.2: Implementar Mejoras de Producción (Opcional)
**Objetivo:** Preparar para producción

**Subtareas:**
- [ ] 5.2.1 Implementar Outbox Pattern en Eventos
- [ ] 5.2.2 Agregar Retry Policies en MassTransit
- [ ] 5.2.3 Configurar Dead Letter Queues
- [ ] 5.2.4 Implementar Circuit Breaker
- [ ] 5.2.5 Agregar métricas de publicación (Prometheus)
- [ ] 5.2.6 Configurar alertas
- [ ] 5.2.7 Implementar logging estructurado (Serilog)

**Tiempo estimado:** 4-6 horas

---

### Task 5.3: Configurar Docker Compose Completo
**Objetivo:** Facilitar el despliegue del sistema completo

**Subtareas:**
- [ ] 5.3.1 Crear docker-compose.yml unificado
- [ ] 5.3.2 Incluir todos los servicios:
  - RabbitMQ
  - PostgreSQL
  - MongoDB
  - API Eventos
  - API Reportes
  - API Asientos (si aplica)
- [ ] 5.3.3 Configurar redes Docker
- [ ] 5.3.4 Configurar volúmenes persistentes
- [ ] 5.3.5 Configurar health checks
- [ ] 5.3.6 Probar despliegue completo
- [ ] 5.3.7 Documentar comandos

**Tiempo estimado:** 1 hora

---

## Fase 6: Integración con Microservicio de Asientos

### Task 6.1: Analizar Eventos Necesarios de Asientos
**Objetivo:** Identificar qué eventos debe publicar el microservicio de Asientos

**Subtareas:**
- [ ] 6.1.1 Revisar dominio de Asientos
- [ ] 6.1.2 Identificar eventos de dominio existentes:
  - AsientoReservadoEventoDominio
  - AsientoLiberadoEventoDominio
  - MapaAsientosCreadoEventoDominio
- [ ] 6.1.3 Documentar estructura de cada evento
- [ ] 6.1.4 Crear plan de integración similar al de Eventos

**Tiempo estimado:** 30 minutos

---

### Task 6.2: Implementar Publicación en Asientos
**Objetivo:** Integrar RabbitMQ en el microservicio de Asientos

**Subtareas:**
- [ ] 6.2.1 Instalar MassTransit.RabbitMQ en Asientos
- [ ] 6.2.2 Configurar MassTransit en Program.cs
- [ ] 6.2.3 Modificar handlers para publicar eventos
- [ ] 6.2.4 Compilar y verificar
- [ ] 6.2.5 Realizar pruebas similares a Eventos

**Tiempo estimado:** 1-2 horas

---

## Resumen de Tiempos Estimados

| Fase | Tiempo Estimado |
|------|-----------------|
| Fase 1: Verificación Local | 40 minutos |
| Fase 2: Actualización Reportes | 55 minutos |
| Fase 3: Pruebas E2E | 1 hora 5 minutos |
| Fase 4: Resiliencia | 50 minutos |
| Fase 5: Documentación | 1 hora 45 minutos |
| Fase 6: Asientos | 2-3 horas |
| **TOTAL** | **6-7 horas** |

---

## Prioridades

### 🔴 Alta Prioridad (Hacer Primero)
- Fase 1: Verificación y Pruebas Locales
- Fase 2: Actualización del Microservicio de Reportes
- Fase 3: Pruebas de Integración End-to-End

### 🟡 Media Prioridad (Hacer Después)
- Fase 4: Pruebas de Resiliencia
- Task 5.1: Actualizar Documentación
- Task 5.3: Docker Compose Completo

### 🟢 Baja Prioridad (Opcional/Futuro)
- Task 5.2: Mejoras de Producción
- Fase 6: Integración con Asientos

---

## Checklist Rápido de Inicio

Para empezar inmediatamente:

- [ ] Levantar RabbitMQ: `docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management`
- [ ] Levantar PostgreSQL: `docker run -d --name postgres -e POSTGRES_DB=eventsdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:15`
- [ ] Configurar variables: `$env:RabbitMq:Host="localhost"`
- [ ] Ejecutar API: `cd Eventos/backend/src/Services/Eventos/Eventos.API && dotnet run`
- [ ] Abrir Swagger: http://localhost:5000/swagger
- [ ] Abrir RabbitMQ UI: http://localhost:15672 (guest/guest)
- [ ] Seguir guía en VERIFICACION-INTEGRACION.md

---

## Notas Importantes

1. **Orden de Ejecución:** Seguir las fases en orden para evitar problemas
2. **Documentación:** Documentar todos los hallazgos y problemas
3. **Logs:** Mantener logs de todas las pruebas
4. **Rollback:** Tener plan de rollback si algo falla
5. **Backup:** Hacer backup de bases de datos antes de pruebas

---

**Estado:** 📋 PLAN LISTO PARA EJECUCIÓN
