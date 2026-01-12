# 🚧 Pasos Pendientes para Completar Notificaciones

## ⚠️ Estado Actual

El sistema de notificaciones está **casi completo**, pero hay un error de compilación en Docker que necesita resolverse manualmente.

---

## ✅ Lo que YA está Implementado:

1. ✅ **NotificacionesHub.cs** - Hub de SignalR
2. ✅ **PagoAprobadoConsumer.cs** - Consumer de RabbitMQ
3. ✅ **PagoAprobadoEvento.cs** - Contrato del evento
4. ✅ **Program.cs** - Configuración completa
5. ✅ **Frontend Hook** - `useSignalR.ts`
6. ✅ **Frontend Integration** - `App.tsx`
7. ✅ **Dependencias instaladas** - `@microsoft/signalr`, `react-hot-toast`

---

## 🔧 Pasos para Resolver el Error de Compilación:

### Opción 1: Compilar Localmente (Recomendado)

1. **Navega al proyecto de Notificaciones**:
   ```bash
   cd Notificaciones/src/Notificaciones.API
   ```

2. **Restaura los paquetes NuGet**:
   ```bash
   dotnet restore
   ```

3. **Compila el proyecto**:
   ```bash
   dotnet build
   ```

4. **Si hay errores**, revisa:
   - Que todos los archivos `.cs` tengan los `using` correctos
   - Que las referencias entre proyectos estén bien configuradas

5. **Una vez que compile localmente**, vuelve a intentar con Docker:
   ```bash
   cd ../../../Infraestructura
   docker compose up -d notificaciones-api --build
   ```

---

### Opción 2: Verificar Estructura de Carpetas

Asegúrate de que la estructura sea:

```
Notificaciones/
├── src/
│   ├── Notificaciones.API/
│   │   ├── Hubs/
│   │   │   └── NotificacionesHub.cs
│   │   ├── Program.cs
│   │   └── Notificaciones.API.csproj
│   ├── Notificaciones.Aplicacion/
│   │   ├── Consumers/
│   │   │   └── PagoAprobadoConsumer.cs
│   │   └── Notificaciones.Aplicacion.csproj
│   └── Notificaciones.Dominio/
│       ├── ContratosExternos/
│       │   └── PagoAprobadoEvento.cs
│       └── Notificaciones.Dominio.csproj
```

---

## 📝 Configuración del Gateway (PENDIENTE)

**IMPORTANTE**: Debes agregar manualmente la configuración al Gateway.

Abre: `Gateway/src/Gateway.API/appsettings.json`

### 1. En la sección "Routes", después de "usuarios-route", agrega:

```json
"notificaciones-hub-route": {
  "ClusterId": "notificaciones-cluster",
  "Match": {
    "Path": "/hub/notificaciones"
  },
  "Transforms": [
    {
      "PathPattern": "/hub/notificaciones"
    }
  ]
}
```

**⚠️ No olvides la coma (`,`) después del cierre de "usuarios-route"**

### 2. En la sección "Clusters", verifica que exista:

```json
"notificaciones-cluster": {
  "Destinations": {
    "destination1": {
      "Address": "http://notificaciones-api:8080"
    }
  }
}
```

### 3. Reinicia el Gateway:

```bash
docker compose restart gateway-api
```

---

## 🧪 Cómo Probar (Una vez que funcione)

### 1. Verificar que el servicio esté corriendo:

```bash
docker ps | grep notificaciones
docker logs kairo-notificaciones --tail 50 -f
```

### 2. Abrir el Frontend:

```
http://localhost:5173
```

### 3. Iniciar sesión y verificar en la consola:

Deberías ver:
```
🔌 Conectando a SignalR Hub: http://localhost:8080/hub/notificaciones
✅ SignalR conectado. ConnectionId: ...
```

### 4. Simular una notificación:

Opción A - Completar un pago real en la aplicación

Opción B - Publicar un mensaje manualmente en RabbitMQ:

```bash
docker exec -it kairo-rabbitmq rabbitmqadmin publish \
  exchange=amq.topic \
  routing_key=PagoAprobadoEvento \
  payload='{"TransaccionId":"test-123","OrdenId":"orden-456","UsuarioId":"TU_USER_ID","Monto":100.50,"UrlFactura":"http://example.com/factura"}'
```

**Reemplaza `TU_USER_ID`** con tu ID de usuario real (lo puedes ver en la consola del navegador).

---

## 📚 Documentación Completa

Ver: `.docs/NOTIFICACIONES_TIEMPO_REAL.md`

---

## 🆘 Si Necesitas Ayuda

1. **Error de compilación**: Compila localmente primero con `dotnet build`
2. **SignalR no conecta**: Verifica la configuración del Gateway
3. **No recibo notificaciones**: Verifica que el `UsuarioId` coincida

---

## ✨ Próximos Pasos Después de que Funcione:

1. ✅ Probar notificaciones en tiempo real
2. ✅ Integrar con el flujo de pagos real
3. ✅ Agregar más tipos de notificaciones (evento cancelado, etc.)
4. ✅ Implementar historial de notificaciones
