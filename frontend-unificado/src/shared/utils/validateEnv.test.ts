import { describe, it, expect, vi } from 'vitest';
import * as fc from 'fast-check';
import { validateEnv } from './validateEnv';

// Feature: frontend-unificado, Property 16: Variables de Entorno Requeridas
// Validates: Requirements 18.7

describe('validateEnv - Property-Based Tests', () => {
  const requiredVars = [
    'VITE_GATEWAY_URL',
    'VITE_KEYCLOAK_URL',
    'VITE_KEYCLOAK_REALM',
    'VITE_KEYCLOAK_CLIENT_ID',
  ];

  it('Property 16: For any subset of missing required variables, validateEnv should throw an error listing all missing variables', () => {
    fc.assert(
      fc.property(
        // Generate a random subset of required variables to be missing
        fc.subarray(requiredVars, { minLength: 1, maxLength: requiredVars.length }),
        (missingVars) => {
          // Setup: Create env with some variables present and some missing
          const mockEnv: Record<string, string> = {};

          // Add present variables
          requiredVars.forEach((varName) => {
            if (!missingVars.includes(varName)) {
              mockEnv[varName] = `test-value-${varName}`;
            }
          });

          // Execute and verify
          try {
            validateEnv(mockEnv);
            // If no error thrown, all variables must be present
            expect(missingVars.length).toBe(0);
          } catch (error) {
            // Error should be thrown if any variables are missing
            expect(error).toBeInstanceOf(Error);
            const errorMessage = (error as Error).message;

            // Verify all missing variables are mentioned in error message
            missingVars.forEach((varName) => {
              expect(errorMessage).toContain(varName);
            });

            // Verify error message contains expected text
            expect(errorMessage).toContain('Missing required environment variables');
          }
        }
      ),
      { numRuns: 100 }
    );
  });

  it('Property 16: For any complete set of required variables with valid values, validateEnv should not throw', () => {
    fc.assert(
      fc.property(
        // Generate random valid values for each required variable
        fc.record({
          VITE_GATEWAY_URL: fc.webUrl(),
          VITE_KEYCLOAK_URL: fc.webUrl(),
          VITE_KEYCLOAK_REALM: fc.string({ minLength: 1, maxLength: 50 }),
          VITE_KEYCLOAK_CLIENT_ID: fc.string({ minLength: 1, maxLength: 50 }),
        }),
        (envVars) => {
          // Mock console.log to avoid noise in test output
          const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {});

          // Should not throw when all variables are present
          expect(() => validateEnv(envVars)).not.toThrow();

          consoleSpy.mockRestore();
        }
      ),
      { numRuns: 100 }
    );
  });

  it('Property 16: For any empty environment, validateEnv should throw listing all required variables', () => {
    const mockEnv = {};

    expect(() => validateEnv(mockEnv)).toThrow();

    try {
      validateEnv(mockEnv);
    } catch (error) {
      const errorMessage = (error as Error).message;

      // All required variables should be listed
      requiredVars.forEach((varName) => {
        expect(errorMessage).toContain(varName);
      });
    }
  });
});
