# 🎫 Guía Rápida: Cómo Usar el Sistema de Cupones

## 📍 Ubicación en el Frontend

### Para Administradores

1. **Inicia sesión** como administrador
2. Ve a **Admin → Gestión de Eventos**
3. En la tabla de eventos, pasa el mouse sobre cualquier evento
4. Verás aparecer botones de acción:
   - ✏️ Editar
   - 🗑️ Eliminar
   - 📐 Gestionar Asientos
   - **🏷️ Gestionar Cupones** ← ¡NUEVO!
   - 🚀 Publicar (si está en borrador)

5. Haz clic en el botón **🏷️ (Tag morado)** para abrir el gestor de cupones

## 🎨 Interfaz del Gestor de Cupones

El modal que se abre tiene **3 pestañas**:

### 📋 Pestaña 1: "Lista de Cupones"
- Muestra todos los cupones creados para el evento
- Columnas:
  - **Código**: El código del cupón (ej: PROMO2026)
  - **Descuento**: Monto en dólares
  - **Tipo**: General (reutilizable) o Único (un solo uso)
  - **Estado**: Activo ✅ / Usado 🔵 / Expirado ❌
  - **Expira**: Fecha de expiración
  - **Usos**: Ilimitado o 1 uso

- **Acciones disponibles**:
  - 📋 **Copiar Todos**: Copia todos los códigos al portapapeles
  - 📥 **Exportar CSV**: Descarga archivo CSV con todos los cupones

### ➕ Pestaña 2: "Crear Promoción"
**Para campañas de marketing con un código único**

Ejemplo: Crear "VERANO2026" para newsletter

**Campos**:
- **Código del Cupón**: Escribe el código (se convierte a mayúsculas automáticamente)
  - Ejemplo: `VERANO2026`, `BLACKFRIDAY`, `PROMO50`
- **Descuento ($)**: Monto en dólares
  - Ejemplo: `20.00` = $20 de descuento
- **Fecha de Expiración** (opcional): Hasta cuándo es válido
  - Ejemplo: `31/12/2026`
- **☑️ Cupón global**: Si lo marcas, el cupón funciona para TODOS los eventos, no solo este

**Clic en "Crear Cupón"** → ✅ Cupón creado

### 🎲 Pestaña 3: "Generar Lote"
**Para sorteos, regalos o distribución masiva**

Ejemplo: 100 cupones únicos de $10 para sorteo

**Campos**:
- **Cantidad de Cupones**: Cuántos códigos generar (máx. 1000)
  - Ejemplo: `50`, `100`, `500`
- **Descuento por Cupón ($)**: Monto que descuenta cada uno
  - Ejemplo: `10.00` = cada cupón descuenta $10
- **Fecha de Expiración** (opcional): Validez de todos los cupones
  - Ejemplo: `15/02/2026`

**Clic en "Generar Lote"** → ✅ Se crean N cupones con códigos aleatorios

**Luego puedes**:
- Ver todos en la pestaña "Lista"
- Exportar CSV para enviar por email
- Copiar todos los códigos

## 💳 Cómo lo Usan los Usuarios

### En el Checkout

1. Usuario selecciona asientos
2. Ve el resumen de compra con el subtotal
3. Aparece un campo: **"¿Tienes un cupón de descuento?"**
4. Ingresa el código (ej: `PROMO2026`)
5. Clic en **"Aplicar"**
6. Si es válido:
   - ✅ Mensaje verde: "¡Cupón PROMO2026 aplicado! Ahorraste $20.00"
   - 💚 El resumen muestra:
     ```
     Subtotal: $100.00
     Descuento (PROMO2026): -$20.00
     Total a Pagar: $80.00 ✨
     ```
   - El total cambia de color morado a verde
7. Procede al pago con el precio con descuento

### Caso Especial: Entrada Gratis
Si el cupón descuenta el 100%:
```
Subtotal: $50.00
Descuento (GRATIS100): -$50.00
Total a Pagar: $0.00
🎉 ¡Entrada gratis con tu cupón!
```

## 📊 Casos de Uso Reales

### Caso 1: Promoción de Newsletter
```
Admin:
1. Pestaña "Crear Promoción"
2. Código: NEWSLETTER2026
3. Descuento: $15
4. Fecha: 31/12/2026
5. Crear

Usuario:
1. Recibe email con código NEWSLETTER2026
2. Va al checkout
3. Aplica cupón
4. Ahorra $15
```

### Caso 2: Sorteo de 50 Entradas con Descuento
```
Admin:
1. Pestaña "Generar Lote"
2. Cantidad: 50
3. Descuento: $25
4. Generar
5. Exportar CSV
6. Enviar códigos a ganadores

Ganador:
1. Recibe código único: ABC123XYZ
2. Usa el código en checkout
3. Descuento de $25 aplicado
4. El código queda marcado como "Usado"
```

### Caso 3: Cupón Global para Todos los Eventos
```
Admin:
1. Pestaña "Crear Promoción"
2. Código: BIENVENIDA2026
3. Descuento: $10
4. ✅ Marcar "Cupón global"
5. Crear

Usuario:
- Puede usar BIENVENIDA2026 en CUALQUIER evento
- Descuento de $10 en todos
```

## 🎯 Ubicación de los Botones

En la tabla de eventos, cuando pasas el mouse sobre una fila, aparecen estos botones:

```
[✏️ Editar] [🗑️ Eliminar] [📐 Asientos] [🏷️ Cupones] [🚀 Publicar]
                                          ↑
                                    ¡ESTE ES!
                                 (icono de etiqueta morado)
```

## 🔍 Verificación Rápida

**¿Ves el botón de cupones?**
- ✅ Sí → Pasa el mouse sobre cualquier evento en la tabla
- ❌ No → Verifica que estés en "Admin → Gestión de Eventos"

**¿El modal se abre?**
- ✅ Sí → Verás 3 pestañas: Lista / Crear / Lote
- ❌ No → Revisa la consola del navegador (F12)

## 💡 Tips

1. **Códigos cortos y memorables**: `VERANO2026` es mejor que `DESC20PERCENT2026SUMMER`
2. **Fechas de expiración**: Crea urgencia (ej: válido solo 1 semana)
3. **Exporta CSV**: Guarda los códigos generados para tus registros
4. **Cupones globales**: Úsalos para promociones de bienvenida
5. **Lotes grandes**: Para eventos masivos, genera 500-1000 cupones

---

**¿Necesitas ayuda?** Revisa la consola del navegador (F12) para ver logs de depuración.
