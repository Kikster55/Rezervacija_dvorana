import { useCallback, useEffect, useMemo, useState } from 'react';
import { reservationsApi } from '../../api/reservations';
import { Message } from '../../components/Message';
import { formatDate, formatDateTime, formatTime, statusLabel } from '../../format';
import type { Reservation } from '../../types';

export function AdminReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [statusFilter, setStatusFilter] = useState('');
  const [search, setSearch] = useState('');

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);

    try {
      setReservations(await reservationsApi.all());
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

  // Filtriranje je namjerno na klijentu - popis je malen, a korisniku je odziv trenutan.
  const visible = useMemo(() => {
    const term = search.trim().toLowerCase();

    return reservations.filter((reservation) => {
      const matchesStatus = !statusFilter || reservation.status === statusFilter;
      const matchesSearch =
        !term ||
        reservation.resourceName.toLowerCase().includes(term) ||
        reservation.userFullName.toLowerCase().includes(term);

      return matchesStatus && matchesSearch;
    });
  }, [reservations, statusFilter, search]);

  const handleCancel = async (reservation: Reservation) => {
    if (!window.confirm(`Otkazati rezervaciju korisnika ${reservation.userFullName}?`)) {
      return;
    }

    setError(null);
    setSuccess(null);

    try {
      await reservationsApi.cancel(reservation.id);
      setSuccess('Rezervacija je otkazana.');
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Otkazivanje nije uspjelo.');
    }
  };

  return (
    <div>
      <h1>Sve rezervacije</h1>
      <p className="muted">Pregled rezervacija svih korisnika s mogućnošću otkazivanja.</p>

      <Message text={error} />
      <Message text={success} tone="success" />

      <div className="toolbar">
        <input
          placeholder="Pretraži po resursu ili korisniku"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="">Svi statusi</option>
          <option value="Active">Aktivne</option>
          <option value="Cancelled">Otkazane</option>
        </select>

        <span className="muted">
          Prikazano: <strong>{visible.length}</strong> od {reservations.length}
        </span>
      </div>

      {loading && <p className="muted">Učitavanje...</p>}

      {!loading && visible.length === 0 && <p className="muted">Nema rezervacija za prikaz.</p>}

      {visible.length > 0 && (
        <table className="table">
          <thead>
            <tr>
              <th>Korisnik</th>
              <th>Resurs</th>
              <th>Datum</th>
              <th>Vrijeme</th>
              <th>Napomena</th>
              <th>Kreirano</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {visible.map((reservation) => (
              <tr key={reservation.id} className={reservation.status === 'Cancelled' ? 'row-muted' : ''}>
                <td>{reservation.userFullName}</td>
                <td>{reservation.resourceName}</td>
                <td>{formatDate(reservation.startsAt)}</td>
                <td>
                  {formatTime(reservation.startsAt)} - {formatTime(reservation.endsAt)}
                </td>
                <td className="muted">{reservation.note ?? '-'}</td>
                <td className="muted small">{formatDateTime(reservation.createdAt)}</td>
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
