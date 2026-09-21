import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from './AuthContext';

/** Rute dostupne samo prijavljenima, a uz requireAdmin samo adminu. */
export function ProtectedRoute({ requireAdmin = false }: { requireAdmin?: boolean }) {
  const { user, loading } = useAuth();

  if (loading) {
    return <p className="muted">Učitavanje...</p>;
  }

  if (!user) {
    return <Navigate to="/prijava" replace />;
  }

  if (requireAdmin && user.role !== 'Admin') {
    return <Navigate to="/resursi" replace />;
  }

  return <Outlet />;
}
