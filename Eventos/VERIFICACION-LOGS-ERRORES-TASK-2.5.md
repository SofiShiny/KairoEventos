# Verificación de Logs y Manejo de Errores - Task 2.5

## Objetivo

Validar que el sistema registra correctamente los logs y maneja errores de RabbitMQ de forma apropiada.

## Fecha de Verificación

**Fecha:** [Pendiente de ejecución]

## Requisitos Verificados

- **Requirement 1.5:** Registro de errores en logs cuando ocurre un error en la publicación

## Configuración de Logging

### Nivel de Logging Configurado

```csharp
// En Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

**Niveles disponibles:**
- Debug: Información detallada para diagnóstico
- Information: Flujo general de la aplicación
- Warning: Eventos anormales que no detienen la ejecución
- Error: Errores y excepciones
- Critical: Fallos críticos del sistema

## Logs Implementados en Handlers

### PublicarEventoComandoHandler

**Logs Informativos:**
- ✅ "Iniciando publicación de evento {EventoId}"
- ✅ "Evento {EventoId} encontrado, estado actual: {Estado}"
- ✅ "Evento {EventoId} marcado como publicado, guardando en BD..."
- ✅ "Evento {EventoId} guardado exitosamente en BD"
- ✅ "Verificación OK: Evento {EventoId} existe con estado {Estado}"
- ✅ "Publicando evento {EventoId} a RabbitMQ..."
- ✅ "Evento {EventoId} publicado exitosamente a RabbitMQ"

**Logs de Advertencia:**
- ✅ "Evento {EventoId} no encontrado"

**Logs de Error:**
- ✅ "ERROR CRÍTICO: Evento {EventoId} no se encuentra después de guardar"
- ✅ "Error de operación inválida al publicar evento {EventoId}"
- ✅ "Error inesperado al publicar evento {EventoId}"

### RegistrarAsistenteComandoHandler

**Logs Informativos:**
- ✅ "Iniciando registro de asistente para evento {EventoId}"
- ✅ "Registrando asistente {UsuarioId} en evento {EventoId}"
- ✅ "Guardando cambios en BD..."
- ✅ "Cambios guardados exitosamente en BD"
- ✅ "Publicando AsistenteRegistrado a RabbitMQ..."
- ✅ "AsistenteRegistrado publicado exitosamente a RabbitMQ"

**Logs de Debug:**
- ✅ "Estado del evento antes de registrar: {Estado}, Asistentes actuales: {Asistentes}"
- ✅ "Asistente agregado a la colección, total asistentes: {Total}"

**Logs de Error:**
- ✅ "Error de operación inválida al registrar asistente en evento {EventoId}"
- ✅ "Error inesperado al registrar asistente en evento {EventoId}. Tipo: {TipoExcepcion}, Mensaje: {Mensaje}"
- ✅ "Inner exception: {InnerMessage}" (cuando existe)

### CancelarEventoComandoHandler

**Logs Informativos:**
- ✅ "Iniciando cancelación de evento {EventoId}"
- ✅ "Cancelando evento {EventoId}, estado actual: {Estado}"
- ✅ "Evento cancelado, guardando en BD..."
- ✅ "Evento {EventoId} cancelado exitosamente en BD"
- ✅ "Publicando EventoCancelado a RabbitMQ..."
- ✅ "EventoCancelado publicado exitosamente a RabbitMQ"

**Logs de Error:**
- ✅ "Error de operación inválida al cancelar evento {EventoId}"
- ✅ "Error inesperado al cancelar evento {EventoId}. Tipo: {TipoExcepcion}, Mensaje: {Mensaje}"
- ✅ "Inner exception: {InnerMessage}" (cuando existe)

## Escenarios de Prueba

### Escenario 1: Operación Exitosa con RabbitMQ Funcionando

**Pasos:**
1. Crear evento
2. Publicar evento
3. Verificar logs

**Logs Esperados:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Borrador
[INFO] Evento {EventoId} marcado como publicado, guardando en BD...
[INFO] Evento {EventoId} guardado exitosamente en BD
[INFO] Verificación OK: Evento {EventoId} existe con estado Publicado
[INFO] Publicando evento {EventoId} a RabbitMQ...
[INFO] Evento {EventoId} publicado exitosamente a RabbitMQ
```

**Resultado:** [Pendiente]

### Escenario 2: Error de RabbitMQ (Servicio Caído)

**Pasos:**
1. Detener RabbitMQ: `docker stop rabbitmq-eventos`
2. Crear evento
3. Intentar publicar evento
4. Verificar logs de error

**Logs Esperados:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Borrador
[INFO] Evento {EventoId} marcado como publicado, guardando en BD...
[INFO] Evento {EventoId} guardado exitosamente en BD
[INFO] Verificación OK: Evento {EventoId} existe con estado Publicado
[INFO] Publicando evento {EventoId} a RabbitMQ...
[ERROR] Error inesperado al publicar evento {EventoId}. Tipo: RabbitMqConnectionException, Mensaje: ...
```

**Resultado:** [Pendiente]

### Escenario 3: Recuperación Automática de RabbitMQ

**Pasos:**
1. Reiniciar RabbitMQ: `docker start rabbitmq-eventos`
2. Esperar 10 segundos
3. Crear nuevo evento
4. Publicar evento
5. Verificar que funciona correctamente

**Logs Esperados:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Borrador
[INFO] Evento {EventoId} marcado como publicado, guardando en BD...
[INFO] Evento {EventoId} guardado exitosamente en BD
[INFO] Verificación OK: Evento {EventoId} existe con estado Publicado
[INFO] Publicando evento {EventoId} a RabbitMQ...
[INFO] Evento {EventoId} publicado exitosamente a RabbitMQ
```

**Resultado:** [Pendiente]

### Escenario 4: Error de Validación de Dominio

**Pasos:**
1. Crear evento
2. Publicar evento
3. Intentar publicar el mismo evento nuevamente (ya está publicado)
4. Verificar logs de error

**Logs Esperados:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Publicado
[ERROR] Error de operación inválida al publicar evento {EventoId}
```

**Resultado:** [Pendiente]

## Tipos de Excepciones Manejadas

### 1. InvalidOperationException
- **Origen:** Lógica de dominio (reglas de negocio)
- **Manejo:** Log de error + Resultado.Falla con mensaje
- **Ejemplo:** Intentar publicar un evento ya publicado

### 2. DbException
- **Origen:** Errores de base de datos
- **Manejo:** Log de error + Resultado.Falla
- **Ejemplo:** Error de conexión a PostgreSQL

### 3. RabbitMQ Exceptions
- **Origen:** MassTransit/RabbitMQ
- **Manejo:** Log de error con tipo y mensaje detallado
- **Ejemplo:** RabbitMQ no disponible

### 4. Exception (General)
- **Origen:** Cualquier error no esperado
- **Manejo:** Log de error con tipo, mensaje e inner exception
- **Ejemplo:** Errores de red, timeouts, etc.

## Información Incluida en Logs de Error

Para cada error, los logs incluyen:

1. ✅ **Tipo de excepción:** `ex.GetType().Name`
2. ✅ **Mensaje de error:** `ex.Message`
3. ✅ **Inner exception:** `ex.InnerException?.Message` (si existe)
4. ✅ **Contexto:** EventoId, UsuarioId, etc.
5. ✅ **Stack trace:** Automático con `_logger.LogError(ex, ...)`

## Estrategia de Manejo de Errores

### Orden de Operaciones

```
1. Lógica de Dominio (puede lanzar InvalidOperationException)
   ↓
2. Persistencia en PostgreSQL (puede lanzar DbException)
   ↓
3. Publicación a RabbitMQ (puede lanzar Exception)
```

### Decisión de Diseño

**Si falla la publicación a RabbitMQ:**
- ✅ Los cambios en PostgreSQL YA están guardados
- ✅ Se registra el error en logs
- ⚠️ Actualmente retorna error al cliente
- 💡 Considerar: Implementar Outbox Pattern para garantizar eventual consistency

## Comandos de Verificación

### 1. Ejecutar Script de Pruebas

```powershell
cd Eventos
.\test-logs-y-errores.ps1
```

### 2. Ver Logs de la API en Tiempo Real

```powershell
# En la terminal donde ejecutaste dotnet run
# Los logs se muestran automáticamente en consola
```

### 3. Filtrar Logs por Nivel

```powershell
# Buscar solo errores
docker logs eventos-api 2>&1 | Select-String "ERROR"

# Buscar logs de publicación
docker logs eventos-api 2>&1 | Select-String "Publicando evento"
```

### 4. Verificar Estado de RabbitMQ

```powershell
docker ps | Select-String "rabbitmq"
```

## Checklist de Verificación

### Logs Informativos
- [ ] Se registran logs al iniciar cada operación
- [ ] Se registran logs al completar cada paso exitosamente
- [ ] Los logs incluyen IDs relevantes (EventoId, UsuarioId)
- [ ] Los logs incluyen información de estado

### Logs de Error
- [ ] Se registran errores de dominio (InvalidOperationException)
- [ ] Se registran errores de persistencia (DbException)
- [ ] Se registran errores de RabbitMQ
- [ ] Los logs de error incluyen tipo de excepción
- [ ] Los logs de error incluyen mensaje detallado
- [ ] Los logs de error incluyen inner exception cuando existe

### Manejo de Errores
- [ ] Los errores de dominio retornan Resultado.Falla
- [ ] Los errores de persistencia retornan Resultado.Falla
- [ ] Los errores de RabbitMQ se registran correctamente
- [ ] El sistema no se cae ante errores de RabbitMQ
- [ ] El sistema se recupera automáticamente cuando RabbitMQ vuelve

### Resiliencia
- [ ] El sistema funciona cuando RabbitMQ está disponible
- [ ] El sistema registra errores cuando RabbitMQ no está disponible
- [ ] El sistema se reconecta automáticamente a RabbitMQ
- [ ] Los cambios en PostgreSQL persisten incluso si RabbitMQ falla

## Resultados de Ejecución

### Ejecución 1: [Fecha]

**Entorno:**
- API de Eventos: [Estado]
- RabbitMQ: [Estado]
- PostgreSQL: [Estado]

**Escenario 1 - Operación Exitosa:**
- Resultado: [Pendiente]
- Logs observados: [Pendiente]
- Observaciones: [Pendiente]

**Escenario 2 - Error de RabbitMQ:**
- Resultado: [Pendiente]
- Logs observados: [Pendiente]
- Observaciones: [Pendiente]

**Escenario 3 - Recuperación:**
- Resultado: [Pendiente]
- Logs observados: [Pendiente]
- Observaciones: [Pendiente]

**Escenario 4 - Error de Validación:**
- Resultado: [Pendiente]
- Logs observados: [Pendiente]
- Observaciones: [Pendiente]

## Problemas Encontrados

[Documentar aquí cualquier problema encontrado durante las pruebas]

## Mejoras Sugeridas

1. **Outbox Pattern:** Implementar para garantizar eventual consistency
2. **Retry Policy:** Configurar reintentos automáticos en MassTransit
3. **Circuit Breaker:** Implementar para evitar sobrecarga cuando RabbitMQ está caído
4. **Structured Logging:** Considerar Serilog para logs estructurados en JSON
5. **Correlation IDs:** Agregar IDs de correlación para rastrear requests

## Conclusión

**Estado de Task 2.5:** [Pendiente de ejecución]

**Cumplimiento de Requirement 1.5:** [Pendiente]

**Próximos Pasos:**
1. Ejecutar script de pruebas: `.\test-logs-y-errores.ps1`
2. Documentar resultados en este archivo
3. Verificar todos los checkpoints
4. Marcar task 2.5 como completada

---

**Documentado por:** Kiro AI  
**Última actualización:** [Pendiente]
