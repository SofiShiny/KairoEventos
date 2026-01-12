import { Skeleton, Box, Card, CardContent, Stack } from '@mui/material';

interface SkeletonLoaderProps {
  variant?: 'list' | 'card' | 'table' | 'detail';
  count?: number;
}

/**
 * SkeletonLoader - Displays skeleton loading placeholders
 * Provides different variants for common UI patterns
 */
export function SkeletonLoader({
  variant = 'list',
  count = 3,
}: SkeletonLoaderProps) {
  if (variant === 'card') {
    return (
      <Box sx={{ display: 'grid', gap: 2, gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))' }}>
        {Array.from({ length: count }).map((_, index) => (
          <Card key={index}>
            <Skeleton variant="rectangular" height={200} />
            <CardContent>
              <Skeleton variant="text" width="80%" height={32} />
              <Skeleton variant="text" width="60%" />
              <Skeleton variant="text" width="40%" />
            </CardContent>
          </Card>
        ))}
      </Box>
    );
  }

  if (variant === 'table') {
    return (
      <Box>
        {Array.from({ length: count }).map((_, index) => (
          <Box
            key={index}
            sx={{
              display: 'flex',
              gap: 2,
              p: 2,
              borderBottom: '1px solid',
              borderColor: 'divider',
            }}
          >
            <Skeleton variant="circular" width={40} height={40} />
            <Box sx={{ flex: 1 }}>
              <Skeleton variant="text" width="60%" />
              <Skeleton variant="text" width="40%" />
            </Box>
            <Skeleton variant="rectangular" width={80} height={32} />
          </Box>
        ))}
      </Box>
    );
  }

  if (variant === 'detail') {
    return (
      <Box>
        <Skeleton variant="rectangular" height={300} sx={{ mb: 2 }} />
        <Skeleton variant="text" width="80%" height={40} sx={{ mb: 1 }} />
        <Skeleton variant="text" width="60%" sx={{ mb: 2 }} />
        <Stack spacing={1}>
          <Skeleton variant="text" width="100%" />
          <Skeleton variant="text" width="100%" />
          <Skeleton variant="text" width="90%" />
          <Skeleton variant="text" width="95%" />
        </Stack>
      </Box>
    );
  }

  // Default: list variant
  return (
    <Stack spacing={2}>
      {Array.from({ length: count }).map((_, index) => (
        <Box
          key={index}
          sx={{
            display: 'flex',
            gap: 2,
            alignItems: 'center',
          }}
        >
          <Skeleton variant="rectangular" width={80} height={80} />
          <Box sx={{ flex: 1 }}>
            <Skeleton variant="text" width="70%" height={24} />
            <Skeleton variant="text" width="50%" />
            <Skeleton variant="text" width="40%" />
          </Box>
        </Box>
      ))}
    </Stack>
  );
}
