# React Query Quick Start Guide

Quick reference for using React Query in the Frontend Unificado application.

## Basic Patterns

### 1. Fetch Data (Query)

```tsx
import { useQuery } from '@tanstack/react-query';

const { data, isLoading, error } = useQuery({
  queryKey: ['eventos'],
  queryFn: eventosService.fetchAll,
});
```

### 2. Fetch with Parameters

```tsx
const { data: evento } = useQuery({
  queryKey: ['eventos', eventoId],
  queryFn: () => eventosService.fetchById(eventoId),
  enabled: !!eventoId,
});
```

### 3. Create/Update/Delete (Mutation with Auto-Invalidation)

```tsx
import { useMutationWithInvalidation } from '@shared/hooks';

const createEvento = useMutationWithInvalidation(
  (data: CreateEventoDto) => eventosService.create(data),
  ['eventos'], // Queries to invalidate
);

// Use it
createEvento.mutate(formData);
```

### 4. Manual Cache Invalidation

```tsx
import { useInvalidateQueries } from '@shared/hooks';

const invalidate = useInvalidateQueries();

// After some operation
invalidate(['eventos', 'dashboard-stats']);
```

## Query Keys Convention

```tsx
['eventos']                    // All eventos
['eventos', eventoId]          // Specific evento
['eventos', 'paginated', page] // Paginated eventos
['mis-entradas']               // User's entradas
['mis-entradas', filtro]       // Filtered entradas
```

## Loading & Error States

```tsx
if (isLoading) return <LoadingSpinner />;
if (error) return <ErrorMessage error={error} onRetry={refetch} />;
```

## Mutation States

```tsx
<button disabled={mutation.isPending}>
  {mutation.isPending ? 'Saving...' : 'Save'}
</button>

{mutation.isSuccess && <p>✅ Success!</p>}
{mutation.isError && <p>❌ Error: {mutation.error.message}</p>}
```

## Common Options

```tsx
useQuery({
  queryKey: ['eventos'],
  queryFn: fetchEventos,
  staleTime: 5 * 60 * 1000,    // 5 minutes
  enabled: isAuthenticated,     // Conditional fetching
  refetchOnWindowFocus: true,   // Refetch on focus
});
```

## Full Documentation

See [REACT-QUERY.md](./REACT-QUERY.md) for complete documentation and advanced patterns.
