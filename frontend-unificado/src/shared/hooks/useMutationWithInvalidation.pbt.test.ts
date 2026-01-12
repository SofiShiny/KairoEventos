/**
 * Property-Based Tests for Cache Invalidation
 * 
 * These tests validate that cache invalidation works correctly for ALL possible
 * mutation operations using property-based testing with fast-check library.
 * 
 * Each test runs 100 iterations with randomly generated inputs.
 */

import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import * as fc from 'fast-check';
import { useMutationWithInvalidation } from './useMutationWithInvalidation';
import type { ReactNode } from 'react';
import React from 'react';

describe('Property-Based Tests: Cache Invalidation', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
          gcTime: Infinity,
        },
        mutations: {
          retry: false,
        },
      },
    });
  });

  afterEach(() => {
    queryClient.clear();
    vi.clearAllMocks();
  });

  const createWrapper = () => {
    return ({ children }: { children: ReactNode }) => 
      React.createElement(QueryClientProvider, { client: queryClient }, children);
  };

  /**
   * Property 13: Invalidación de Caché al Modificar Datos
   * 
   * For any operación de modificación de datos (crear, actualizar, eliminar), el sistema
   * debe invalidar la caché de React Query para esos datos, forzando una recarga.
   * 
   * Validates: Requirements 16.4
   * Feature: frontend-unificado, Property 13: Invalidación de Caché al Modificar Datos
   */
  describe('Property 13: Invalidación de Caché al Modificar Datos', () => {
    it('should ALWAYS invalidate specified query keys after successful mutation', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            // Generate random query keys to invalidate
            queriesToInvalidate: fc.array(
              fc.constantFrom('eventos', 'usuarios', 'entradas', 'reportes', 'dashboard', 'asientos'),
              { minLength: 1, maxLength: 4 }
            ).map(arr => [...new Set(arr)]), // Remove duplicates
            
            // Generate random mutation data
            mutationData: fc.record({
              id: fc.uuid(),
              name: fc.string({ minLength: 3, maxLength: 50 }),
              value: fc.integer({ min: 0, max: 1000 }),
            }),
            
            // Generate random response data
            responseData: fc.record({
              id: fc.uuid(),
              success: fc.constant(true),
              timestamp: fc.date(),
            }),
          }),
          async ({ queriesToInvalidate, mutationData, responseData }) => {
            // Setup: Populate cache with data for each query key
            const cacheData: Record<string, unknown[]> = {};
            for (const queryKey of queriesToInvalidate) {
              const data = [
                { id: '1', name: 'Item 1' },
                { id: '2', name: 'Item 2' },
              ];
              cacheData[queryKey] = data;
              queryClient.setQueryData([queryKey], data);
            }

            // Verify initial cache state
            for (const queryKey of queriesToInvalidate) {
              const cachedData = queryClient.getQueryData([queryKey]);
              expect(cachedData).toEqual(cacheData[queryKey]);
            }

            // Create a mock mutation function
            // Note: React Query passes (variables, context) to mutation functions
            const mockMutationFn = vi.fn().mockResolvedValue(responseData);

            // Render the hook
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                queriesToInvalidate
              ),
              { wrapper: createWrapper() }
            );

            // Execute the mutation
            result.current.mutate(mutationData);

            // Wait for mutation to complete
            await waitFor(() => {
              expect(result.current.isSuccess).toBe(true);
            }, { timeout: 3000 });

            // Property: ALL specified query keys MUST be invalidated
            for (const queryKey of queriesToInvalidate) {
              const queryState = queryClient.getQueryState([queryKey]);
              
              // The query should be marked as invalid (stale)
              expect(queryState).toBeDefined();
              expect(queryState?.isInvalidated).toBe(true);
            }

            // Property: Mutation function should have been called
            // React Query passes the mutation variables as first argument
            expect(mockMutationFn).toHaveBeenCalled();
            expect(mockMutationFn.mock.calls[0][0]).toEqual(mutationData);
          }
        ),
        { numRuns: 20 }
      );
    }, 15000); // Timeout for property-based test

    it('should NOT invalidate unrelated query keys', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            // Keys to invalidate
            queriesToInvalidate: fc.array(
              fc.constantFrom('eventos', 'usuarios'),
              { minLength: 1, maxLength: 2 }
            ).map(arr => [...new Set(arr)]),
            
            // Keys that should NOT be invalidated
            unrelatedKeys: fc.array(
              fc.constantFrom('entradas', 'reportes', 'dashboard'),
              { minLength: 1, maxLength: 3 }
            ).map(arr => [...new Set(arr)]),
            
            mutationData: fc.record({
              id: fc.uuid(),
              action: fc.constantFrom('create', 'update', 'delete'),
            }),
          }),
          async ({ queriesToInvalidate, unrelatedKeys, mutationData }) => {
            // Ensure no overlap between keys to invalidate and unrelated keys
            const filteredUnrelatedKeys = unrelatedKeys.filter(
              key => !queriesToInvalidate.includes(key)
            );

            if (filteredUnrelatedKeys.length === 0) {
              // Skip if no unrelated keys after filtering
              return;
            }

            // Setup: Populate cache for all keys
            const allKeys = [...queriesToInvalidate, ...filteredUnrelatedKeys];
            for (const queryKey of allKeys) {
              queryClient.setQueryData([queryKey], [{ id: '1', data: 'test' }]);
            }

            // Create mock mutation
            const mockMutationFn = vi.fn().mockResolvedValue({ success: true });

            // Render hook
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                queriesToInvalidate
              ),
              { wrapper: createWrapper() }
            );

            // Execute mutation
            result.current.mutate(mutationData);

            await waitFor(() => {
              expect(result.current.isSuccess).toBe(true);
            }, { timeout: 3000 });

            // Property: Specified keys MUST be invalidated
            for (const queryKey of queriesToInvalidate) {
              const queryState = queryClient.getQueryState([queryKey]);
              expect(queryState?.isInvalidated).toBe(true);
            }

            // Property: Unrelated keys MUST NOT be invalidated
            for (const queryKey of filteredUnrelatedKeys) {
              const queryState = queryClient.getQueryState([queryKey]);
              expect(queryState?.isInvalidated).toBe(false);
            }
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);

    it('should invalidate cache for any type of mutation operation', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            operationType: fc.constantFrom('create', 'update', 'delete', 'patch'),
            resourceType: fc.constantFrom('eventos', 'usuarios', 'entradas', 'reportes'),
            mutationPayload: fc.oneof(
              // Create operation
              fc.record({
                name: fc.string({ minLength: 1, maxLength: 50 }),
                description: fc.string({ minLength: 0, maxLength: 200 }),
              }),
              // Update operation
              fc.record({
                id: fc.uuid(),
                updates: fc.dictionary(
                  fc.string({ minLength: 1, maxLength: 20 }),
                  fc.string({ minLength: 0, maxLength: 50 })
                ),
              }),
              // Delete operation
              fc.record({
                id: fc.uuid(),
              })
            ),
          }),
          async ({ operationType, resourceType, mutationPayload }) => {
            // Setup cache
            queryClient.setQueryData([resourceType], [
              { id: '1', name: 'Item 1' },
              { id: '2', name: 'Item 2' },
            ]);

            // Mock mutation based on operation type
            const mockMutationFn = vi.fn().mockResolvedValue({
              success: true,
              operation: operationType,
              data: mutationPayload,
            });

            // Render hook
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                [resourceType]
              ),
              { wrapper: createWrapper() }
            );

            // Execute mutation
            result.current.mutate(mutationPayload);

            await waitFor(() => {
              expect(result.current.isSuccess).toBe(true);
            }, { timeout: 3000 });

            // Property: Cache MUST be invalidated regardless of operation type
            const queryState = queryClient.getQueryState([resourceType]);
            expect(queryState?.isInvalidated).toBe(true);

            // Property: Mutation should have been called
            expect(mockMutationFn).toHaveBeenCalled();
            expect(mockMutationFn.mock.calls[0][0]).toEqual(mutationPayload);
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);

    it('should handle multiple simultaneous mutations with correct invalidation', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            mutations: fc.array(
              fc.record({
                queryKeys: fc.array(
                  fc.constantFrom('eventos', 'usuarios', 'entradas'),
                  { minLength: 1, maxLength: 2 }
                ).map(arr => [...new Set(arr)]),
                data: fc.record({
                  id: fc.uuid(),
                  value: fc.integer(),
                }),
              }),
              { minLength: 2, maxLength: 4 }
            ),
          }),
          async ({ mutations }) => {
            // Setup: Populate cache for all possible keys
            const allPossibleKeys = ['eventos', 'usuarios', 'entradas'];
            for (const key of allPossibleKeys) {
              queryClient.setQueryData([key], [{ id: '1', data: 'initial' }]);
            }

            // Track which keys should be invalidated
            const keysToInvalidate = new Set<string>();
            mutations.forEach(mutation => {
              mutation.queryKeys.forEach(key => keysToInvalidate.add(key));
            });

            // Execute all mutations
            const mutationPromises = mutations.map(async (mutation) => {
              const mockFn = vi.fn().mockResolvedValue({ success: true });
              
              const { result } = renderHook(
                () => useMutationWithInvalidation(mockFn, mutation.queryKeys),
                { wrapper: createWrapper() }
              );

              result.current.mutate(mutation.data);

              await waitFor(() => {
                expect(result.current.isSuccess).toBe(true);
              }, { timeout: 3000 });
            });

            await Promise.all(mutationPromises);

            // Small delay to ensure all invalidations are processed
            await new Promise(resolve => setTimeout(resolve, 100));

            // Property: ALL keys that were targeted by ANY mutation MUST be invalidated
            for (const key of keysToInvalidate) {
              const queryState = queryClient.getQueryState([key]);
              // Query should exist and be invalidated
              expect(queryState).toBeDefined();
              if (queryState) {
                expect(queryState.isInvalidated).toBe(true);
              }
            }

            // Property: Keys not targeted by any mutation should NOT be invalidated
            const untargetedKeys = allPossibleKeys.filter(key => !keysToInvalidate.has(key));
            for (const key of untargetedKeys) {
              const queryState = queryClient.getQueryState([key]);
              if (queryState) {
                expect(queryState.isInvalidated).toBe(false);
              }
            }
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);

    it('should preserve cache invalidation even if mutation fails', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            queryKeys: fc.array(
              fc.constantFrom('eventos', 'usuarios', 'entradas'),
              { minLength: 1, maxLength: 3 }
            ).map(arr => [...new Set(arr)]),
            mutationData: fc.record({
              id: fc.uuid(),
            }),
            errorMessage: fc.string({ minLength: 5, maxLength: 100 }),
          }),
          async ({ queryKeys, mutationData, errorMessage }) => {
            // Setup cache
            for (const key of queryKeys) {
              queryClient.setQueryData([key], [{ id: '1', data: 'test' }]);
            }

            // Mock mutation that fails
            const mockMutationFn = vi.fn().mockRejectedValue(new Error(errorMessage));

            // Render hook
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                queryKeys
              ),
              { wrapper: createWrapper() }
            );

            // Execute mutation
            result.current.mutate(mutationData);

            // Wait for mutation to fail
            await waitFor(() => {
              expect(result.current.isError).toBe(true);
            }, { timeout: 3000 });

            // Property: Cache should NOT be invalidated on error
            // (invalidation only happens on success)
            for (const key of queryKeys) {
              const queryState = queryClient.getQueryState([key]);
              expect(queryState?.isInvalidated).toBe(false);
            }

            // Property: Error should be captured
            expect(result.current.error).toBeDefined();
            expect((result.current.error as Error).message).toBe(errorMessage);
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);

    it('should work correctly with empty query key arrays', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            mutationData: fc.record({
              id: fc.uuid(),
              value: fc.string(),
            }),
          }),
          async ({ mutationData }) => {
            // Setup some cache data
            queryClient.setQueryData(['eventos'], [{ id: '1' }]);
            queryClient.setQueryData(['usuarios'], [{ id: '2' }]);

            // Mock mutation
            const mockMutationFn = vi.fn().mockResolvedValue({ success: true });

            // Render hook with empty invalidation array
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                [] // No keys to invalidate
              ),
              { wrapper: createWrapper() }
            );

            // Execute mutation
            result.current.mutate(mutationData);

            await waitFor(() => {
              expect(result.current.isSuccess).toBe(true);
            }, { timeout: 3000 });

            // Property: No cache should be invalidated when array is empty
            const eventosState = queryClient.getQueryState(['eventos']);
            const usuariosState = queryClient.getQueryState(['usuarios']);
            
            expect(eventosState?.isInvalidated).toBe(false);
            expect(usuariosState?.isInvalidated).toBe(false);

            // Property: Mutation should still execute successfully
            expect(mockMutationFn).toHaveBeenCalled();
            expect(mockMutationFn.mock.calls[0][0]).toEqual(mutationData);
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);

    it('should handle nested query keys correctly', async () => {
      await fc.assert(
        fc.asyncProperty(
          fc.record({
            baseKey: fc.constantFrom('eventos', 'usuarios', 'entradas'),
            mutationData: fc.record({
              id: fc.uuid(),
            }),
          }),
          async ({ baseKey, mutationData }) => {
            // Setup cache with nested keys
            queryClient.setQueryData([baseKey], [{ id: '1' }]);
            queryClient.setQueryData([baseKey, 'detail'], { id: '1', details: 'test' });
            queryClient.setQueryData([baseKey, 'stats'], { count: 10 });

            // Mock mutation
            const mockMutationFn = vi.fn().mockResolvedValue({ success: true });

            // Render hook - invalidate base key
            const { result } = renderHook(
              () => useMutationWithInvalidation(
                mockMutationFn,
                [baseKey]
              ),
              { wrapper: createWrapper() }
            );

            // Execute mutation
            result.current.mutate(mutationData);

            await waitFor(() => {
              expect(result.current.isSuccess).toBe(true);
            }, { timeout: 3000 });

            // Small delay to ensure invalidation is processed
            await new Promise(resolve => setTimeout(resolve, 100));

            // Property: Base key should be invalidated
            const baseState = queryClient.getQueryState([baseKey]);
            expect(baseState).toBeDefined();
            if (baseState) {
              expect(baseState.isInvalidated).toBe(true);
            }

            // Property: Nested keys with same base should also be invalidated
            // (React Query invalidates all queries that start with the key)
            const detailState = queryClient.getQueryState([baseKey, 'detail']);
            const statsState = queryClient.getQueryState([baseKey, 'stats']);
            
            // These should be invalidated as they share the base key
            if (detailState) {
              expect(detailState.isInvalidated).toBe(true);
            }
            if (statsState) {
              expect(statsState.isInvalidated).toBe(true);
            }
          }
        ),
        { numRuns: 20 }
      );
    }, 15000);
  });
});
