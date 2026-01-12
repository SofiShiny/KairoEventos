# 🔔 Sistema de Notificaciones en Tiempo Real

## 📋 Resumen

Este sistema implementa notificaciones en tiempo real usando **SignalR** para notificar a los usuarios cuando su pago es aprobado.

---

## 🏗️ Arquitectura

```
┌─────────────┐      ┌──────────────┐      ┌─────────────────┐      ┌──────────────┐
│   Pagos     │─────▶│  RabbitMQ    │─────▶│ Notificaciones  │─────▶│   Frontend   │
│   Service   │      │              │      │   Consumer      │      │   (React)    │
└─────────────┘      └──────────────┘      └─────────────────┘      └──────────────┘
                                                     │
                                                     ▼
                                            ┌─────────────────┐
                                            │  SignalR Hub    │
                                            │  (WebSockets)   │
                                            └─────────────────┘
```

---

## 🔧 Componentes Implementados

### 1. Backend - Notificaciones.API

#### `NotificacionesHub.cs`
- Hub de SignalR para gestionar conexiones WebSocket
- Autenticado con JWT
- Logs de conexión/desconexión

#### `PagoAprobadoConsumer.cs`
- Consumer de MassTransit que escucha `PagoAprobadoEvento`
- Envía notificación al usuario específico vía SignalR
- Manejo de errores sin reencolar mensajes

#### `Program.cs`
- Configuración de SignalR con autenticación JWT
- Soporte para tokens en query string (WebSockets)
- MassTransit configurado para RabbitMQ
- CORS habilitado para el frontend

---

### 2. Gateway

#### Configuración YARP
```json
"notificaciones-hub-route": {
  "ClusterId": "notificaciones-cluster",
  "Match": {
    "Path": "/hub/notificaciones"
  }
}
```

**Ver**: `.docs/CONFIGURACION_SIGNALR_GATEWAY.md` para instrucciones detalladas.

---

### 3. Frontend

#### `useSignalR.ts`
Hook de React que:
- ✅ Conecta automáticamente al Hub cuando el usuario está autenticado
- ✅ Reconexión automática con estrategia exponencial
- ✅ Envía el token JWT en la conexión
- ✅ Escucha eventos `RecibirNotificacion`
- ✅ Muestra toasts con `react-hot-toast`
- ✅ Dispara eventos personalizados para otros componentes

#### `App.tsx`
- Integra el hook `useSignalR`
- Configura `Toaster` para mostrar notificaciones
- Indicador de conexión en modo desarrollo

---

## 🚀 Cómo Usar

### 1. Levantar el Microservicio

```bash
cd Infraestructura
docker compose up -d notificaciones-api --build
```

### 2. Configurar el Gateway

Sigue las instrucciones en `.docs/CONFIGURACION_SIGNALR_GATEWAY.md`

### 3. Verificar Conexión

1. Abre el frontend: `http://localhost:5173`
2. Inicia sesión
3. Abre la consola del navegador (F12)
4. Deberías ver: `✅ SignalR conectado. ConnectionId: ...`

---

## 🧪 Probar el Sistema

### Opción 1: Simular Pago Aprobado

Publica un mensaje en RabbitMQ manualmente:

```bash
docker exec -it kairo-rabbitmq rabbitmqadmin publish \
  exchange=amq.topic \
  routing_key=PagoAprobadoEvento \
  payload='{"TransaccionId":"test-123","OrdenId":"orden-456","UsuarioId":"TU_USER_ID","Monto":100.50,"UrlFactura":"http://example.com/factura"}'
```

### Opción 2: Completar un Pago Real

1. Ve a un evento y compra una entrada
2. Completa el pago en MercadoPago
3. Cuando el pago sea aprobado, recibirás la notificación automáticamente

---

## 📊 Flujo Completo

1. **Usuario compra entrada** → Se crea orden en estado "Pendiente"
2. **Usuario paga** → MercadoPago webhook notifica al backend
3. **Pagos.API procesa** → Publica `PagoAprobadoEvento` en RabbitMQ
4. **Notificaciones Consumer** → Recibe el evento
5. **SignalR Hub** → Envía mensaje al usuario conectado
6. **Frontend** → Muestra toast "¡Pago Confirmado! 🎉"
7. **Opcional** → Recarga lista de entradas automáticamente

---

## 🔍 Debugging

### Ver Logs del Microservicio

```bash
docker logs kairo-notificaciones --tail 50 -f
```

### Ver Conexiones Activas en SignalR

En el frontend, abre la consola y ejecuta:

```javascript
console.log('SignalR ConnectionId:', window.signalRConnection?.connectionId);
```

### Verificar RabbitMQ

1. Abre: `http://localhost:15672`
2. Usuario: `guest` / Password: `guest`
3. Ve a "Queues" → Busca `notificaciones-pago-aprobado`

---

## ⚡ Características Avanzadas

### Recargar Datos Automáticamente

Escucha el evento personalizado en cualquier componente:

```typescript
useEffect(() => {
  const handlePagoAprobado = (event: CustomEvent) => {
    console.log('Pago aprobado:', event.detail);
    // Recargar lista de entradas
    queryClient.invalidateQueries(['entradas']);
  };

  window.addEventListener('pagoAprobado', handlePagoAprobado as EventListener);
  
  return () => {
    window.removeEventListener('pagoAprobado', handlePagoAprobado as EventListener);
  };
}, []);
```

### Notificaciones Personalizadas

Desde el backend, puedes enviar cualquier tipo de notificación:

```csharp
await _hubContext.Clients
    .User(usuarioId)
    .SendAsync("RecibirNotificacion", new {
        tipo = "evento_cancelado",
        titulo = "Evento Cancelado",
        mensaje = "El evento ha sido cancelado",
        eventoId = "123"
    });
```

---

## 🛡️ Seguridad

- ✅ Autenticación JWT requerida para conectarse al Hub
- ✅ Solo el usuario autenticado recibe sus propias notificaciones
- ✅ Token validado en cada conexión WebSocket
- ✅ CORS configurado solo para orígenes permitidos

---

## 📦 Dependencias

### Backend
- `Microsoft.AspNetCore.SignalR`
- `MassTransit.RabbitMQ`
- `Microsoft.AspNetCore.Authentication.JwtBearer`

### Frontend
- `@microsoft/signalr`
- `react-hot-toast`

---

## 🐛 Troubleshooting

### "SignalR no conecta"
1. Verifica que el servicio esté corriendo: `docker ps | grep notificaciones`
2. Verifica la configuración del Gateway
3. Revisa los logs: `docker logs kairo-notificaciones`

### "No recibo notificaciones"
1. Verifica que estés autenticado
2. Abre la consola y busca errores
3. Verifica que el `UsuarioId` en el evento coincida con tu `userId`

### "Error de CORS"
1. Verifica que el frontend esté en `http://localhost:5173`
2. Revisa la configuración de CORS en `Program.cs`

---

## 📚 Referencias

- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [MassTransit Documentation](https://masstransit.io/)
- [React Hot Toast](https://react-hot-toast.com/)
