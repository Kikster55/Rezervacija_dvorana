import { NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

export function Navbar() {
  const { user, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  if (!user) {
    return null;
  }

  const handleLogout = () => {
    logout();
    navigate('/prijava');
  };

  return (
    <header className="navbar">
      <div className="navbar-inner">
        <span className="brand">Rezervacija resursa</span>

        <nav className="nav-links">
          <NavLink to="/resursi">Resursi</NavLink>
          <NavLink to="/moje-rezervacije">Moje rezervacije</NavLink>
          {isAdmin && <NavLink to="/admin/resursi">Upravljanje resursima</NavLink>}
          {isAdmin && <NavLink to="/admin/rezervacije">Sve rezervacije</NavLink>}
        </nav>

        <div className="nav-user">
          <span className="muted">
            {user.fullName}
            {isAdmin && <span className="badge badge-admin">Admin</span>}
          </span>
          <button type="button" className="btn btn-quiet" onClick={handleLogout}>
            Odjava
          </button>
        </div>
      </div>
    </header>
  );
}
