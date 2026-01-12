import { describe, test, expect } from 'vitest';
import * as fc from 'fast-check';

/**
 * Example tests demonstrating the testing setup
 * These tests verify that the testing framework is configured correctly
 */

describe('Testing Framework Configuration', () => {
  test('vitest is working', () => {
    expect(true).toBe(true);
  });

  test('jest-dom matchers are available', () => {
    const div = document.createElement('div');
    div.textContent = 'Hello World';
    document.body.appendChild(div);
    
    expect(div).toBeInTheDocument();
    expect(div).toHaveTextContent('Hello World');
    
    document.body.removeChild(div);
  });

  test('fast-check is working', () => {
    fc.assert(
      fc.property(fc.integer(), (n) => {
        expect(n + 0).toBe(n);
      }),
      { numRuns: 100 }
    );
  });

  test('async tests work', async () => {
    const promise = Promise.resolve('success');
    await expect(promise).resolves.toBe('success');
  });
});

describe('Property-Based Testing Examples', () => {
  // Feature: frontend-unificado, Property Example: Addition is commutative
  test('addition is commutative', () => {
    fc.assert(
      fc.property(fc.integer(), fc.integer(), (a, b) => {
        expect(a + b).toBe(b + a);
      }),
      { numRuns: 100 }
    );
  });

  // Feature: frontend-unificado, Property Example: String concatenation length
  test('concatenated string length equals sum of lengths', () => {
    fc.assert(
      fc.property(fc.string(), fc.string(), (s1, s2) => {
        const concatenated = s1 + s2;
        expect(concatenated.length).toBe(s1.length + s2.length);
      }),
      { numRuns: 100 }
    );
  });

  // Feature: frontend-unificado, Property Example: Array reverse is involutive
  test('reversing array twice returns original', () => {
    fc.assert(
      fc.property(fc.array(fc.integer()), (arr) => {
        const reversed = [...arr].reverse();
        const doubleReversed = [...reversed].reverse();
        expect(doubleReversed).toEqual(arr);
      }),
      { numRuns: 100 }
    );
  });
});
