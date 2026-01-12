# Configuración de SignalR en el Gateway

## 📋 Instrucciones

Agrega la siguiente configuración al archivo `Gateway/src/Gateway.API/appsettings.json`:

### 1. Agregar Ruta para SignalR Hub

Dentro de la sección `"Routes"`, **después de `"usuarios-route"`**, agrega:

```json
"notificaciones-hub-route": {
  "ClusterId": "notificaciones-cluster",
  "Match": {
    "Path": "/hub/notificaciones/{**catch-all}"
  },
  "Transforms": [
    {
      "PathPattern": "/hub/notificaciones/{**catch-all}"
    }
  ]
}
```

**IMPORTANTE**: 
- Asegúrate de agregar una coma (`,`) después del cierre de `"usuarios-route"`.
- Es vital incluir `/{**catch-all}` en el Path y el PathPattern para que funcione la negociación de SignalR.

### 2. Agregar Cluster para Notificaciones

Dentro de la sección `"Clusters"`, **después de `"notificaciones-cluster"`** (si ya existe), verifica que tenga:

```json
"notificaciones-cluster": {
  "Destinations": {
    "destination1": {
      "Address": "http://notificaciones-api:8080"
    }
  }
}
```

Si no existe, agrégalo después del último cluster.

---

## ✅ Resultado Esperado Completo

La configuración final debería verse así:

```json
{
  "ReverseProxy": {
    "Routes": {
      // ... otras rutas ...
      "usuarios-route": {
        "ClusterId": "usuarios-cluster",
        "Match": {
          "Path": "/api/usuarios/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/api/usuarios/{**catch-all}"
          }
        ]
      },
      "notificaciones-hub-route": {
        "ClusterId": "notificaciones-cluster",
        "Match": {
          "Path": "/hub/notificaciones/{**catch-all}"
        },
        "Transforms": [
          {
            "PathPattern": "/hub/notificaciones/{**catch-all}"
          }
        ]
      }
    },
    "Clusters": {
      // ... otros clusters ...
      "notificaciones-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://notificaciones-api:8080"
          }
        }
      }
    }
  }
}
```

---

## 🔧 Verificación

Después de agregar la configuración:
1. Reinicia el Gateway: `docker compose restart gateway-api` (o `gateway`)
2. Verifica que la ruta esté activa accediendo (o haciendo curl) a: `http://localhost:8080/hub/notificaciones/negotiate` (debería dar un error de método o 404 de SignalR interno, pero no un 404 del proxy).
