import { useCallback, useEffect, useState } from 'react';
import { reservationsApi } from '../api/reservations';
import { Message } from '../components/Message';
import { formatDate, formatTime, statusLabel } from '../format';
import type { Reservation, ReservationStats } from '../types';

export function MyReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [stats, setStats] = useState<ReservationStats | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);

    try {
      const [list, statsData] = await Promise.all([
        reservationsApi.mine(),
        reservationsApi.stats(),
      ]);

      setReservations(list);
      setStats(statsData);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Dohvat rezervacija nije uspio.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const handleCancel = async (reservation: Reservation) => {
    setError(null);
    setSuccess(null);

    try {
      await reservationsApi.cancel(reservation.id);
      setSuccess(`Rezervacija za ${reservation.resourceName} je otkazana.`);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Otkazivanje nije uspjelo.');
    }
  };

  return (
    <div>
      <h1>Moje rezervacije</h1>

      <Message text={error} />
      <Message text={success} tone="success" />

      {stats && (
        <div className="stat-row">
          <div className="stat">
            <span className="stat-value">{stats.total}</span>
            <span className="stat-label">Ukupno rezervacija</span>
          </div>
          <div className="stat">
            <span className="stat-value">{stats.active}</span>
            <span className="stat-label">Aktivnih</span>
          </div>
          <div className="stat">
            <span className="stat-value">{stats.cancelled}</span>
            <span className="stat-label">Otkazanih</span>
          </div>
          <div className="stat">
            <span className="stat-value">{stats.hoursBooked}</span>
            <span className="stat-label">Rezerviranih sati</span>
          </div>
          <div className="stat stat-wide">
            <span className="stat-value">{stats.mostUsedResource ?? '-'}</span>
            <span className="stat-label">Najčešće korišten resurs</span>
          </div>
        </div>
      )}

      {loading && <p className="muted">Učitavanje...</p>}

      {!loading && reservations.length === 0 && (
        <p className="muted">Još nemaš rezervacija. Odaberi resurs i rezerviraj termin.</p>
      )}

      {reservations.length > 0 && (
        <table className="table">
          <thead>
            <tr>
              <th>Resurs</th>
              <th>Datum</th>
              <th>Vrijeme</th>
              <th>Napomena</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {reservations.map((reservation) => (
              <tr key={reservation.id} className={reservation.status === 'Cancelled' ? 'row-muted' : ''}>
                <td>{reservation.resourceName}</td>
                <td>{formatDate(reservation.startsAt)}</td>
                <td>
                  {formatTime(reservation.startsAt)} - {formatTime(reservation.endsAt)}
                </td>
                <td className="muted">{reservation.note ?? '-'}</td>
                <td>
                  <span
                    className={`badge ${
                      reservation.status === 'Active' ? 'badge-ok' : 'badge-warn'
                    }`}
                  >
                    {statusLabel(reservation.status)}
                  </span>
                </td>
                <td>
                  {reservation.status === 'Active' && (
                    <button
                      type="button"
                      className="btn btn-quiet"
                      onClick={() => handleCancel(reservation)}
                    >
                      Otkaži
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
