import React from 'react';
import { Box, Container, Grid } from '@mui/material';
import CategoryManagementSection from './CategoryManagementSection';
import SeatAssignmentSection from './SeatAssignmentSection';
import SeatGrid from './SeatGrid';
import { useEventSeats, useEventCategories } from '../api/seats';

interface SeatManagementTabProps {
  eventoId: string;
  mapaId: string;
}

const SeatManagementTab: React.FC<SeatManagementTabProps> = ({ eventoId, mapaId }) => {
  const { data: seats, isLoading } = useEventSeats(eventoId);
  const { data: categories } = useEventCategories(mapaId);

  return (
    <Container maxWidth="xl" sx={{ py: { xs: 1, sm: 2, md: 3 }, px: { xs: 0, sm: 2 } }}>
      <Grid container spacing={{ xs: 2, sm: 3 }}>
        {/* Left column: Category and Seat Management */}
        <Grid item xs={12} lg={6}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: { xs: 2, sm: 3 } }}>
            <CategoryManagementSection eventoId={eventoId} mapaId={mapaId} />
            <SeatAssignmentSection eventoId={eventoId} mapaId={mapaId} />
          </Box>
        </Grid>

        {/* Right column: Seat Grid Visualization */}
        <Grid item xs={12} lg={6}>
          <SeatGrid seats={seats} categories={categories} isInteractive={false} />
        </Grid>
      </Grid>
    </Container>
  );
};

export default SeatManagementTab;
