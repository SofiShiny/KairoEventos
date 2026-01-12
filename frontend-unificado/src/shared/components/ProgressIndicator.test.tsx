import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ProgressIndicator } from './ProgressIndicator';

describe('ProgressIndicator', () => {
  it('should render linear progress by default', () => {
    const { container } = render(<ProgressIndicator />);
    expect(container.querySelector('.MuiLinearProgress-root')).toBeInTheDocument();
  });

  it('should render circular progress when specified', () => {
    const { container } = render(<ProgressIndicator variant="circular" />);
    expect(container.querySelector('.MuiCircularProgress-root')).toBeInTheDocument();
  });

  it('should show label when provided', () => {
    render(<ProgressIndicator label="Loading..." />);
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('should show percentage for determinate progress', () => {
    render(<ProgressIndicator value={75} label="Loading" showPercentage />);
    expect(screen.getByText('75%')).toBeInTheDocument();
  });

  it('should not show percentage when showPercentage is false', () => {
    render(<ProgressIndicator value={75} showPercentage={false} />);
    expect(screen.queryByText('75%')).not.toBeInTheDocument();
  });

  it('should render indeterminate progress when value is undefined', () => {
    const { container } = render(<ProgressIndicator />);
    const progress = container.querySelector('.MuiLinearProgress-root');
    expect(progress).toHaveClass('MuiLinearProgress-indeterminate');
  });

  it('should render determinate progress when value is provided', () => {
    const { container } = render(<ProgressIndicator value={50} />);
    const progress = container.querySelector('.MuiLinearProgress-root');
    expect(progress).toHaveClass('MuiLinearProgress-determinate');
  });
});
