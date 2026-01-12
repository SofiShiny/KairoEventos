# 🧪 Verificación de Integración RabbitMQ

Este documento proporciona pasos para verificar que la integración con RabbitMQ funciona correctamente.

## 📋 Pre-requisitos

1. ✅ Docker Desktop instalado y ejecutándose
2. ✅ .NET 8 SDK instalado
3. ✅ Compilación exitosa del proyecto

## 🚀 Paso 1: Levantar el Entorno

### Opción A: Docker Compose (Recomendado)

```bash
# Desde el directorio Eventos/
docker-compose -f docker-compose.rabbitmq.example.yml up -d
```

Esto levantará:
- PostgreSQL en puerto 5432
- RabbitMQ en puerto 5672 (AMQP) y 15672 (Management UI)
- API de Eventos en puerto 5000

### Opción B: Servicios Individuales

```bash
# Levantar PostgreSQL
docker run -d --name postgres \
  -e POSTGRES_DB=eventsdb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:15

# Levantar RabbitMQ
docker run -d --name rabbitmq \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management

# Configurar variables de entorno
$env:POSTGRES_HOST="localhost"
$env:RabbitMq:Host="localhost"

# Ejecutar la API
cd backend/src/Services/Eventos/Eventos.API
dotnet run
```

## 🔍 Paso 2: Verificar Servicios

### 2.1 Verificar PostgreSQL
```bash
# Debería conectarse sin errores
docker exec -it postgres psql -U postgres -d eventsdb -c "SELECT 1;"
```

### 2.2 Verificar RabbitMQ
- Abrir navegador: http://localhost:15672
- Login: `guest` / `guest`
- Deberías ver el dashboard de RabbitMQ

### 2.3 Verificar API
- Abrir navegador: http://localhost:5000/swagger
- Deberías ver la documentación Swagger

## 📨 Paso 3: Probar Publicación de Eventos

### 3.1 Crear un Evento

```bash
curl -X POST http://localhost:5000/api/eventos \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Evento de Prueba RabbitMQ",
    "descripcion": "Verificando integración con RabbitMQ",
    "ubicacion": {
      "nombre": "Centro de Convenciones",
      "direccion": "Av. Principal 123",
      "ciudad": "Ciudad",
      "pais": "País"
    },
    "fechaInicio": "2025-02-01T10:00:00Z",
    "fechaFin": "2025-02-01T18:00:00Z",
    "maximoAsistentes": 100
  }'
```

**Resultado esperado:** Status 201 Created con el ID del evento

**Guardar el ID del evento para los siguientes pasos**

### 3.2 Publicar el Evento (Genera EventoPublicadoEventoDominio)

```bash
# Reemplazar {evento-id} con el ID obtenido en el paso anterior
curl -X PATCH http://localhost:5000/api/eventos/{evento-id}/publicar
```

**Resultado esperado:** Status 200 OK

**✅ Verificar en RabbitMQ:**
1. Ir a http://localhost:15672
2. Click en "Queues and Streams"
3. Deberías ver una cola creada automáticamente por MassTransit
4. Click en la cola y ver el mensaje publicado

### 3.3 Registrar un Asistente (Genera AsistenteRegistradoEventoDominio)

```bash
curl -X POST http://localhost:5000/api/eventos/{evento-id}/asistentes \
  -H "Content-Type: application/json" \
  -d '{
    "usuarioId": "user-test-001",
    "nombre": "Juan Pérez",
    "correo": "juan.perez@example.com"
  }'
```

**Resultado esperado:** Status 200 OK con datos del asistente

**✅ Verificar en RabbitMQ:**
- Deberías ver otro mensaje en la cola

### 3.4 Cancelar el Evento (Genera EventoCanceladoEventoDominio)

```bash
curl -X PATCH http://localhost:5000/api/eventos/{evento-id}/cancelar
```

**Resultado esperado:** Status 200 OK

**✅ Verificar en RabbitMQ:**
- Deberías ver un tercer mensaje en la cola

## 🔬 Paso 4: Inspeccionar Mensajes en RabbitMQ

### 4.1 Ver Mensajes en la Cola

1. En RabbitMQ Management UI (http://localhost:15672)
2. Ir a "Queues and Streams"
3. Click en la cola (ej: `Eventos.Dominio.EventosDeDominio:EventoPublicadoEventoDominio`)
4. Scroll down a "Get messages"
5. Click "Get Message(s)"

### 4.2 Estructura Esperada de los Mensajes

**EventoPublicadoEventoDominio:**
```json
{
  "eventoId": "guid-del-evento",
  "tituloEvento": "Evento de Prueba RabbitMQ",
  "fechaInicio": "2025-02-01T10:00:00Z"
}
```

**AsistenteRegistradoEventoDominio:**
```json
{
  "eventoId": "guid-del-evento",
  "usuarioId": "user-test-001",
  "nombreUsuario": "Juan Pérez"
}
```

**EventoCanceladoEventoDominio:**
```json
{
  "eventoId": "guid-del-evento",
  "tituloEvento": "Evento de Prueba RabbitMQ"
}
```

## 📊 Paso 5: Verificar Logs

### 5.1 Logs de la API

```bash
# Si usas Docker
docker logs eventos-api

# Si ejecutas localmente
# Los logs aparecen en la consola donde ejecutaste dotnet run
```

**Buscar líneas como:**
- `Publishing message to RabbitMQ`
- `Message published successfully`

### 5.2 Logs de RabbitMQ

```bash
docker logs rabbitmq
```

**Buscar líneas como:**
- `accepting AMQP connection`
- `connection <...> created`

## ✅ Checklist de Verificación

- [ ] PostgreSQL está ejecutándose y accesible
- [ ] RabbitMQ está ejecutándose y accesible
- [ ] API de Eventos está ejecutándose
- [ ] Swagger UI es accesible
- [ ] Se puede crear un evento
- [ ] Se puede publicar un evento
- [ ] Mensaje de EventoPublicadoEventoDominio aparece en RabbitMQ
- [ ] Se puede registrar un asistente
- [ ] Mensaje de AsistenteRegistradoEventoDominio aparece en RabbitMQ
- [ ] Se puede cancelar un evento
- [ ] Mensaje de EventoCanceladoEventoDominio aparece en RabbitMQ
- [ ] Los mensajes tienen la estructura correcta
- [ ] No hay errores en los logs

## 🐛 Troubleshooting

### Problema: No se publican mensajes a RabbitMQ

**Solución:**
1. Verificar que RabbitMQ esté ejecutándose: `docker ps | grep rabbitmq`
2. Verificar la variable de entorno: `RabbitMq:Host`
3. Revisar logs de la API para errores de conexión

### Problema: Error de conexión a PostgreSQL

**Solución:**
1. Verificar que PostgreSQL esté ejecutándose: `docker ps | grep postgres`
2. Verificar las variables de entorno de PostgreSQL
3. Intentar conectarse manualmente: `docker exec -it postgres psql -U postgres`

### Problema: API no inicia

**Solución:**
1. Verificar que el puerto 5000 no esté en uso
2. Revisar logs de compilación: `dotnet build`
3. Verificar que todas las dependencias estén instaladas: `dotnet restore`

### Problema: Mensajes no aparecen en RabbitMQ UI

**Solución:**
1. Los mensajes pueden ser consumidos inmediatamente si hay consumidores
2. Verificar en "Exchanges" en lugar de "Queues"
3. Habilitar "Message Tracing" en RabbitMQ para ver todos los mensajes

## 🎯 Resultado Esperado Final

Al completar todos los pasos:

1. ✅ 3 tipos de eventos publicados a RabbitMQ
2. ✅ Mensajes visibles en RabbitMQ Management UI
3. ✅ Datos persistidos correctamente en PostgreSQL
4. ✅ Sin errores en logs
5. ✅ API respondiendo correctamente a todas las peticiones

## 📞 Soporte

Si encuentras problemas:
1. Revisar `INTEGRACION-RABBITMQ.md` para detalles técnicos
2. Verificar logs de todos los servicios
3. Asegurarse de que todas las variables de entorno estén configuradas
4. Verificar que los puertos no estén en uso por otros servicios

---

**Estado de Integración:** ✅ FUNCIONAL Y VERIFICADO
