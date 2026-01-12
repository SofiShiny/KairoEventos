# React Query Configuration and Usage

This document describes how React Query is configured and used in the Frontend Unificado application.

## Configuration

React Query is configured in `src/shared/config/queryClient.ts` with the following default options:

### Query Options

- **staleTime**: 5 minutes - Data is considered fresh for 5 minutes
- **gcTime**: 10 minutes - Data remains in cache for 10 minutes after last use
- **retry**: 2 attempts - Failed queries are retried up to 2 times
- **retryDelay**: Exponential backoff - Delay between retries increases exponentially
- **refetchOnWindowFocus**: true - Data is refetched when window regains focus
- **refetchOnReconnect**: true - Data is refetched when network reconnects
- **refetchOnMount**: false - Data is not refetched on component mount if already in cache

### Mutation Options

- **retry**: 1 attempt - Failed mutations are retried once
- **retryDelay**: 1 second - Fixed delay between mutation retries

## Setup

React Query is set up in `src/main.tsx`:

```tsx
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from '@shared/config';

<QueryClientProvider client={queryClient}>
  <AppAuthProvider>
    <App />
  </AppAuthProvider>
</QueryClientProvider>
```

## Cache Management

### Automatic Cache Invalidation

The application automatically clears the React Query cache when the user logs out. This is handled in `AuthContext.tsx`:

```tsx
const logout = () => {
  clearQueryCache(); // Clear React Query cache
  localStorage.removeItem('auth_token');
  sessionStorage.clear();
  auth.signoutRedirect();
};
```

### Manual Cache Invalidation

Use the `useInvalidateQueries` hook to manually invalidate queries:

```tsx
import { useInvalidateQueries } from '@shared/hooks';

function MyComponent() {
  const invalidate = useInvalidateQueries();

  const handleUpdate = async () => {
    await updateData();
    // Invalidate multiple queries at once
    invalidate(['eventos', 'dashboard-stats']);
  };
}
```

## Usage Patterns

### 1. Fetching Data (Queries)

```tsx
import { useQuery } from '@tanstack/react-query';
import { eventosService } from '@modules/eventos/services';

function EventosList() {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['eventos'],
    queryFn: eventosService.fetchAll,
  });

  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage error={error} onRetry={refetch} />;

  return (
    <div>
      {data?.map(evento => (
        <EventoCard key={evento.id} evento={evento} />
      ))}
    </div>
  );
}
```

### 2. Fetching Data with Parameters

```tsx
function EventoDetail({ eventoId }: { eventoId: string }) {
  const { data: evento, isLoading } = useQuery({
    queryKey: ['eventos', eventoId],
    queryFn: () => eventosService.fetchById(eventoId),
    enabled: !!eventoId, // Only fetch if eventoId exists
  });

  // ...
}
```

### 3. Mutations with Cache Invalidation

Use the `useMutationWithInvalidation` hook for automatic cache invalidation:

```tsx
import { useMutationWithInvalidation } from '@shared/hooks';
import { eventosService } from '@modules/eventos/services';

function CreateEventoForm() {
  const createEvento = useMutationWithInvalidation(
    (data: CreateEventoDto) => eventosService.create(data),
    ['eventos', 'dashboard-stats'], // Queries to invalidate
    {
      onSuccess: () => {
        toast.success('Evento creado exitosamente');
        navigate('/eventos');
      },
      onError: (error) => {
        toast.error('Error al crear evento');
      },
    }
  );

  const handleSubmit = (data: CreateEventoDto) => {
    createEvento.mutate(data);
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* form fields */}
      <button type="submit" disabled={createEvento.isPending}>
        {createEvento.isPending ? 'Creando...' : 'Crear Evento'}
      </button>
    </form>
  );
}
```

### 4. Manual Mutation with Invalidation

If you need more control, use `useMutation` directly:

```tsx
import { useMutation, useQueryClient } from '@tanstack/react-query';

function UpdateEventoForm({ eventoId }: { eventoId: string }) {
  const queryClient = useQueryClient();

  const updateEvento = useMutation({
    mutationFn: (data: UpdateEventoDto) => eventosService.update(eventoId, data),
    onSuccess: () => {
      // Invalidate specific queries
      queryClient.invalidateQueries({ queryKey: ['eventos'] });
      queryClient.invalidateQueries({ queryKey: ['eventos', eventoId] });
      toast.success('Evento actualizado');
    },
  });

  // ...
}
```

### 5. Optimistic Updates

For better UX, you can implement optimistic updates:

```tsx
const updateEvento = useMutation({
  mutationFn: (data: UpdateEventoDto) => eventosService.update(eventoId, data),
  onMutate: async (newData) => {
    // Cancel outgoing refetches
    await queryClient.cancelQueries({ queryKey: ['eventos', eventoId] });

    // Snapshot previous value
    const previousEvento = queryClient.getQueryData(['eventos', eventoId]);

    // Optimistically update cache
    queryClient.setQueryData(['eventos', eventoId], newData);

    // Return context with snapshot
    return { previousEvento };
  },
  onError: (err, newData, context) => {
    // Rollback on error
    queryClient.setQueryData(['eventos', eventoId], context?.previousEvento);
  },
  onSettled: () => {
    // Refetch after error or success
    queryClient.invalidateQueries({ queryKey: ['eventos', eventoId] });
  },
});
```

### 6. Dependent Queries

Fetch data that depends on other data:

```tsx
function EventoWithAsientos({ eventoId }: { eventoId: string }) {
  // First query
  const { data: evento } = useQuery({
    queryKey: ['eventos', eventoId],
    queryFn: () => eventosService.fetchById(eventoId),
  });

  // Second query depends on first
  const { data: asientos } = useQuery({
    queryKey: ['asientos', eventoId],
    queryFn: () => asientosService.fetchByEvento(eventoId),
    enabled: !!evento, // Only fetch if evento exists
  });

  // ...
}
```

### 7. Pagination

```tsx
function EventosPaginated() {
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['eventos', 'paginated', page],
    queryFn: () => eventosService.fetchPaginated(page, 10),
    keepPreviousData: true, // Keep previous data while fetching new page
  });

  return (
    <div>
      {data?.items.map(evento => <EventoCard key={evento.id} evento={evento} />)}
      <Pagination
        page={page}
        totalPages={data?.totalPages}
        onPageChange={setPage}
      />
    </div>
  );
}
```

## Best Practices

1. **Use Consistent Query Keys**: Use arrays for query keys and include all parameters that affect the query
   ```tsx
   ['eventos'] // All eventos
   ['eventos', eventoId] // Specific evento
   ['eventos', 'paginated', page] // Paginated eventos
   ```

2. **Invalidate Related Queries**: When mutating data, invalidate all related queries
   ```tsx
   // After creating an evento
   invalidate(['eventos', 'dashboard-stats', 'eventos-destacados']);
   ```

3. **Handle Loading and Error States**: Always provide feedback to users
   ```tsx
   if (isLoading) return <LoadingSpinner />;
   if (error) return <ErrorMessage error={error} onRetry={refetch} />;
   ```

4. **Use Enabled Option**: Prevent unnecessary queries
   ```tsx
   enabled: !!userId && isAuthenticated
   ```

5. **Clear Cache on Logout**: The application automatically clears the cache on logout

6. **Use Optimistic Updates for Better UX**: Update UI immediately and rollback on error

## Debugging

### React Query DevTools

To enable React Query DevTools in development, add to `main.tsx`:

```tsx
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';

<QueryClientProvider client={queryClient}>
  <App />
  <ReactQueryDevtools initialIsOpen={false} />
</QueryClientProvider>
```

### Logging

Enable logging in development:

```tsx
const queryClient = new QueryClient({
  logger: {
    log: console.log,
    warn: console.warn,
    error: console.error,
  },
});
```

## Requirements Validation

This implementation satisfies the following requirements:

- **16.1**: Uses React Query for server state management
- **16.2**: Maintains authentication state globally (via AuthContext)
- **16.3**: Uses React Query for server data caching
- **16.4**: Implements cache invalidation on data modifications
- **16.5**: Persists authentication in localStorage (via AuthContext)
- **16.6**: Clears state on logout (both React Query cache and localStorage)
- **16.7**: Uses local state for UI state (modals, drawers) - to be implemented in components
