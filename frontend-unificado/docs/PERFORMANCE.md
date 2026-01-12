# Performance Optimizations

This document describes the performance optimizations implemented in the Frontend Unificado application.

## Overview

The application implements multiple performance optimization strategies to ensure fast load times, smooth interactions, and efficient resource usage.

## Implemented Optimizations

### 1. Code Splitting with Lazy Loading

**Location**: `src/routes/AppRoutes.tsx`

All non-critical routes are lazy-loaded using React's `lazy()` and `Suspense`:

```typescript
const EventosPage = lazy(() =>
  import('../modules/eventos/pages').then((m) => ({ default: m.EventosPage }))
);
```

**Benefits**:
- Reduces initial bundle size
- Faster initial page load
- Routes are loaded on-demand

**Critical routes** (LoginPage, DashboardPage) are loaded eagerly for immediate availability.

### 2. Lazy Loading of Images

**Location**: `src/shared/components/ImagePlaceholder.tsx`

All images use native browser lazy loading:

```typescript
<img loading="lazy" src={src} alt={alt} />
```

**Benefits**:
- Images below the fold are not loaded until needed
- Reduces initial bandwidth usage
- Improves perceived performance

**Usage**:
```typescript
<ImagePlaceholder
  src={evento.imagenUrl}
  alt={evento.nombre}
  loading="lazy" // Default value
/>
```

### 3. Data Prefetching with React Query

**Location**: 
- `src/shared/config/queryClient.ts`
- `src/modules/eventos/hooks/useEventos.ts`

Prefetching loads data before the user needs it:

```typescript
// Prefetch on hover
const prefetchEvento = usePrefetchEvento();
<EventoCard onMouseEnter={() => prefetchEvento(evento.id)} />
```

**Benefits**:
- Instant navigation to detail pages
- Better perceived performance
- Reduced loading states

**Configuration**:
```typescript
export const prefetchQuery = async <TData>(
  queryKey: unknown[],
  queryFn: () => Promise<TData>,
  staleTime?: number
) => {
  await queryClient.prefetchQuery({
    queryKey,
    queryFn,
    staleTime: staleTime ?? 5 * 60 * 1000,
  });
};
```

### 4. Optimistic Updates

**Location**: `src/shared/hooks/useMutationWithInvalidation.ts`

Optimistic updates improve UX by updating the UI immediately:

```typescript
export function useMutationWithOptimisticUpdate<TData, TVariables, TError = Error>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  queryKey: unknown[],
  optimisticUpdateFn: (oldData: TData | undefined, variables: TVariables) => TData,
  options?: Omit<UseMutationOptions<TData, TError, TVariables, TData | undefined>, 'mutationFn'>
)
```

**Benefits**:
- Instant UI feedback
- Better perceived performance
- Automatic rollback on error

**Usage Example**:
```typescript
const updateEvento = useMutationWithOptimisticUpdate(
  (data: UpdateEventoDto) => eventosService.update(data.id, data),
  ['eventos'],
  (oldData: Evento[], newData: UpdateEventoDto) => {
    return oldData.map(e => e.id === newData.id ? { ...e, ...newData } : e);
  }
);
```

### 5. Component Memoization

**Location**: 
- `src/modules/eventos/components/EventoCard.tsx`
- `src/modules/eventos/components/EventosList.tsx`

Heavy components are memoized to prevent unnecessary re-renders:

```typescript
// Memoize component
const EventoCard = memo(EventoCardComponent);

// Memoize callbacks
const handleClick = useCallback(() => {
  // ...
}, [dependencies]);

// Memoize computed values
const disponibilidadPorcentaje = useMemo(
  () => Math.round((asientosDisponibles / capacidadTotal) * 100),
  [asientosDisponibles, capacidadTotal]
);
```

**Benefits**:
- Prevents unnecessary re-renders
- Reduces CPU usage
- Smoother scrolling and interactions

**When to use**:
- Components that render frequently
- Components with expensive computations
- Components in lists

### 6. Manual Chunks (Vendor Splitting)

**Location**: `vite.config.ts`

Vendor code is split into separate chunks for better caching:

```typescript
build: {
  rollupOptions: {
    output: {
      manualChunks: {
        'react-vendor': ['react', 'react-dom', 'react-router-dom'],
        'mui-vendor': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
        'form-vendor': ['react-hook-form', 'zod', '@hookform/resolvers'],
        'query-vendor': ['@tanstack/react-query'],
        'auth-vendor': ['react-oidc-context', 'oidc-client-ts'],
      },
    },
  },
}
```

**Benefits**:
- Better browser caching
- Smaller individual chunks
- Faster updates (vendor code rarely changes)

**Chunk Strategy**:
- `react-vendor`: Core React libraries
- `mui-vendor`: Material UI components
- `form-vendor`: Form handling libraries
- `query-vendor`: React Query
- `auth-vendor`: Authentication libraries

## React Query Configuration

**Location**: `src/shared/config/queryClient.ts`

Optimized cache configuration:

```typescript
{
  staleTime: 5 * 60 * 1000,      // 5 minutes - data stays fresh
  gcTime: 10 * 60 * 1000,        // 10 minutes - cache retention
  retry: 2,                       // Retry failed requests
  refetchOnWindowFocus: true,     // Refresh on window focus
  refetchOnReconnect: true,       // Refresh on reconnect
  refetchOnMount: false,          // Don't refetch if cached
}
```

## Performance Metrics

### Bundle Size Targets

- Initial bundle: < 300KB (gzipped)
- Vendor chunks: < 200KB each (gzipped)
- Route chunks: < 100KB each (gzipped)

### Loading Performance

- First Contentful Paint (FCP): < 1.5s
- Largest Contentful Paint (LCP): < 2.5s
- Time to Interactive (TTI): < 3.5s
- Cumulative Layout Shift (CLS): < 0.1

### Runtime Performance

- Component re-renders: Minimized with memo/useMemo/useCallback
- List rendering: Optimized with proper keys and memoization
- Image loading: Lazy loaded below the fold

## Best Practices

### When to Use Memoization

✅ **Use memo/useMemo/useCallback when**:
- Component renders frequently
- Component has expensive computations
- Component is in a list
- Props are objects/arrays/functions

❌ **Don't use when**:
- Component rarely re-renders
- Computation is trivial
- Premature optimization

### When to Use Lazy Loading

✅ **Lazy load**:
- Non-critical routes
- Images below the fold
- Heavy components not immediately visible

❌ **Don't lazy load**:
- Critical path components
- Above-the-fold content
- Small components

### When to Use Prefetching

✅ **Prefetch**:
- On hover (detail pages)
- On route change (next likely page)
- On idle (background prefetch)

❌ **Don't prefetch**:
- Large datasets
- Rarely accessed data
- On slow connections

## Monitoring Performance

### Development

Use React DevTools Profiler to identify:
- Unnecessary re-renders
- Expensive components
- Render bottlenecks

### Production

Monitor with Web Vitals:
```typescript
import { getCLS, getFID, getFCP, getLCP, getTTFB } from 'web-vitals';

getCLS(console.log);
getFID(console.log);
getFCP(console.log);
getLCP(console.log);
getTTFB(console.log);
```

## Future Optimizations

Potential improvements for future iterations:

1. **Virtual Scrolling**: For very long lists (>100 items)
2. **Service Worker**: For offline support and caching
3. **Image Optimization**: WebP format, responsive images
4. **Bundle Analysis**: Regular analysis with `vite-bundle-visualizer`
5. **CDN**: Serve static assets from CDN
6. **HTTP/2 Server Push**: Push critical resources
7. **Preconnect**: Preconnect to API domains

## Resources

- [React Performance Optimization](https://react.dev/learn/render-and-commit)
- [React Query Performance](https://tanstack.com/query/latest/docs/react/guides/performance)
- [Vite Performance](https://vitejs.dev/guide/performance.html)
- [Web Vitals](https://web.dev/vitals/)
