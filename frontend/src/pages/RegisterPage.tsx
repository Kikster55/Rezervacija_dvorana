import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { Message } from '../components/Message';

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);
    setBusy(true);

    try {
      await register(email, password, fullName);
      navigate('/resursi');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registracija nije uspjela.');
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="centered-card">
      <h1>Registracija</h1>

      <Message text={error} />

      <form onSubmit={handleSubmit} className="form">
        <label>
          Ime i prezime
          <input value={fullName} onChange={(e) => setFullName(e.target.value)} required />
        </label>

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
            minLength={6}
            autoComplete="new-password"
          />
          <span className="hint">Najmanje 6 znakova.</span>
        </label>

        <button type="submit" className="btn btn-primary" disabled={busy}>
          {busy ? 'Spremanje...' : 'Registriraj se'}
        </button>
      </form>

      <p className="muted small">
        Već imaš račun? <Link to="/prijava">Prijavi se</Link>
      </p>
    </div>
  );
}
