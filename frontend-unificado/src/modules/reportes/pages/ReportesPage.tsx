/**
 * ReportesPage - Reports and analytics (Admin/Organizator only)
 * Displays event metrics, attendance history, and financial reconciliation
 */

import {
  Container,
  Typography,
  Box,
  Tabs,
  Tab,
  Button,
  CircularProgress,
} from '@mui/material';
import { useState } from 'react';
import { useMetricasEventos } from '../hooks/useMetricasEventos';
import { useHistorialAsistencia } from '../hooks/useHistorialAsistencia';
import { useConciliacionFinanciera } from '../hooks/useConciliacionFinanciera';
import { useExportarReporte } from '../hooks/useExportarReporte';
import { useEventos } from '../../eventos/hooks/useEventos';
import { MetricasEventos } from '../components/MetricasEventos';
import { HistorialAsistencia } from '../components/HistorialAsistencia';
import { ConciliacionFinanciera } from '../components/ConciliacionFinanciera';
import { ReporteFiltros } from '../components/ReporteFiltros';
import type { ReporteFiltros as ReporteFiltrosType } from '../types';
import DownloadIcon from '@mui/icons-material/Download';
import { useToast } from '../../../shared/hooks';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`report-tabpanel-${index}`}
      aria-labelledby={`report-tab-${index}`}
      {...other}
    >
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

export function ReportesPage() {
  const [tabValue, setTabValue] = useState(0);
  const [filtros, setFiltros] = useState<ReporteFiltrosType>({
    fechaInicio: new Date(new Date().setMonth(new Date().getMonth() - 1)),
    fechaFin: new Date(),
  });

  const { showToast } = useToast();

  // Fetch data
  const {
    data: metricas = [],
    isLoading: isLoadingMetricas,
    error: errorMetricas,
  } = useMetricasEventos(filtros);

  const {
    data: asistencia = null,
    isLoading: isLoadingAsistencia,
    error: errorAsistencia,
  } = useHistorialAsistencia(filtros);

  const {
    data: conciliacion = null,
    isLoading: isLoadingConciliacion,
    error: errorConciliacion,
  } = useConciliacionFinanciera(filtros);

  // Fetch eventos for filter dropdown
  const { data: eventosData } = useEventos();
  const eventos =
    eventosData?.map((e) => ({ id: e.id, nombre: e.nombre })) || [];

  // Export functionality
  const { mutate: exportar, isPending: isExporting } = useExportarReporte();

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue);
  };

  const handleFiltrosChange = (newFiltros: ReporteFiltrosType) => {
    setFiltros(newFiltros);
  };

  const handleExportar = () => {
    const tipoReporte =
      tabValue === 0
        ? 'metricas'
        : tabValue === 1
          ? 'asistencia'
          : 'conciliacion';

    exportar(
      {
        tipo: tipoReporte,
        formato: 'csv',
        filtros,
      },
      {
        onSuccess: () => {
          showToast('Reporte exportado exitosamente', 'success');
        },
        onError: () => {
          showToast('Error al exportar reporte', 'error');
        },
      }
    );
  };

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      {/* Header */}
      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Typography variant="h3" component="h1">
          Reportes y Análisis
        </Typography>
        <Button
          variant="contained"
          startIcon={
            isExporting ? <CircularProgress size={20} /> : <DownloadIcon />
          }
          onClick={handleExportar}
          disabled={isExporting}
        >
          Exportar a Excel
        </Button>
      </Box>

      {/* Filters */}
      <ReporteFiltros
        filtros={filtros}
        onFiltrosChange={handleFiltrosChange}
        eventos={eventos}
        showEventoFilter={tabValue === 1} // Only show event filter for attendance
      />

      {/* Tabs */}
      <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
        <Tabs value={tabValue} onChange={handleTabChange}>
          <Tab label="Métricas de Eventos" />
          <Tab label="Historial de Asistencia" />
          <Tab label="Conciliación Financiera" />
        </Tabs>
      </Box>

      {/* Tab Panels */}
      <TabPanel value={tabValue} index={0}>
        <MetricasEventos
          metricas={metricas}
          isLoading={isLoadingMetricas}
          error={errorMetricas}
        />
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <HistorialAsistencia
          asistencia={asistencia}
          isLoading={isLoadingAsistencia}
          error={errorAsistencia}
        />
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <ConciliacionFinanciera
          conciliacion={conciliacion}
          isLoading={isLoadingConciliacion}
          error={errorConciliacion}
        />
      </TabPanel>
    </Container>
  );
}
