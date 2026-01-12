import { useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Stack,
  Divider,
} from '@mui/material';
import {
  LoadingSpinner,
  ErrorMessage,
  EmptyState,
  Button,
  TextField,
  useToast,
} from '../components';
import EventIcon from '@mui/icons-material/Event';
import AddIcon from '@mui/icons-material/Add';

/**
 * ComponentsShowcase - Demonstrates all shared components
 * This is for development and documentation purposes
 */
export function ComponentsShowcase() {
  const toast = useToast();
  const [loading, setLoading] = useState(false);
  const [textValue, setTextValue] = useState('');

  const handleLoadingDemo = () => {
    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      toast.showSuccess('Operation completed successfully!');
    }, 2000);
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h3" gutterBottom>
        Shared Components Showcase
      </Typography>
      <Typography variant="body1" color="text.secondary" paragraph>
        This page demonstrates all the shared components available in the
        application.
      </Typography>

      <Stack spacing={4}>
        {/* LoadingSpinner */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            LoadingSpinner
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Displays loading indicators in different sizes
          </Typography>
          <Stack direction="row" spacing={4} alignItems="center">
            <Box>
              <Typography variant="caption" display="block" gutterBottom>
                Small
              </Typography>
              <LoadingSpinner size="small" />
            </Box>
            <Box>
              <Typography variant="caption" display="block" gutterBottom>
                Medium
              </Typography>
              <LoadingSpinner size="medium" />
            </Box>
            <Box>
              <Typography variant="caption" display="block" gutterBottom>
                Large
              </Typography>
              <LoadingSpinner size="large" />
            </Box>
          </Stack>
        </Paper>

        {/* ErrorMessage */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            ErrorMessage
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Displays error messages with optional retry button
          </Typography>
          <Stack spacing={2}>
            <ErrorMessage
              error="This is an inline error message"
              variant="inline"
            />
            <ErrorMessage
              error={new Error('Network connection failed')}
              variant="inline"
              onRetry={() => toast.showInfo('Retrying...')}
            />
            <Box sx={{ border: 1, borderColor: 'divider', p: 2 }}>
              <ErrorMessage
                error="This is a centered error message"
                variant="centered"
                onRetry={() => toast.showInfo('Retrying...')}
              />
            </Box>
          </Stack>
        </Paper>

        {/* EmptyState */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            EmptyState
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Displays informative empty states for lists
          </Typography>
          <Box sx={{ border: 1, borderColor: 'divider' }}>
            <EmptyState
              icon={<EventIcon />}
              title="No events available"
              description="There are currently no events published. Check back later or create a new event."
              action={
                <Button variant="contained" startIcon={<AddIcon />}>
                  Create Event
                </Button>
              }
            />
          </Box>
        </Paper>

        {/* Button */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            Button
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Enhanced button with loading state
          </Typography>
          <Stack direction="row" spacing={2} flexWrap="wrap">
            <Button variant="contained">Contained</Button>
            <Button variant="outlined">Outlined</Button>
            <Button variant="text">Text</Button>
            <Button variant="contained" loading={loading}>
              {loading ? 'Loading...' : 'Click to Load'}
            </Button>
            <Button
              variant="contained"
              color="secondary"
              onClick={handleLoadingDemo}
            >
              Demo Loading
            </Button>
          </Stack>
        </Paper>

        {/* TextField */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            TextField
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Standardized text input component
          </Typography>
          <Stack spacing={2}>
            <TextField
              label="Standard TextField"
              value={textValue}
              onChange={(e) => setTextValue(e.target.value)}
              helperText="This is a helper text"
            />
            <TextField
              label="Required Field"
              required
              error={textValue === ''}
              helperText={textValue === '' ? 'This field is required' : ''}
            />
            <TextField
              label="Email"
              type="email"
              placeholder="user@example.com"
            />
            <TextField
              label="Multiline"
              multiline
              rows={4}
              placeholder="Enter multiple lines..."
            />
          </Stack>
        </Paper>

        {/* Toast Notifications */}
        <Paper sx={{ p: 3 }}>
          <Typography variant="h5" gutterBottom>
            Toast Notifications
          </Typography>
          <Typography variant="body2" color="text.secondary" paragraph>
            Show toast notifications with different severity levels
          </Typography>
          <Stack direction="row" spacing={2} flexWrap="wrap">
            <Button
              variant="outlined"
              color="success"
              onClick={() => toast.showSuccess('Success message!')}
            >
              Success Toast
            </Button>
            <Button
              variant="outlined"
              color="error"
              onClick={() => toast.showError('Error message!')}
            >
              Error Toast
            </Button>
            <Button
              variant="outlined"
              color="warning"
              onClick={() => toast.showWarning('Warning message!')}
            >
              Warning Toast
            </Button>
            <Button
              variant="outlined"
              color="info"
              onClick={() => toast.showInfo('Info message!')}
            >
              Info Toast
            </Button>
          </Stack>
        </Paper>

        <Divider />

        <Typography variant="body2" color="text.secondary" align="center">
          All components are accessible, responsive, and follow Material Design
          guidelines
        </Typography>
      </Stack>
    </Container>
  );
}
