import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { PageTransition } from './PageTransition';

describe('PageTransition', () => {
  it('should render children', () => {
    render(
      <PageTransition>
        <div>Test Content</div>
      </PageTransition>
    );
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should render with fade transition by default', () => {
    render(
      <PageTransition>
        <div>Test Content</div>
      </PageTransition>
    );
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should render with slide transition when specified', () => {
    render(
      <PageTransition variant="slide">
        <div>Test Content</div>
      </PageTransition>
    );
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should render with grow transition when specified', () => {
    render(
      <PageTransition variant="grow">
        <div>Test Content</div>
      </PageTransition>
    );
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });

  it('should render with zoom transition when specified', () => {
    render(
      <PageTransition variant="zoom">
        <div>Test Content</div>
      </PageTransition>
    );
    expect(screen.getByText('Test Content')).toBeInTheDocument();
  });
});
