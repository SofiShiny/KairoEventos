# Módulo de Reportes

Este módulo maneja la funcionalidad de reportes y análisis del sistema, incluyendo métricas de eventos, historial de asistencia y conciliación financiera.

## Estructura

```
reportes/
├── components/       # Componentes UI para reportes
├── hooks/           # Custom hooks para reportes
├── pages/           # Páginas del módulo
├── services/        # Servicios API
├── types/           # Tipos TypeScript
└── index.ts         # Barrel export
```

## Servicios

### `reportesService.ts`

Servicios para comunicación con el API de reportes:

- `fetchMetricasEventos(filtros)` - Obtiene métricas de eventos
- `fetchHistorialAsistencia(filtros)` - Obtiene historial de asistencia de un evento
- `fetchConciliacionFinanciera(filtros)` - Obtiene datos de conciliación financiera
- `exportarReporte(params)` - Exporta reportes en diferentes formatos

## Hooks

### `useMetricasEventos(filtros)`

Hook para obtener métricas de eventos con React Query.

**Parámetros:**
- `filtros: ReporteFiltros` - Filtros opcionales (fechaInicio, fechaFin, eventoId)

**Retorna:**
- `data: MetricasEvento[]` - Array de métricas de eventos
- `isLoading: boolean` - Estado de carga
- `error: Error | null` - Error si ocurre

**Ejemplo:**
```typescript
const { data: metricas, isLoading, error } = useMetricasEventos({
  fechaInicio: new Date('2024-01-01'),
  fechaFin: new Date('2024-01-31'),
});
```

### `useHistorialAsistencia(filtros)`

Hook para obtener historial de asistencia de un evento.

**Parámetros:**
- `filtros: ReporteFiltros` - Debe incluir `eventoId`

**Retorna:**
- `data: HistorialAsistencia[]` - Array con datos de asistencia
- `isLoading: boolean` - Estado de carga
- `error: Error | null` - Error si ocurre

**Ejemplo:**
```typescript
const { data: historial, isLoading } = useHistorialAsistencia({
  eventoId: '123e4567-e89b-12d3-a456-426614174000',
});
```

### `useConciliacionFinanciera(filtros)`

Hook para obtener datos de conciliación financiera.

**Parámetros:**
- `filtros: ReporteFiltros` - Filtros opcionales (fechaInicio, fechaFin)

**Retorna:**
- `data: ConciliacionFinanciera` - Datos de conciliación
- `isLoading: boolean` - Estado de carga
- `error: Error | null` - Error si ocurre

**Ejemplo:**
```typescript
const { data: conciliacion, isLoading } = useConciliacionFinanciera({
  fechaInicio: new Date('2024-01-01'),
  fechaFin: new Date('2024-01-31'),
});
```

### `useExportarReporte()`

Hook para exportar reportes en diferentes formatos.

**Retorna:**
- `mutate: (params: ExportarReporteParams) => void` - Función para exportar
- `isPending: boolean` - Estado de exportación
- `error: Error | null` - Error si ocurre

**Ejemplo:**
```typescript
const { mutate: exportar, isPending } = useExportarReporte();

const handleExport = () => {
  exportar({
    tipo: 'metricas',
    formato: 'csv',
    filtros: { fechaInicio: new Date(), fechaFin: new Date() }
  });
};
```

## Tipos

### `ReporteFiltros`

Filtros para consultas de reportes:

```typescript
interface ReporteFiltros {
  fechaInicio?: Date;
  fechaFin?: Date;
  eventoId?: string;
}
```

### `MetricasEvento`

Métricas de un evento:

```typescript
interface MetricasEvento {
  eventoId: string;
  tituloEvento: string;
  totalAsistentes: number;
  estado: string;
  fechaPublicacion?: string;
  ultimaActualizacion: string;
}
```

### `HistorialAsistencia`

Historial de asistencia de un evento:

```typescript
interface HistorialAsistencia {
  eventoId: string;
  tituloEvento: string;
  totalAsistentes: number;
  asientosReservados: number;
  asientosDisponibles: number;
  capacidadTotal: number;
  porcentajeOcupacion: number;
  ultimaActualizacion: string;
}
```

### `ConciliacionFinanciera`

Datos de conciliación financiera:

```typescript
interface ConciliacionFinanciera {
  fechaInicio: string;
  fechaFin: string;
  totalIngresos: number;
  cantidadTransacciones: number;
  desglosePorCategoria: Record<string, number>;
  transacciones: Transaccion[];
}
```

## Endpoints del Gateway

El módulo se comunica con los siguientes endpoints del Gateway:

- `GET /api/reportes/metricas-evento/:eventoId` - Métricas de un evento
- `GET /api/reportes/resumen-ventas` - Resumen de ventas (usado para métricas múltiples)
- `GET /api/reportes/asistencia/:eventoId` - Historial de asistencia
- `GET /api/reportes/conciliacion-financiera` - Conciliación financiera

## Acceso

Este módulo es accesible solo para usuarios con roles:
- **Admin** - Acceso completo a todos los reportes
- **Organizator** - Acceso a reportes de sus propios eventos

## Notas de Implementación

1. **Caché**: Los datos de reportes se cachean por 2 minutos (menos frecuente que otros módulos)
2. **Filtros por defecto**: Si no se especifican fechas, el backend usa valores por defecto
3. **Exportación**: Actualmente soporta formato CSV, PDF y Excel están pendientes
4. **Validación**: El eventoId es requerido para historial de asistencia
5. **Descarga automática**: La exportación descarga automáticamente el archivo generado

## Próximos Pasos

- Implementar componentes UI (Task 17)
- Agregar soporte para exportación a PDF y Excel
- Implementar gráficos visuales con recharts
- Agregar más filtros avanzados
