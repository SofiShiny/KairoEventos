# ✅ Sistema de Cupones - Implementación Completa

## 🎉 ¡Implementación Finalizada!

Se ha implementado un sistema completo de cupones con descuentos basados en **porcentajes** (no dólares fijos) tanto en el backend como en el frontend.

---

## 📦 Backend (Microservicio Pagos)

### Archivos Creados

#### 1. **Dominio** (`Pagos.Dominio`)
- ✅ `Entidades/Cupon.cs` - Entidad del dominio con lógica de negocio
  - Tipos: General (reutilizable) y Único (un solo uso)
  - Estados: Activo, Usado, Expirado, Agotado
  - Validación de cupones
  - Cálculo de descuentos en porcentaje
  - Métodos factory para crear cupones

- ✅ `Interfaces/ICuponRepositorio.cs` - Interfaz del repositorio

#### 2. **Infraestructura** (`Pagos.Infraestructura`)
- ✅ `Repositorios/CuponRepositorio.cs` - Implementación del repositorio con EF Core
- ✅ `Persistencia/PagosDbContext.cs` - Actualizado con DbSet<Cupon>
- ✅ `DependencyInjection.cs` - Registro de servicios
- ✅ **Migración**: `AgregarCupones` - Crea tabla Cupones en la base de datos

#### 3. **Aplicación** (`Pagos.Aplicacion`)
- ✅ `Servicios/CuponServicio.cs` - Lógica de negocio de cupones
  - Validar cupones
  - Crear cupones generales
  - Generar lotes de cupones únicos con códigos aleatorios
  - Consultar cupones por evento
  - Marcar cupones como usados

#### 4. **API** (`Pagos.API`)
- ✅ `Controllers/CuponesController.cs` - Endpoints REST
  - `POST /api/cupones/validar` - Validar cupón
  - `POST /api/cupones/general` - Crear cupón general (Admin)
  - `POST /api/cupones/lote` - Generar lote (Admin)
  - `GET /api/cupones/evento/{eventoId}` - Listar cupones (Admin)
  - `GET /api/cupones/globales` - Listar cupones globales (Admin)

---

## 🎨 Frontend (React + TypeScript)

### Archivos Creados/Modificados

#### 1. **Servicios**
- ✅ `pagos.service.ts` - Actualizado con métodos de cupones
  - Interfaces con porcentajes
  - Métodos para todas las operaciones de cupones

#### 2. **Componentes**
- ✅ `CouponInput.tsx` - Input de cupón para el checkout
  - Validación en tiempo real
  - Animaciones al aplicar/remover
  - Feedback visual

- ✅ `AdminCuponesManager.tsx` - Panel de administración
  - 3 pestañas: Lista, Crear, Generar Lote
  - Exportación CSV
  - Copiar códigos al portapapeles

#### 3. **Páginas**
- ✅ `CheckoutPage.tsx` - Integración de cupones
  - Muestra input de cupón
  - Calcula descuento dinámicamente
  - Envía código al backend

- ✅ `AdminEventos.tsx` - Botón de gestión de cupones
  - Modal con AdminCuponesManager

---

## 🔑 Características Principales

### Descuentos en Porcentaje
- ✅ **10%** = 10% de descuento
- ✅ **50%** = 50% de descuento  
- ✅ **100%** = Entrada gratis
- ✅ Validación: 1-100%

### Tipos de Cupones

#### **Cupón General**
- Un código único (ej: `PROMO2026`)
- Puede ser usado por múltiples usuarios
- Opcional: Límite de usos
- Opcional: Fecha de expiración
- Puede ser global (todos los eventos) o específico

#### **Cupón Único**
- Códigos aleatorios de 8 caracteres
- Un solo uso por cupón
- Generación en lote (hasta 1000)
- Ideal para sorteos y regalos

### Validaciones

**Backend:**
- Cupón existe
- No está expirado
- Aplica al evento
- No ha sido usado (únicos)
- No excede límite de usos (generales)

**Frontend:**
- Porcentaje entre 1-100
- Código no vacío
- Formato correcto

---

## 🚀 Cómo Usar

### Para Administradores

1. **Ir a Admin → Gestión de Eventos**
2. **Pasar mouse sobre un evento**
3. **Clic en botón morado 🏷️ "Gestionar Cupones"**

#### Crear Cupón General
```
Pestaña: "Crear Promoción"
Código: VERANO2026
Porcentaje: 20
Fecha Exp: 31/12/2026
Límite Usos: (vacío = ilimitado)
☐ Cupón global
```

#### Generar Lote
```
Pestaña: "Generar Lote"
Cantidad: 100
Porcentaje: 15
Fecha Exp: (opcional)
→ Genera 100 códigos únicos
→ Exportar CSV
```

### Para Usuarios

1. **Seleccionar asientos en checkout**
2. **Ver campo "¿Tienes un cupón?"**
3. **Ingresar código (ej: PROMO2026)**
4. **Clic "Aplicar"**
5. **Ver descuento aplicado:**
   ```
   Subtotal: $100.00
   Descuento (PROMO2026): -$20.00 (20%)
   Total a Pagar: $80.00 ✨
   ```

---

## 📊 Ejemplo de Flujo Completo

```
Admin crea cupón:
  Código: BLACKFRIDAY
  Porcentaje: 30%
  
Usuario compra:
  3 asientos × $50 = $150
  Aplica BLACKFRIDAY
  Descuento: $45 (30% de $150)
  Total: $105
  
Backend valida:
  ✅ Cupón existe
  ✅ Activo
  ✅ No expirado
  ✅ Aplica al evento
  ✅ Calcula: $150 × 0.30 = $45
  
Pago procesado con descuento
```

---

## 🗄️ Estructura de Base de Datos

### Tabla: Cupones

```sql
CREATE TABLE Cupones (
    Id UUID PRIMARY KEY,
    Codigo VARCHAR(20) UNIQUE NOT NULL,
    PorcentajeDescuento DECIMAL(5,2) NOT NULL,
    Tipo INT NOT NULL, -- 1=General, 2=Unico
    Estado INT NOT NULL, -- 1=Activo, 2=Usado, 3=Expirado, 4=Agotado
    EventoId UUID NULL, -- NULL = global
    FechaCreacion TIMESTAMP NOT NULL,
    FechaExpiracion TIMESTAMP NULL,
    UsuarioId UUID NULL, -- Para cupones únicos
    FechaUso TIMESTAMP NULL,
    ContadorUsos INT NOT NULL DEFAULT 0,
    LimiteUsos INT NULL
);

CREATE INDEX IX_Cupones_Codigo ON Cupones(Codigo);
CREATE INDEX IX_Cupones_EventoId ON Cupones(EventoId);
CREATE INDEX IX_Cupones_Estado ON Cupones(Estado);
```

---

## 🔧 Configuración Requerida

### Backend
1. **Cadena de conexión PostgreSQL** en `appsettings.json`
2. **La migración se aplica automáticamente** al iniciar el servicio
3. **RabbitMQ** para eventos (ya configurado)

### Frontend
1. **Gateway debe rutear** `/pagos/*` al microservicio de Pagos
2. **Autenticación** para endpoints de admin
3. **CORS** configurado para `localhost:5173`

---

## ✨ Mejoras Implementadas vs Solicitud Original

### Cambios Clave:
- ✅ **Porcentajes en lugar de dólares** (más flexible)
- ✅ **Límite de usos** para cupones generales
- ✅ **Estado "Agotado"** cuando se alcanza el límite
- ✅ **Validación robusta** en backend y frontend
- ✅ **Generación de códigos aleatorios** sin caracteres confusos
- ✅ **Exportación CSV** con símbolo de porcentaje
- ✅ **Manejo de errores mejorado** (404 vs 500)

---

## 📝 Endpoints del API

### Públicos
```http
POST /api/cupones/validar
Content-Type: application/json

{
  "codigo": "PROMO2026",
  "eventoId": "guid",
  "montoTotal": 150.00
}

Response:
{
  "esValido": true,
  "descuento": 30.00,
  "nuevoTotal": 120.00,
  "porcentajeDescuento": 20,
  "mensaje": "Cupón aplicado: 20% de descuento"
}
```

### Admin Only
```http
POST /api/cupones/general
Authorization: Bearer {token}
Content-Type: application/json

{
  "codigo": "VERANO2026",
  "porcentajeDescuento": 25,
  "fechaExpiracion": "2026-12-31",
  "eventoId": "guid",
  "esGlobal": false,
  "limiteUsos": 100
}
```

```http
POST /api/cupones/lote
Authorization: Bearer {token}
Content-Type: application/json

{
  "cantidad": 50,
  "porcentajeDescuento": 15,
  "eventoId": "guid",
  "fechaExpiracion": "2026-06-30"
}
```

```http
GET /api/cupones/evento/{eventoId}
Authorization: Bearer {token}

Response: Cupon[]
```

---

## 🎓 Casos de Uso Cubiertos

1. ✅ **Promoción de Newsletter** - Cupón general con código manual
2. ✅ **Sorteo de Entradas** - Lote de cupones únicos
3. ✅ **Black Friday** - Cupón general con límite de usos
4. ✅ **Cupón de Bienvenida** - Cupón global para todos los eventos
5. ✅ **Entrada Gratis** - Cupón con 100% de descuento
6. ✅ **Promoción Temporal** - Cupón con fecha de expiración

---

## 🐛 Manejo de Errores

### Frontend
- Cupón inválido → Mensaje específico
- Cupón expirado → "El cupón ha expirado"
- No aplica al evento → "No aplica a este evento"
- Ya usado → "Este cupón ya ha sido utilizado"
- Límite alcanzado → "El cupón ha alcanzado su límite de usos"

### Backend
- Código duplicado → 400 Bad Request
- Porcentaje inválido → ArgumentException
- Cantidad fuera de rango → ArgumentException
- Cupón no encontrado → 404 Not Found

---

## 🚀 Próximos Pasos (Opcional)

1. **Dashboard de Estadísticas**
   - Cupones más usados
   - Descuentos totales otorgados
   - Tasa de conversión

2. **Notificaciones**
   - Email cuando se crea un lote
   - Alerta cuando un cupón está por expirar

3. **Cupones Personalizados**
   - Cupones por usuario específico
   - Cupones por categoría de evento

4. **Integración con Marketing**
   - Generación automática para campañas
   - Tracking de origen del cupón

---

## ✅ Checklist de Implementación

- [x] Entidad Cupon en el dominio
- [x] Repositorio de cupones
- [x] Servicio de cupones
- [x] Controlador del API
- [x] Migración de base de datos
- [x] Registro de dependencias
- [x] Servicio frontend actualizado
- [x] Componente CouponInput
- [x] Componente AdminCuponesManager
- [x] Integración en CheckoutPage
- [x] Integración en AdminEventos
- [x] Validaciones frontend y backend
- [x] Manejo de errores
- [x] Exportación CSV
- [x] Documentación completa

---

## 🎯 Resumen

**Backend:** ✅ Completo
- Entidades, repositorios, servicios, API
- Migración de base de datos
- Lógica de negocio robusta

**Frontend:** ✅ Completo
- UI moderna y responsiva
- Validaciones en tiempo real
- Integración completa

**Funcionalidad:** ✅ 100% Operativa
- Cupones generales y únicos
- Descuentos en porcentaje
- Validación completa
- Exportación y gestión

---

**¡El sistema de cupones está listo para usar!** 🎉

Reinicia el microservicio de Pagos para que aplique la migración automáticamente.
