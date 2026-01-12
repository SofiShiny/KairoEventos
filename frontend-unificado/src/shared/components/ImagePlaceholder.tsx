import { Box, Skeleton } from '@mui/material';
import { useState } from 'react';
import ImageIcon from '@mui/icons-material/Image';

interface ImagePlaceholderProps {
  src?: string;
  alt: string;
  width?: string | number;
  height?: string | number;
  borderRadius?: string | number;
  objectFit?: 'cover' | 'contain' | 'fill' | 'none' | 'scale-down';
  loading?: 'lazy' | 'eager';
}

/**
 * ImagePlaceholder - Image component with loading placeholder and error fallback
 * Shows skeleton while loading and a placeholder icon if image fails to load
 * 
 * Performance optimizations:
 * - Native lazy loading support (loading="lazy")
 * - Skeleton placeholder during load
 */
export function ImagePlaceholder({
  src,
  alt,
  width = '100%',
  height = 200,
  borderRadius = 1,
  objectFit = 'cover',
  loading: loadingStrategy = 'lazy',
}: ImagePlaceholderProps) {
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(false);

  const handleLoad = () => {
    setIsLoading(false);
  };

  const handleError = () => {
    setIsLoading(false);
    setError(true);
  };

  if (!src || error) {
    return (
      <Box
        sx={{
          width,
          height,
          borderRadius,
          backgroundColor: 'grey.200',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <ImageIcon sx={{ fontSize: 48, color: 'grey.400' }} />
      </Box>
    );
  }

  return (
    <Box
      sx={{
        position: 'relative',
        width,
        height,
        borderRadius,
        overflow: 'hidden',
      }}
    >
      {isLoading && (
        <Skeleton
          variant="rectangular"
          width="100%"
          height="100%"
          sx={{
            position: 'absolute',
            top: 0,
            left: 0,
          }}
        />
      )}
      <img
        src={src}
        alt={alt}
        loading={loadingStrategy}
        onLoad={handleLoad}
        onError={handleError}
        style={{
          width: '100%',
          height: '100%',
          objectFit,
          display: isLoading ? 'none' : 'block',
        }}
      />
    </Box>
  );
}
