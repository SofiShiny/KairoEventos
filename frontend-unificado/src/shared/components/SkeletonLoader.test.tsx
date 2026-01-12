import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { SkeletonLoader } from './SkeletonLoader';

describe('SkeletonLoader', () => {
  it('should render list variant by default', () => {
    const { container } = render(<SkeletonLoader />);
    expect(container.querySelector('.MuiSkeleton-root')).toBeInTheDocument();
  });

  it('should render specified number of items', () => {
    const { container } = render(<SkeletonLoader count={5} />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-rectangular');
    expect(skeletons.length).toBe(5);
  });

  it('should render card variant', () => {
    const { container } = render(<SkeletonLoader variant="card" count={2} />);
    const cards = container.querySelectorAll('.MuiCard-root');
    expect(cards.length).toBe(2);
  });

  it('should render table variant', () => {
    const { container } = render(<SkeletonLoader variant="table" count={3} />);
    const rows = container.querySelectorAll('.MuiBox-root > .MuiBox-root');
    expect(rows.length).toBeGreaterThanOrEqual(3);
  });

  it('should render detail variant', () => {
    const { container } = render(<SkeletonLoader variant="detail" />);
    const skeletons = container.querySelectorAll('.MuiSkeleton-root');
    expect(skeletons.length).toBeGreaterThan(0);
  });
});
