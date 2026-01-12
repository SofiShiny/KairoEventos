# Sistema de Gestión de Eventos

Sistema web para gestión de eventos con reserva de asientos y categorías.

## Requisitos

- Node.js 18+

## Instalación

```bash
cd frontend
npm install
```

## Configuración

### Backend URLs (Azure Brazil South)

El frontend se conecta a los siguientes servicios:

- **Eventos**: `https://contenedor-evento.jollybay-7b230335.brazilsouth.azurecontainerapps.io`
- **Asientos**: `https://contenedor-asiento.jollybay-7b230335.brazilsouth.azurecontainerapps.io`

La configuración del proxy está en `frontend/vite.config.ts`.

### Variables de Entorno

Archivo `frontend/.env`:

```
VITE_EVENTS_API_URL=http://localhost:5173
VITE_SEATS_API_URL=http://localhost:5173
```

Las peticiones se redirigen al backend de Azure mediante el proxy de Vite.

## Comandos

### Desarrollo

```bash
cd frontend
npm run dev
```

Abre http://localhost:5173

### Build

```bash
cd frontend
npm run build
```

### Tests

```bash
cd frontend
npm test
```

## Funcionalidades

### Administrador/Organizador
- Crear y editar eventos
- Crear categorías de asientos (nombre, precio, prioridad)
- Crear asientos con categoría asignada
- Visualizar mapa de asientos

### Usuario
- Ver eventos disponibles
- Reservar asientos por categoría
- Ver precio por categoría

## Rutas

- `/usuario` - Vista pública de eventos
- `/usuario/evento/:id` - Detalle y reserva de asientos
- `/admin/dashboard` - Panel de administrador
- `/admin/crear` - Crear evento
- `/admin/editar/:id` - Editar evento
- `/organizador/dashboard` - Panel de organizador
- `/organizador/crear` - Crear evento
- `/organizador/editar/:id` - Editar evento

