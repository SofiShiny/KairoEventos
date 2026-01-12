import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { ImagePlaceholder } from './ImagePlaceholder';

describe('ImagePlaceholder', () => {
  it('should render placeholder icon when no src provided', () => {
    const { container } = render(<ImagePlaceholder alt="Test image" />);
    expect(container.querySelector('.MuiSvgIcon-root')).toBeInTheDocument();
  });

  it('should render image when src is provided', () => {
    render(<ImagePlaceholder src="https://example.com/image.jpg" alt="Test image" />);
    const img = screen.getByAltText('Test image');
    expect(img).toBeInTheDocument();
    expect(img).toHaveAttribute('src', 'https://example.com/image.jpg');
  });

  it('should have lazy loading attribute', () => {
    render(<ImagePlaceholder src="https://example.com/image.jpg" alt="Test image" />);
    const img = screen.getByAltText('Test image');
    expect(img).toHaveAttribute('loading', 'lazy');
  });

  it('should show skeleton while loading', () => {
    const { container } = render(
      <ImagePlaceholder src="https://example.com/image.jpg" alt="Test image" />
    );
    expect(container.querySelector('.MuiSkeleton-root')).toBeInTheDocument();
  });

  it('should apply custom dimensions', () => {
    const { container } = render(
      <ImagePlaceholder
        src="https://example.com/image.jpg"
        alt="Test image"
        width={300}
        height={200}
      />
    );
    const wrapper = container.firstChild as HTMLElement;
    expect(wrapper).toHaveStyle({ width: '300px', height: '200px' });
  });

  it('should show placeholder icon on error', async () => {
    const { container } = render(
      <ImagePlaceholder src="https://invalid-url.com/image.jpg" alt="Test image" />
    );
    
    const img = screen.getByAltText('Test image');
    
    // Simulate image load error
    img.dispatchEvent(new Event('error'));
    
    await waitFor(() => {
      expect(container.querySelector('.MuiSvgIcon-root')).toBeInTheDocument();
    });
  });
});
