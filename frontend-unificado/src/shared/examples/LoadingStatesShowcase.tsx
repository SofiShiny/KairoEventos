import {
  Box,
  Container,
  Typography,
  Paper,
  Stack,
  Divider,
} from '@mui/material';
import { useState } from 'react';
import {
  LoadingSpinner,
  Button,
  SkeletonLoader,
  ProgressIndicator,
  ImagePlaceholder,
  PageTransition,
  EmptyState,
  useToast,
} from '@shared/components';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';

/**
 * LoadingStatesShowcase - Demonstrates all loading states and feedback UX components
 * This is a reference implementation showing how to use each component
 */
export function LoadingStatesShowcase() {
  const [loading, setLoading] = useState(false);
  const [progress, setProgress] = useState(0);
  const toast = useToast();

  const handleButtonClick = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      toast.showSuccess('Operation completed successfully!');
    }, 2000);
  };

  const handleProgressDemo = () => {
    setProgress(0);
    const interval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 100) {
          clearInterval(interval);
          toast.showSuccess('Upload complete!');
          return 100;
        }
        return prev + 10;
      });
    }, 500);
  };

  return (
    <PageTransition variant="fade">
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Typography variant="h3" gutterBottom>
          Loading States & Feedback UX Showcase
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          Comprehensive examples of all loading states and user feedback components
        </Typography>

        <Stack spacing={4}>
          {/* Loading Spinners */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              1. Loading Spinners
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Use for indicating loading state in different contexts
            </Typography>
            <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
              <Box sx={{ flex: '1 1 300px', textAlign: 'center' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Small
                </Typography>
                <LoadingSpinner size="small" />
              </Box>
              <Box sx={{ flex: '1 1 300px', textAlign: 'center' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Medium (Default)
                </Typography>
                <LoadingSpinner size="medium" />
              </Box>
              <Box sx={{ flex: '1 1 300px', textAlign: 'center' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Large
                </Typography>
                <LoadingSpinner size="large" />
              </Box>
            </Box>
          </Paper>

          {/* Button Loading States */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              2. Button Loading States
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Buttons automatically disable and show spinner when loading
            </Typography>
            <Stack direction="row" spacing={2}>
              <Button
                variant="contained"
                loading={loading}
                onClick={handleButtonClick}
              >
                Click Me
              </Button>
              <Button variant="outlined" loading={loading}>
                Outlined Button
              </Button>
              <Button variant="text" loading={loading}>
                Text Button
              </Button>
            </Stack>
          </Paper>

          {/* Skeleton Loaders */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              3. Skeleton Loaders
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Show skeleton placeholders while content is loading
            </Typography>
            
            <Stack spacing={3}>
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  List Variant
                </Typography>
                <SkeletonLoader variant="list" count={3} />
              </Box>

              <Divider />

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Card Variant
                </Typography>
                <SkeletonLoader variant="card" count={3} />
              </Box>

              <Divider />

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Table Variant
                </Typography>
                <SkeletonLoader variant="table" count={4} />
              </Box>

              <Divider />

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Detail Variant
                </Typography>
                <SkeletonLoader variant="detail" count={1} />
              </Box>
            </Stack>
          </Paper>

          {/* Progress Indicators */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              4. Progress Indicators
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Show progress for long-running operations
            </Typography>
            
            <Stack spacing={3}>
              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Linear Progress (Indeterminate)
                </Typography>
                <ProgressIndicator
                  variant="linear"
                  label="Processing..."
                  showPercentage={false}
                />
              </Box>

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Linear Progress (Determinate)
                </Typography>
                <ProgressIndicator
                  variant="linear"
                  value={progress}
                  label="Uploading file..."
                  showPercentage
                />
                <Button
                  variant="outlined"
                  size="small"
                  onClick={handleProgressDemo}
                  sx={{ mt: 2 }}
                >
                  Start Upload Demo
                </Button>
              </Box>

              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Circular Progress
                </Typography>
                <ProgressIndicator
                  variant="circular"
                  value={progress}
                  label="Processing..."
                  showPercentage
                />
              </Box>
            </Stack>
          </Paper>

          {/* Image Placeholders */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              5. Image Placeholders
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Images with loading states and error fallbacks
            </Typography>
            
            <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
              <Box sx={{ flex: '1 1 300px' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Valid Image
                </Typography>
                <ImagePlaceholder
                  src="https://via.placeholder.com/300x200"
                  alt="Sample image"
                  height={200}
                />
              </Box>
              <Box sx={{ flex: '1 1 300px' }}>
                <Typography variant="subtitle2" gutterBottom>
                  Broken Image (Fallback)
                </Typography>
                <ImagePlaceholder
                  src="https://invalid-url.com/image.jpg"
                  alt="Broken image"
                  height={200}
                />
              </Box>
              <Box sx={{ flex: '1 1 300px' }}>
                <Typography variant="subtitle2" gutterBottom>
                  No Source (Placeholder)
                </Typography>
                <ImagePlaceholder
                  alt="No image"
                  height={200}
                />
              </Box>
            </Box>
          </Paper>

          {/* Toast Notifications */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              6. Toast Notifications
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Show feedback messages for user actions
            </Typography>
            
            <Stack direction="row" spacing={2} flexWrap="wrap">
              <Button
                variant="contained"
                color="success"
                onClick={() => toast.showSuccess('Operation successful!')}
              >
                Show Success
              </Button>
              <Button
                variant="contained"
                color="error"
                onClick={() => toast.showError('An error occurred!')}
              >
                Show Error
              </Button>
              <Button
                variant="contained"
                color="warning"
                onClick={() => toast.showWarning('Warning message!')}
              >
                Show Warning
              </Button>
              <Button
                variant="contained"
                color="info"
                onClick={() => toast.showInfo('Information message')}
              >
                Show Info
              </Button>
            </Stack>
          </Paper>

          {/* Empty States */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              7. Empty States
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              Show informative messages when lists are empty
            </Typography>
            
            <EmptyState
              icon={<CheckCircleIcon sx={{ fontSize: 64 }} />}
              title="No items found"
              description="There are no items to display at the moment. Try adding some items or adjusting your filters."
              action={
                <Button variant="contained">
                  Add New Item
                </Button>
              }
            />
          </Paper>

          {/* Page Transitions */}
          <Paper sx={{ p: 3 }}>
            <Typography variant="h5" gutterBottom>
              8. Page Transitions
            </Typography>
            <Typography variant="body2" color="text.secondary" paragraph>
              This entire page uses a fade transition. Wrap your page content with PageTransition component.
            </Typography>
            
            <Box
              sx={{
                p: 2,
                bgcolor: 'grey.100',
                borderRadius: 1,
                fontFamily: 'monospace',
              }}
            >
              <code>
                {`<PageTransition variant="fade">
  <YourPageContent />
</PageTransition>`}
              </code>
            </Box>
          </Paper>

          {/* Best Practices */}
          <Paper sx={{ p: 3, bgcolor: 'info.light' }}>
            <Typography variant="h5" gutterBottom>
              Best Practices
            </Typography>
            <Stack spacing={1} component="ul" sx={{ pl: 2 }}>
              <Typography component="li">
                <strong>Loading Spinners:</strong> Use for short operations ({"<"}3 seconds)
              </Typography>
              <Typography component="li">
                <strong>Skeleton Loaders:</strong> Use for initial page loads to show content structure
              </Typography>
              <Typography component="li">
                <strong>Progress Indicators:</strong> Use for long operations ({">"} 3 seconds) where progress can be tracked
              </Typography>
              <Typography component="li">
                <strong>Button Loading:</strong> Always disable buttons during async operations
              </Typography>
              <Typography component="li">
                <strong>Toast Notifications:</strong> Use for feedback on user actions (success, error, warning)
              </Typography>
              <Typography component="li">
                <strong>Empty States:</strong> Provide helpful guidance when lists are empty
              </Typography>
              <Typography component="li">
                <strong>Image Placeholders:</strong> Always use lazy loading for images
              </Typography>
            </Stack>
          </Paper>
        </Stack>
      </Container>
    </PageTransition>
  );
}
