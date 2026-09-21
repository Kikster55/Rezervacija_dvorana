import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { resourcesApi } from '../api/resources';
import { Message } from '../components/Message';
import { resourceTypeLabel, shortTime } from '../format';
import type { Resource } from '../types';

export function ResourcesPage() {
  const [resources, setResources] = useState<Resource[]>([]);
  const [search, setSearch] = useState('');
  const [type, setType] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    // Kratka odgoda da se popis ne dohvaća na svako pritisnuto slovo.
    const timer = setTimeout(() => {
      setLoading(true);

      resourcesApi
        .list(search, type)
        .then((data) => {
          if (!cancelled) {
            setResources(data);
            setError(null);
          }
        })
        .catch((err: unknown) => {
          if (!cancelled) {
            setError(err instanceof Error ? err.message : 'Dohvat resursa nije uspio.');
          }
        })
        .finally(() => {
          if (!cancelled) {
            setLoading(false);
          }
        });
    }, 250);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [search, type]);

  return (
    <div>
      <h1>Resursi</h1>
      <p className="muted">Odaberi resurs da vidiš slobodne termine i rezerviraš.</p>

      <div className="toolbar">
        <input
          placeholder="Pretraži po nazivu ili lokaciji"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <select value={type} onChange={(e) => setType(e.target.value)}>
          <option value="">Sve vrste</option>
          <option value="MeetingRoom">Sale</option>
          <option value="Equipment">Oprema</option>
        </select>
      </div>

      <Message text={error} />

      {loading && <p className="muted">Učitavanje...</p>}

      {!loading && resources.length === 0 && (
        <p className="muted">Nema resursa koji odgovaraju pretrazi.</p>
      )}

      <div className="card-grid">
        {resources.map((resource) => (
          <Link key={resource.id} to={`/resursi/${resource.id}`} className="card">
            <div className="card-head">
              <h2>{resource.name}</h2>
              <span className="badge">{resourceTypeLabel(resource.type)}</span>
            </div>

            {resource.description && <p className="muted small">{resource.description}</p>}

            <dl className="card-facts">
              <div>
                <dt>Lokacija</dt>
                <dd>{resource.location}</dd>
              </div>
              <div>
                <dt>Kapacitet</dt>
                <dd>{resource.capacity}</dd>
              </div>
              <div>
                <dt>Radno vrijeme</dt>
                <dd>
                  {shortTime(resource.openingTime)} - {shortTime(resource.closingTime)}
                </dd>
              </div>
              <div>
                <dt>Termin</dt>
                <dd>{resource.slotMinutes} min</dd>
              </div>
            </dl>

            {!resource.isActive && <span className="badge badge-warn">Neaktivan</span>}
          </Link>
        ))}
      </div>
    </div>
  );
}
