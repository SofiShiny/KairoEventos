# 🎫 Kairo - Sistema de Eventos Frontend

## 🚀 Nuevas Funcionalidades Implementadas

### 1. **Registro de Usuarios** (`/register`)
- Formulario completo de registro con validaciones
- Sincronización automática con Keycloak y BD local
- Diseño Kairo Dark con efectos neón
- Validaciones:
  - Username: mínimo 3 caracteres
  - Password: mínimo 8 caracteres
  - Email: formato válido
  - Confirmación de contraseña

**Ruta:** `http://localhost:5173/register`

### 2. **Checkout con Mapa de Asientos** (`/checkout/:eventoId`)
- Visualización interactiva del mapa de asientos
- Estados visuales claros:
  - 🟢 **Verde**: Disponible
  - 🔵 **Azul**: Seleccionado
  - 🟡 **Amarillo**: Reservado
  - 🔴 **Rojo**: Ocupado
- Selección múltiple de asientos
- Cálculo automático del precio total
- Resumen de compra en tiempo real
- Diseño tipo cine con escenario

**Ruta:** `http://localhost:5173/checkout/{eventoId}`

## 📁 Estructura de Archivos Creados

```
src/
├── features/
│   ├── auth/
│   │   ├── pages/
│   │   │   └── RegisterPage.tsx          ✨ NUEVO
│   │   └── services/
│   │       └── auth.service.ts           ✨ NUEVO
│   ├── asientos/
│   │   ├── components/
│   │   │   └── SeatMap.tsx               ✨ NUEVO
│   │   └── services/
│   │       └── asientos.service.ts       ✨ NUEVO
│   └── entradas/
│       └── pages/
│           └── CheckoutPage.tsx          ✨ ACTUALIZADO
└── router.tsx                            ✨ ACTUALIZADO
```

## 🎨 Diseño Kairo Dark

Todos los componentes siguen el tema oscuro premium:
- Fondo negro (#000000)
- Acentos neón (púrpura, rosa, cyan)
- Gradientes vibrantes
- Efectos de blur y glow
- Animaciones suaves
- Tipografía moderna

## 🔧 Configuración

1. **Copiar variables de entorno:**
```bash
cp .env.example .env
```

2. **Configurar URLs de API en `.env`:**
```env
VITE_API_URL=http://localhost:5005          # Usuarios API
VITE_ASIENTOS_API_URL=http://localhost:5003 # Asientos API
VITE_EVENTOS_API_URL=http://localhost:5001  # Eventos API
```

3. **Instalar dependencias (si es necesario):**
```bash
npm install axios
```

4. **Ejecutar el proyecto:**
```bash
npm run dev
```

## 📝 Flujo de Usuario

### Registro
1. Usuario navega a `/register`
2. Completa el formulario
3. Sistema valida los datos
4. Se crea cuenta en Keycloak + BD local
5. Redirección a login con mensaje de éxito

### Compra de Entradas
1. Usuario ve evento y hace clic en "Comprar Ticket"
2. Navega a `/checkout/{eventoId}`
3. Se carga el mapa de asientos del evento
4. Usuario selecciona asientos disponibles
5. Ve el precio total en tiempo real
6. Hace clic en "Pagar Tickets"
7. Se procesa la compra (batch o individual)

## 🎯 Endpoints Utilizados

### Usuarios API (Puerto 5005)
- `POST /api/Usuarios` - Crear usuario

### Asientos API (Puerto 5003)
- `GET /api/mapas/evento/{eventoId}` - Obtener mapa del evento
- `GET /api/asientos/mapa/{mapaId}` - Obtener asientos del mapa
- `POST /api/asientos/reservar` - Reservar asiento
- `POST /api/asientos/liberar` - Liberar asiento

## 🎨 Componentes Clave

### `RegisterPage.tsx`
- Formulario de registro completo
- Validaciones en tiempo real
- Manejo de errores
- Loading states
- Diseño responsive

### `SeatMap.tsx`
- Renderizado de asientos por fila
- Estados visuales interactivos
- Selección/deselección de asientos
- Leyenda de estados
- Tooltips informativos

### `CheckoutPage.tsx`
- Layout de 2 columnas (mapa + resumen)
- Integración con SeatMap
- Cálculo de precio total
- Botón de pago condicional
- Información de política

## 🚨 Validaciones Importantes

### Registro
- Username: 3-50 caracteres
- Nombre: requerido, max 100 caracteres
- Email: formato válido
- Teléfono: opcional
- Dirección: opcional, min 5 caracteres si se proporciona
- Password: mínimo 8 caracteres
- Confirmación de password debe coincidir

### Checkout
- Solo se pueden seleccionar asientos disponibles
- No se puede comprar sin seleccionar asientos
- Los asientos reservados/ocupados son solo visuales

## 🎭 Próximas Mejoras Sugeridas

1. **Temporizador de Reserva**: Countdown de 10 minutos
2. **Integración con Pagos**: Conectar con pasarela de pago real
3. **Notificaciones**: Toast messages para feedback
4. **Persistencia**: Guardar selección en localStorage
5. **Autenticación**: Integrar con sistema OIDC completo
6. **Categorías Visuales**: Colores diferentes por categoría de asiento
7. **Vista 3D**: Renderizado 3D del mapa de asientos

## 📱 Responsive Design

Todos los componentes son completamente responsive:
- Mobile: Stack vertical
- Tablet: Grid adaptativo
- Desktop: Layout de 2-3 columnas

## 🎬 Demo

Para probar el flujo completo:

1. **Crear usuario:**
   - Ir a `http://localhost:5173/register`
   - Completar formulario
   - Verificar creación en Swagger (puerto 5005)

2. **Comprar entrada:**
   - Ir a página de eventos
   - Seleccionar un evento
   - Clic en "Comprar Ticket"
   - Seleccionar asientos
   - Confirmar compra

---

**Desarrollado con ❤️ usando React + TypeScript + Tailwind CSS**
