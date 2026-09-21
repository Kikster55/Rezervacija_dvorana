import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { Message } from '../components/Message';

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('korisnik@demo.hr');
  const [password, setPassword] = useState('Korisnik123!');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setBusy(true);

    try {
      await login(email, password);
      navigate('/resursi');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Prijava nije uspjela.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="centered-card">
      <h1>Prijava</h1>

      <Message text={error} />

      <form onSubmit={handleSubmit} className="form">
        <label>
          E-mail
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="username"
          />
        </label>

        <label>
          Lozinka
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
        </label>

        <button type="submit" className="btn btn-primary" disabled={busy}>
          {busy ? 'Prijava u tijeku...' : 'Prijavi se'}
        </button>
      </form>

      <p className="muted small">
        Nemaš račun? <Link to="/registracija">Registriraj se</Link>
      </p>

      <div className="demo-box">
        <strong>Demo računi</strong>
        <div>korisnik@demo.hr / Korisnik123!</div>
        <div>admin@demo.hr / Admin123!</div>
      </div>
    </div>
  );
}
