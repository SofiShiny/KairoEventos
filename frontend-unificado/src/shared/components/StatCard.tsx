/**
 * StatCard Component
 * Displays a statistic card with icon, value, and label
 */

import { Paper, Typography, Skeleton } from '@mui/material';
import type { SvgIconComponent } from '@mui/icons-material';

interface StatCardProps {
  icon: SvgIconComponent;
  value: number | string;
  label: string;
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
  isLoading?: boolean;
}

export function StatCard({ icon: Icon, value, label, color = 'primary', isLoading = false }: StatCardProps) {
  return (
    <Paper
      elevation={2}
      sx={{
        p: 3,
        textAlign: 'center',
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <Icon sx={{ fontSize: 48, color: `${color}.main`, mb: 1 }} />
      {isLoading ? (
        <>
          <Skeleton variant="text" width="60%" sx={{ mx: 'auto', mb: 1 }} height={40} />
          <Skeleton variant="text" width="80%" sx={{ mx: 'auto' }} />
        </>
      ) : (
        <>
          <Typography variant="h4" component="div" sx={{ fontWeight: 'bold', mb: 0.5 }}>
            {value}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {label}
          </Typography>
        </>
      )}
    </Paper>
  );
}
