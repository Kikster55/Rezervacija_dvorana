import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { Navbar } from './components/Navbar';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { ResourcesPage } from './pages/ResourcesPage';
import { ResourceDetailPage } from './pages/ResourceDetailPage';
import { MyReservationsPage } from './pages/MyReservationsPage';
import { AdminResourcesPage } from './pages/admin/AdminResourcesPage';
import { AdminReservationsPage } from './pages/admin/AdminReservationsPage';

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Navbar />

        <main className="container">
          <Routes>
            <Route path="/prijava" element={<LoginPage />} />
            <Route path="/registracija" element={<RegisterPage />} />

            {/* Rute za prijavljene korisnike */}
            <Route element={<ProtectedRoute />}>
              <Route path="/resursi" element={<ResourcesPage />} />
              <Route path="/resursi/:id" element={<ResourceDetailPage />} />
              <Route path="/moje-rezervacije" element={<MyReservationsPage />} />
            </Route>

            {/* Rute samo za admina */}
            <Route element={<ProtectedRoute requireAdmin />}>
              <Route path="/admin/resursi" element={<AdminResourcesPage />} />
              <Route path="/admin/rezervacije" element={<AdminReservationsPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/resursi" replace />} />
          </Routes>
        </main>
      </AuthProvider>
    </BrowserRouter>
  );
}
