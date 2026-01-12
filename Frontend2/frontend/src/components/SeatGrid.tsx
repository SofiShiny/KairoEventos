import React from 'react';
import { Paper, Typography } from '@mui/material';
import type { Asiento, Categoria } from '../types/api';

interface SeatGridProps {
  seats?: Asiento[] | null | any;
  categories?: Categoria[];
  onSeatClick?: (seatId: string) => void;
  isInteractive?: boolean;
}

const SeatGrid: React.FC<SeatGridProps> = ({ seats = [], categories = [], isInteractive = false, onSeatClick }) => {
  // Normalize seats to an array and extract categories from response
  let seatsArray: Asiento[] = [];
  let categoriesArray: Categoria[] = categories || [];

  if (Array.isArray(seats)) {
    seatsArray = seats;
  } else if (seats && Array.isArray(seats.asientos)) {
    seatsArray = seats.asientos;
    // If categories are included in the seats response, use those
    if (seats.categorias && Array.isArray(seats.categorias)) {
      categoriesArray = seats.categorias;
    }
  } else {
    seatsArray = [];
  }

  // Helper function to find category by name (backend returns category name in seat.categoria)
  const findCategoryByName = (categoryName?: string) => {
    if (!categoryName || !categoriesArray) return null;
    return categoriesArray.find(cat => cat.nombre === categoryName);
  };

  // Sort seats by row and number
  const sortedSeats = [...seatsArray].sort((a, b) => {
    // Convert fila to string for comparison
    const filaA = String(a.fila);
    const filaB = String(b.fila);
    
    if (filaA === filaB) {
      return a.numero - b.numero;
    }
    return filaA.localeCompare(filaB);
  });

  return (
    <Paper 
      elevation={2} 
      sx={{ 
        p: 2, 
        mt: { xs: 2, md: 0 }, 
        overflowX: 'auto',
      }}
    >
      <Typography variant="h6" gutterBottom>
        Lista de Asientos
      </Typography>
      
      {sortedSeats.length > 0 ? (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #ddd' }}>
              <th style={{ padding: '8px', textAlign: 'left' }}>Fila</th>
              <th style={{ padding: '8px', textAlign: 'left' }}>Número</th>
              <th style={{ padding: '8px', textAlign: 'left' }}>Categoría</th>
              <th style={{ padding: '8px', textAlign: 'left' }}>Precio</th>
              <th style={{ padding: '8px', textAlign: 'left' }}>Estado</th>
            </tr>
          </thead>
          <tbody>
            {sortedSeats.map((seat: any) => {
              // Backend returns category name in seat.categoria field
              const categoryName = seat.categoria;
              const category = findCategoryByName(categoryName);
              const estado = seat.reservado ? 'Reservado' : 'Disponible';
              const isAvailable = estado === 'Disponible';
              
              return (
                <tr 
                  key={seat.id} 
                  style={{ 
                    borderBottom: '1px solid #eee',
                    cursor: isAvailable && isInteractive ? 'pointer' : 'default',
                    backgroundColor: isAvailable && isInteractive ? 'transparent' : undefined,
                  }}
                  onClick={() => {
                    if (isAvailable && isInteractive && onSeatClick) {
                      onSeatClick(seat.id);
                    }
                  }}
                  onMouseEnter={(e) => {
                    if (isAvailable && isInteractive) {
                      e.currentTarget.style.backgroundColor = '#f5f5f5';
                    }
                  }}
                  onMouseLeave={(e) => {
                    if (isAvailable && isInteractive) {
                      e.currentTarget.style.backgroundColor = 'transparent';
                    }
                  }}
                >
                  <td style={{ padding: '8px' }}>{seat.fila}</td>
                  <td style={{ padding: '8px' }}>{seat.numero}</td>
                  <td style={{ padding: '8px' }}>
                    {categoryName || 'Sin categoría'}
                  </td>
                  <td style={{ padding: '8px' }}>
                    ${category?.precioBase?.toFixed(2) || '0.00'}
                  </td>
                  <td style={{ padding: '8px' }}>
                    <span style={{
                      padding: '4px 8px',
                      borderRadius: '4px',
                      backgroundColor: 
                        estado === 'Disponible' ? '#e3f2fd' :
                        estado === 'Reservado' ? '#fff3e0' :
                        '#ffebee',
                      color:
                        estado === 'Disponible' ? '#1976d2' :
                        estado === 'Reservado' ? '#f57c00' :
                        '#d32f2f'
                    }}>
                      {estado}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      ) : (
        <Typography color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
          No hay asientos creados aún. Crea asientos usando el formulario de asignación.
        </Typography>
      )}
    </Paper>
  );
};

export default SeatGrid;
