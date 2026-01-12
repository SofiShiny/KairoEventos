# Integración del Microservicio de Pagos - Pasos Finales

## ✅ Completado

1. **Servicio de Pagos** (`pagos.service.ts`) - Conectado con el microservicio real
2. **PaymentForm** (`PaymentForm.tsx`) - Actualizado con ordenId y usuarioId

## 🔧 Cambios Pendientes en CheckoutPage.tsx

Necesitas hacer 2 cambios manuales en `CheckoutPage.tsx`:

### Cambio 1: Agregar ordenId al estado (línea ~11-16)

**Busca:**
```typescript
  const [asientos, setAsientos] = useState<Asiento[]>([]);
  const [selectedAsientos, setSelectedAsientos] = useState<Asiento[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [processingPurchase, setProcessingPurchase] = useState(false);
```

**Reemplaza con:**
```typescript
  const [asientos, setAsientos] = useState<Asiento[]>([]);
  const [selectedAsientos, setSelectedAsientos] = useState<Asiento[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [processingPurchase, setProcessingPurchase] = useState(false);
  const [ordenId] = useState(() => crypto.randomUUID()); // Generar ordenId único
```

### Cambio 2: Actualizar PaymentForm con props necesarios (línea ~260-267)

**Busca:**
```typescript
      {/* Modal de Pago */}
      {showPaymentForm && (
        <PaymentForm
          monto={totalPrice}
          onSuccess={handlePaymentSuccess}
          onCancel={() => setShowPaymentForm(false)}
        />
      )}
```

**Reemplaza con:**
```typescript
      {/* Modal de Pago */}
      {showPaymentForm && (
        <PaymentForm
          monto={totalPrice}
          ordenId={ordenId}
          usuarioId="00000000-0000-0000-0000-000000000001" // TODO: Obtener del contexto de autenticación
          onSuccess={handlePaymentSuccess}
          onCancel={() => setShowPaymentForm(false)}
        />
      )}
```

## 📝 Notas Importantes

### UsuarioId Hardcodeado
Por ahora estamos usando un GUID fijo para el `usuarioId`. Cuando implementes autenticación:
1. Crea un contexto de autenticación
2. Almacena el usuario logueado
3. Reemplaza el GUID fijo con: `const { usuario } = useAuth(); usuarioId={usuario.id}`

### OrdenId
Se genera un UUID único cada vez que se carga la página de checkout. Esto es correcto para el flujo actual.

### Endpoint del Microservicio
El servicio está configurado para llamar a `http://localhost:5007/api/pagos` (puerto 5007).
Asegúrate de que tu microservicio de Pagos esté corriendo en ese puerto.

## 🚀 Para Probar

1. Asegúrate de que el microservicio de Pagos esté corriendo
2. Selecciona asientos en el checkout
3. Click en "Proceder al Pago"
4. Completa el formulario con datos de prueba
5. El sistema llamará al endpoint real de Pagos
6. Recibirás un `transaccionId` real del backend
7. Se mostrará la confirmación con el ID de transacción

## 🔍 Verificar en Swagger

Puedes verificar que el pago se procesó correctamente:
```
GET http://localhost:5007/api/pagos/{transaccionId}
```

Esto te mostrará el estado de la transacción en el backend.
