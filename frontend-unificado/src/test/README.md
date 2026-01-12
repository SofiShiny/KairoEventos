# Testing Guide - Frontend Unificado

This guide covers the testing strategy and practices for the Frontend Unificado project.

## Testing Stack

- **Vitest**: Fast test runner with native ESM support
- **React Testing Library**: Component testing focused on user behavior
- **MSW (Mock Service Worker)**: API mocking at the network level
- **fast-check**: Property-based testing for universal properties
- **@testing-library/jest-dom**: Custom matchers for DOM assertions

## Test Types

### 1. Unit Tests

Test individual components, hooks, and utilities in isolation.

**Location**: Co-located with source files (e.g., `Component.test.tsx`)

**Example**:
```typescript
import { render, screen } from '@testing-library/react';
import { Button } from './Button';

test('renders button with text', () => {
  render(<Button>Click me</Button>);
  expect(screen.getByText('Click me')).toBeInTheDocument();
});
```

### 2. Property-Based Tests (PBT)

Test universal properties that should hold for all inputs.

**Location**: Files ending with `.pbt.test.ts` or `.pbt.test.tsx`

**Example**:
```typescript
import * as fc from 'fast-check';

// Feature: frontend-unificado, Property 8: Validación de Campos Requeridos
test('form should be invalid when required fields are empty', () => {
  fc.assert(
    fc.property(
      fc.record({
        nombre: fc.string(),
        correo: fc.string(),
      }),
      (formData) => {
        // Test that empty required fields make form invalid
        const isValid = validateForm(formData);
        if (!formData.nombre || !formData.correo) {
          expect(isValid).toBe(false);
        }
      }
    ),
    { numRuns: 100 }
  );
});
```

### 3. Integration Tests

Test multiple components working together or complete user flows.

**Example**:
```typescript
test('complete purchase flow', async () => {
  render(<App />);
  
  // Login
  fireEvent.click(screen.getByText('Iniciar Sesión'));
  
  // Navigate to eventos
  await waitFor(() => {
    expect(screen.getByText('Eventos')).toBeInTheDocument();
  });
  
  // Select evento and purchase
  // ... more steps
});
```

## Running Tests

```bash
# Run all tests once
npm test

# Run tests in watch mode
npm run test:watch

# Run tests with coverage
npm run test:coverage

# Run only unit tests (exclude PBT)
npm run test:unit

# Run only property-based tests
npm run test:pbt

# Run tests with UI
npm run test:ui
```

## Coverage Requirements

The project enforces minimum coverage thresholds:

- **Lines**: 70%
- **Functions**: 70%
- **Branches**: 70%
- **Statements**: 70%

Coverage reports are generated in `coverage/` directory.

## Writing Tests

### Component Tests

```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { EventosList } from './EventosList';

// Create a test query client
const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

test('renders list of eventos', async () => {
  const queryClient = createTestQueryClient();
  
  render(
    <QueryClientProvider client={queryClient}>
      <EventosList />
    </QueryClientProvider>
  );
  
  // MSW will return mock data
  await waitFor(() => {
    expect(screen.getByText('Evento Test 1')).toBeInTheDocument();
  });
});
```

### Hook Tests

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useEventos } from './useEventos';

test('fetches eventos', async () => {
  const queryClient = new QueryClient();
  const wrapper = ({ children }) => (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
  
  const { result } = renderHook(() => useEventos(), { wrapper });
  
  await waitFor(() => {
    expect(result.current.isLoading).toBe(false);
  });
  
  expect(result.current.eventos).toHaveLength(2);
});
```

### Testing with Authentication

```typescript
import { AuthProvider } from '@/context/AuthContext';

const renderWithAuth = (component, { isAuthenticated = true, roles = [] } = {}) => {
  const mockAuth = {
    isAuthenticated,
    user: isAuthenticated ? { id: '1', username: 'test', roles } : null,
    // ... other auth properties
  };
  
  return render(
    <AuthProvider value={mockAuth}>
      {component}
    </AuthProvider>
  );
};

test('shows admin menu for admin users', () => {
  renderWithAuth(<Navbar />, { roles: ['Admin'] });
  expect(screen.getByText('Usuarios')).toBeInTheDocument();
});
```

### Testing Error Scenarios

```typescript
import { server } from '@/test/mocks/server';
import { serverError500Handler } from '@/test/mocks/errorHandlers';

test('handles server error', async () => {
  server.use(serverError500Handler);
  
  render(<EventosList />);
  
  await waitFor(() => {
    expect(screen.getByText(/error del servidor/i)).toBeInTheDocument();
  });
});
```

### Testing Forms

```typescript
import userEvent from '@testing-library/user-event';

test('validates email format', async () => {
  const user = userEvent.setup();
  render(<UsuarioForm />);
  
  const emailInput = screen.getByLabelText(/correo/i);
  await user.type(emailInput, 'invalid-email');
  
  const submitButton = screen.getByRole('button', { name: /guardar/i });
  await user.click(submitButton);
  
  expect(screen.getByText(/correo inválido/i)).toBeInTheDocument();
});
```

## Best Practices

### 1. Test User Behavior, Not Implementation

❌ Bad:
```typescript
expect(component.state.isLoading).toBe(true);
```

✅ Good:
```typescript
expect(screen.getByRole('progressbar')).toBeInTheDocument();
```

### 2. Use Semantic Queries

Prefer queries in this order:
1. `getByRole`
2. `getByLabelText`
3. `getByPlaceholderText`
4. `getByText`
5. `getByTestId` (last resort)

### 3. Wait for Async Updates

```typescript
// Use waitFor for async operations
await waitFor(() => {
  expect(screen.getByText('Loaded')).toBeInTheDocument();
});

// Or findBy queries (combines getBy + waitFor)
expect(await screen.findByText('Loaded')).toBeInTheDocument();
```

### 4. Clean Up After Tests

Tests are automatically cleaned up by `@testing-library/react` and MSW handlers are reset after each test.

### 5. Avoid Testing Implementation Details

❌ Bad:
```typescript
expect(component.find('.button-class')).toHaveLength(1);
```

✅ Good:
```typescript
expect(screen.getByRole('button', { name: /submit/i })).toBeInTheDocument();
```

### 6. Test Accessibility

```typescript
test('form is accessible', () => {
  render(<UsuarioForm />);
  
  // All inputs should have labels
  expect(screen.getByLabelText(/nombre/i)).toBeInTheDocument();
  expect(screen.getByLabelText(/correo/i)).toBeInTheDocument();
  
  // Buttons should have accessible names
  expect(screen.getByRole('button', { name: /guardar/i })).toBeInTheDocument();
});
```

## Property-Based Testing Guidelines

### When to Use PBT

Use property-based tests for:
- Validation logic
- Data transformations
- Universal properties (e.g., "for all valid inputs, X should be true")
- Round-trip properties (serialize → deserialize)
- Invariants that must hold

### PBT Best Practices

1. **Run at least 100 iterations**:
```typescript
fc.assert(fc.property(...), { numRuns: 100 });
```

2. **Tag with property reference**:
```typescript
// Feature: frontend-unificado, Property 8: Validación de Campos Requeridos
```

3. **Use appropriate generators**:
```typescript
fc.emailAddress()  // For emails
fc.string()        // For strings
fc.integer()       // For numbers
fc.record({...})   // For objects
```

4. **Test edge cases**:
```typescript
fc.oneof(
  fc.constant(''),           // Empty string
  fc.string({ minLength: 1 }) // Non-empty
)
```

## Debugging Tests

### 1. Use screen.debug()

```typescript
test('debug test', () => {
  render(<Component />);
  screen.debug(); // Prints DOM to console
});
```

### 2. Use Vitest UI

```bash
npm run test:ui
```

Opens a browser UI for interactive test debugging.

### 3. Use --reporter=verbose

```bash
npx vitest --reporter=verbose
```

Shows detailed test output.

### 4. Isolate Failing Tests

```typescript
test.only('this test only', () => {
  // Only this test will run
});
```

## Common Issues

### Issue: "Unable to find element"

**Solution**: Use `waitFor` or `findBy` queries for async content.

### Issue: "Not wrapped in act(...)"

**Solution**: Use `waitFor` or ensure all state updates are awaited.

### Issue: "MSW not intercepting requests"

**Solution**: Check that `server.listen()` is called in `beforeAll` and URL matches exactly.

### Issue: "Tests timeout"

**Solution**: Increase timeout or check for infinite loops:
```typescript
test('my test', async () => {
  // ...
}, { timeout: 10000 }); // 10 seconds
```

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [MSW Documentation](https://mswjs.io/)
- [fast-check Documentation](https://fast-check.dev/)
- [Testing Library Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)
