# Guía Rápida: Verificación de Logs y Manejo de Errores

## Resumen

Esta guía te ayuda a verificar que el sistema registra correctamente los logs y maneja errores de RabbitMQ.

## Prerequisitos

- ✅ API de Eventos ejecutándose (`dotnet run` en Eventos.API)
- ✅ RabbitMQ ejecutándose (`docker ps | grep rabbitmq`)
- ✅ PostgreSQL ejecutándose (`docker ps | grep postgres`)

## Opción 1: Verificación Manual (Recomendada) ⭐

### Paso 1: Ejecutar Guía Manual

```powershell
cd Eventos
.\test-logs-manual.ps1
```

### Paso 2: Seguir Instrucciones

El script te guiará paso a paso:
1. Crear y publicar evento con RabbitMQ funcionando
2. Simular error deteniendo RabbitMQ
3. Verificar recuperación al reiniciar RabbitMQ

### Paso 3: Revisar Logs

Los logs aparecen en la consola donde ejecutaste `dotnet run`.

**Busca estos patrones:**

✅ **Operación Exitosa:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Borrador
[INFO] Evento {EventoId} marcado como publicado, guardando en BD...
[INFO] Evento {EventoId} guardado exitosamente en BD
[INFO] Verificación OK: Evento {EventoId} existe con estado Publicado
[INFO] Publicando evento {EventoId} a RabbitMQ...
[INFO] Evento {EventoId} publicado exitosamente a RabbitMQ
```

❌ **Error de RabbitMQ:**
```
[INFO] Iniciando publicación de evento {EventoId}
[INFO] Evento {EventoId} encontrado, estado actual: Borrador
[INFO] Evento {EventoId} marcado como publicado, guardando en BD...
[INFO] Evento {EventoId} guardado exitosamente en BD
[INFO] Verificación OK: Evento {EventoId} existe con estado Publicado
[INFO] Publicando evento {EventoId} a RabbitMQ...
[ERROR] Error inesperado al publicar evento {EventoId}. Tipo: ..., Mensaje: ...
```

## Opción 2: Verificación Automatizada

### Ejecutar Script Automatizado

```powershell
cd Eventos
.\test-logs-y-errores.ps1
```

El script:
- ✅ Verifica servicios
- ✅ Crea y publica eventos
- ✅ Simula error de RabbitMQ
- ✅ Verifica recuperación
- ✅ Genera archivo de log

### Revisar Resultados

1. **Archivo de log:** `test-logs-errores-YYYYMMDD-HHMMSS.log`
2. **Logs de API:** Consola donde ejecutaste `dotnet run`

## Comandos Útiles

### Crear Evento

```powershell
$body = @{
    titulo = "Test Logs"
    descripcion = "Test"
    fechaInicio = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
    fechaFin = (Get-Date).AddDays(7).AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss")
    ubicacion = @{
        nombreLugar = "Test"
        direccion = "123 Test"
        ciudad = "City"
        pais = "Country"
    }
    maximoAsistentes = 100
} | ConvertTo-Json -Depth 3

$r = Invoke-RestMethod -Uri "http://localhost:5000/api/eventos" -Method Post -Body $body -ContentType "application/json"
$eventoId = $r.id
Write-Host "Evento creado: $eventoId"
```

### Publicar Evento

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/eventos/$eventoId/publicar" -Method Patch
```

### Controlar RabbitMQ

```powershell
# Detener
docker stop reportes-rabbitmq

# Iniciar
docker start reportes-rabbitmq

# Verificar estado
docker ps | grep rabbitmq
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

## Documentar Resultados

Documenta tus hallazgos en: `VERIFICACION-LOGS-ERRORES-TASK-2.5.md`

Incluye:
- ✅ Fecha de verificación
- ✅ Logs observados para cada escenario
- ✅ Problemas encontrados (si los hay)
- ✅ Capturas de pantalla (opcional)

## Problemas Comunes

### API no responde
**Solución:** Verifica que la API esté ejecutándose
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/health"
```

### RabbitMQ no está disponible
**Solución:** Inicia RabbitMQ
```powershell
docker start reportes-rabbitmq
```

### Error 400 al crear evento
**Solución:** Verifica la estructura del JSON (debe incluir ubicacion como objeto)

### No veo logs en consola
**Solución:** Verifica que el nivel de logging sea Debug en Program.cs

## Recursos Adicionales

- **Documentación completa:** `VERIFICACION-LOGS-ERRORES-TASK-2.5.md`
- **Resumen de completación:** `TASK-2.5-COMPLETION-SUMMARY.md`
- **Diseño de integración:** `.kiro/specs/integracion-rabbitmq-eventos/design.md`
- **Requirements:** `.kiro/specs/integracion-rabbitmq-eventos/requirements.md`

## Soporte

Si encuentras problemas:
1. Revisa los logs de la API en consola
2. Verifica que todos los servicios estén ejecutándose
3. Consulta la documentación completa
4. Revisa los ejemplos en los scripts de prueba

---

**Última actualización:** 29 de Diciembre de 2025
