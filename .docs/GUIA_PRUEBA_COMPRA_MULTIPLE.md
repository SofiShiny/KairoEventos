# 🧪 Guía de Prueba - Compra Múltiple de Tickets

## ✅ Estado del Sistema

**Backend**: ✅ Actualizado y corriendo
- `kairo-entradas` reconstruido con los cambios
- Consumer `PagoAprobadoConsumer` actualizado
- Repositorio con métodos nuevos implementados

**Frontend**: ✅ Corriendo
- `npm run dev` activo en puerto 5173
- Cambios ya aplicados para enviar múltiples asientos

---

## 🎯 Pasos para Probar

### 1. **Preparación**

Asegúrate de tener:
- ✅ Frontend corriendo: `http://localhost:5173`
- ✅ Backend corriendo: `docker ps` muestra `kairo-entradas`, `kairo-gateway`, `kairo-pagos`
- ✅ Usuario autenticado en Keycloak

### 2. **Flujo de Prueba Completo**

#### **Paso 1: Navegar a un Evento**
```
1. Abre http://localhost:5173
2. Inicia sesión con Keycloak
3. Ve a "Eventos" o "Explorar"
4. Selecciona un evento que tenga asientos disponibles
5. Haz clic en "Comprar Tickets" o "Seleccionar Asientos"
```

#### **Paso 2: Seleccionar Múltiples Asientos**
```
1. En el mapa de asientos, selecciona 2-3 asientos
2. Verifica que el resumen muestre:
   - Cantidad correcta de asientos
   - Precio total sumado
   - Ejemplo: "Subtotal (3 asientos): $150.00"
```

#### **Paso 3: Iniciar Pago**
```
1. Haz clic en "Proceder al Pago"
2. Observa la consola del navegador (F12):
   - Debe mostrar: "Crear entradas para todos los asientos seleccionados"
   - Request a /api/entradas con asientoIds: ["id1", "id2", "id3"]
```

#### **Paso 4: Completar Pago**
```
1. En el formulario de pago, ingresa:
   - Número de tarjeta: 4111 1111 1111 1111 (Visa de prueba)
   - Titular: TU NOMBRE
   - Expiración: 12/25
   - CVV: 123

2. Haz clic en "Pagar $XXX.XX"
```

#### **Paso 5: Verificar Confirmación**
```
1. Espera la confirmación del pago
2. Deberías ver un mensaje: "¡Pago procesado!"
3. Serás redirigido a "Mis Entradas"
```

#### **Paso 6: Verificar Entradas Creadas**
```
1. En "Mis Entradas", deberías ver:
   - TODAS las entradas que compraste (2-3 tickets)
   - Cada una con su propio QR
   - Estado: "Pagada" o "Confirmada"
```

---

## 🔍 Verificación en Backend

### Opción A: Revisar Logs del Consumer

```bash
# Ver logs del servicio de entradas
docker logs kairo-entradas -f

# Buscar mensajes del consumer:
# 🎫 Recibido PagoAprobadoEvento - OrdenId: xxx
# 📋 Se encontraron 3 entrada(s) para confirmar
# ✅ Pago confirmado exitosamente. Nuevas confirmaciones: 3
```

### Opción B: Consultar Base de Datos

```bash
# Conectarse a PostgreSQL
docker exec -it kairo-postgres psql -U postgres -d kairo_entradas

# Ver entradas recientes
SELECT 
    "Id", 
    "UsuarioId", 
    "EventoId", 
    "AsientoId", 
    "Estado", 
    "Monto",
    "FechaCreacion"
FROM "Entradas"
ORDER BY "FechaCreacion" DESC
LIMIT 10;

# Verificar que todas estén en estado Pagada (2)
SELECT "Estado", COUNT(*) 
FROM "Entradas" 
GROUP BY "Estado";
```

---

## 🐛 Troubleshooting

### Problema 1: Solo se crea 1 entrada

**Síntomas**: 
- Frontend envía 3 asientos
- Backend solo crea 1 entrada

**Solución**:
```bash
# Verificar que el backend se reconstruyó
docker ps --filter "name=kairo-entradas"

# Revisar logs
docker logs kairo-entradas | grep "asientosAProcesar"
```

### Problema 2: Entradas no se confirman

**Síntomas**:
- Pago se procesa
- Entradas quedan en estado "Reservada"

**Solución**:
```bash
# Verificar que el consumer está escuchando
docker logs kairo-entradas | grep "PagoAprobadoEvento"

# Verificar RabbitMQ
docker logs kairo-rabbitmq | tail -20

# Verificar que pagos-api esté corriendo
docker ps --filter "name=kairo-pagos"
```

### Problema 3: Error 500 al crear entradas

**Síntomas**:
- Error en consola del navegador
- No se crea ninguna entrada

**Solución**:
```bash
# Ver logs detallados
docker logs kairo-entradas --tail 50

# Verificar que la BD esté accesible
docker exec kairo-postgres pg_isready
```

---

## 📊 Casos de Prueba

### ✅ Caso 1: Compra de 1 Ticket
```
Entrada: 1 asiento seleccionado
Esperado: 1 entrada creada y confirmada
```

### ✅ Caso 2: Compra de 3 Tickets
```
Entrada: 3 asientos seleccionados
Esperado: 3 entradas creadas y confirmadas
```

### ✅ Caso 3: Idempotencia
```
Entrada: Procesar el mismo pago 2 veces (simular)
Esperado: No duplicar entradas, loguear "ya confirmadas"
```

### ✅ Caso 4: Error Parcial
```
Entrada: 3 asientos, 1 con problema
Esperado: 2 entradas confirmadas, 1 con error logueado
```

---

## 🎉 Criterios de Éxito

La prueba es exitosa si:

1. ✅ Puedes seleccionar múltiples asientos (2-3)
2. ✅ El resumen muestra el total correcto
3. ✅ Se crean TODAS las entradas (verificar en BD o "Mis Entradas")
4. ✅ Todas las entradas quedan en estado "Pagada"
5. ✅ Cada entrada tiene su propio QR único
6. ✅ Los logs muestran: "✅ Pago confirmado exitosamente. Nuevas confirmaciones: 3"

---

## 🚀 Comandos Útiles

```bash
# Ver todos los servicios
docker ps

# Reconstruir entradas-api si hay cambios
docker compose up -d entradas-api --build

# Ver logs en tiempo real
docker logs kairo-entradas -f

# Reiniciar un servicio
docker restart kairo-entradas

# Ver estado de RabbitMQ
docker logs kairo-rabbitmq | grep "connection"
```

---

## 📝 Notas

- El frontend ya tiene los cambios aplicados (no necesitas recargar)
- El backend se reconstruyó automáticamente
- Los cambios están en memoria, no necesitas reiniciar Docker Compose completo
- Si algo falla, revisa los logs primero

---

**¡Listo para probar!** 🎫✨

Abre http://localhost:5173 y sigue los pasos.
