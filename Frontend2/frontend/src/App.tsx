import { BrowserRouter, Routes, Route, Navigate, useParams } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import EventDetailPage from './pages/EventDetailPage';
import AdminDashboard from './pages/AdminDashboard';
import OrganizerDashboard from './pages/OrganizerDashboard';
import EventForm from './components/EventForm';
import { useEvent } from './api/events';

// Wrapper for Edit Event to fetch data
const EditEventWrapper = ({ redirectUrl }: { redirectUrl: string }) => {
  const { id } = useParams<{ id: string }>();
  const { data: event, isLoading, error } = useEvent(id!);

  if (isLoading) return <div>Cargando...</div>;
  if (error || !event) return <div>Error cargando el evento</div>;

  return <EventForm mode="edit" initialData={event} redirectUrl={redirectUrl} />;
};

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Layout />}>
            {/* Redirect root to /usuario */}
            <Route index element={<Navigate to="/usuario" replace />} />

            {/* User (Public) Routes */}
            <Route path="usuario">
              <Route index element={<HomePage />} />
              <Route path="evento/:id" element={<EventDetailPage />} />
            </Route>

            {/* Admin Routes */}
            <Route path="admin">
              <Route path="dashboard" element={<AdminDashboard />} />
              <Route path="crear" element={<EventForm mode="create" redirectUrl="/admin/dashboard" />} />
              <Route path="editar/:id" element={<EditEventWrapper redirectUrl="/admin/dashboard" />} />
            </Route>

            {/* Organizer Routes */}
            <Route path="organizador">
              <Route path="dashboard" element={<OrganizerDashboard />} />
              <Route path="crear" element={<EventForm mode="create" redirectUrl="/organizador/dashboard" />} />
              <Route path="editar/:id" element={<EditEventWrapper redirectUrl="/organizador/dashboard" />} />
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
