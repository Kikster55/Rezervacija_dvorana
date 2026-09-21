import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { resourcesApi } from '../api/resources';
import { reservationsApi } from '../api/reservations';
import { Message } from '../components/Message';
import { formatDate, formatTime, resourceTypeLabel, shortTime, todayInputValue } from '../format';
import type { Resource, TimeSlot } from '../types';

export function ResourceDetailPage() {
  const { id } = useParams();
  const resourceId = Number(id);

  const [resource, setResource] = useState<Resource | null>(null);
  const [date, setDate] = useState(todayInputValue());
  const [slots, setSlots] = useState<TimeSlot[]>([]);
  const [selected, setSelected] = useState<TimeSlot | null>(null);
  const [note, setNote] = useState('');

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);

  const loadAvailability = useCallback(async () => {
    setLoading(true);

    try {
      const [resourceData, availability] = await Promise.all([
        resourcesApi.get(resourceId),
        resourcesApi.availability(resourceId, date),
      ]);

      setResource(resourceData);
      setSlots(availability.slots);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Dohvat termina nije uspio.');
    } finally {
      setLoading(false);
    }
  }, [resourceId, date]);

  useEffect(() => {
    setSelected(null);
    void loadAvailability();
  }, [loadAvailability]);

  const handleReserve = async () => {
    if (!selected) {
      return;
    }

    setBusy(true);
    setError(null);
    setSuccess(null);

    try {
      await reservationsApi.create(resourceId, selected.start, selected.end, note);
      setSuccess(
        `Rezervirano: ${formatDate(selected.start)} od ${formatTime(selected.start)} do ${formatTime(selected.end)}.`,
      );
      setSelected(null);
      setNote('');
      await loadAvailability();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Rezervacija nije uspjela.');
    } finally {
      setBusy(false);
    }
  };

  if (loading && !resource) {
    return <p className="muted">Učitavanje...</p>;
  }

  if (!resource) {
    return (
      <div>
        <Message text={error} />
        <Link to="/resursi">Natrag na popis</Link>
      </div>
    );
  }

  const freeCount = slots.filter((slot) => slot.isAvailable).length;

  return (
    <div>
      <Link to="/resursi" className="back-link">
        &larr; Svi resursi
      </Link>

      <div className="card-head">
        <h1>{resource.name}</h1>
        <span className="badge">{resourceTypeLabel(resource.type)}</span>
      </div>

      <p className="muted">
        {resource.location} &middot; kapacitet {resource.capacity} &middot; radno vrijeme{' '}
        {shortTime(resource.openingTime)} - {shortTime(resource.closingTime)} &middot; termin{' '}
        {resource.slotMinutes} min
      </p>

      {resource.description && <p>{resource.description}</p>}

      <Message text={error} />
      <Message text={success} tone="success" />

      <div className="toolbar">
        <label className="inline-label">
          Datum
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        </label>
        <span className="muted">
          Slobodnih termina: <strong>{freeCount}</strong> od {slots.length}
        </span>
      </div>

      {slots.length === 0 && !loading && (
        <p className="muted">Za taj datum nema termina u radnom vremenu resursa.</p>
      )}

      <div className="slot-grid">
        {slots.map((slot) => {
          const isSelected = selected?.start === slot.start;

          return (
            <button
              key={slot.start}
              type="button"
              className={`slot ${slot.isAvailable ? 'slot-free' : 'slot-taken'} ${
                isSelected ? 'slot-selected' : ''
              }`}
              disabled={!slot.isAvailable}
              onClick={() => setSelected(slot)}
            >
              {formatTime(slot.start)} - {formatTime(slot.end)}
              <span className="slot-state">{slot.isAvailable ? 'slobodno' : 'zauzeto'}</span>
            </button>
          );
        })}
      </div>

      {selected && (
        <div className="panel">
          <h2>Potvrda rezervacije</h2>
          <p>
            {formatDate(selected.start)}, {formatTime(selected.start)} - {formatTime(selected.end)}
          </p>

          <label>
            Napomena (nije obavezno)
            <input
              value={note}
              onChange={(e) => setNote(e.target.value)}
              maxLength={300}
              placeholder="npr. sastanak tima"
            />
          </label>

          <div className="row-actions">
            <button type="button" className="btn btn-primary" onClick={handleReserve} disabled={busy}>
              {busy ? 'Spremanje...' : 'Rezerviraj'}
            </button>
            <button type="button" className="btn btn-quiet" onClick={() => setSelected(null)}>
              Odustani
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
