import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { resourcesApi } from '../../api/resources';
import { Message } from '../../components/Message';
import { resourceTypeLabel, shortTime } from '../../format';
import type { Resource, ResourceInput, ResourceType } from '../../types';

const emptyForm: ResourceInput = {
  name: '',
  type: 'MeetingRoom',
  location: '',
  capacity: 1,
  description: '',
  openingTime: '08:00',
  closingTime: '20:00',
  slotMinutes: 60,
  isActive: true,
};

export function AdminResourcesPage() {
  const [resources, setResources] = useState<Resource[]>([]);
  const [form, setForm] = useState<ResourceInput>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const load = useCallback(async () => {
    try {
      setResources(await resourcesApi.list());
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Dohvat resursa nije uspio.');
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const startEdit = (resource: Resource) => {
    setEditingId(resource.id);
    setSuccess(null);
    setError(null);
    setForm({
      name: resource.name,
      type: resource.type,
      location: resource.location,
      capacity: resource.capacity,
      description: resource.description ?? '',
      openingTime: shortTime(resource.openingTime),
      closingTime: shortTime(resource.closingTime),
      slotMinutes: resource.slotMinutes,
      isActive: resource.isActive,
    });
  };

  const resetForm = () => {
    setEditingId(null);
    setForm(emptyForm);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    setSuccess(null);

    try {
      if (editingId === null) {
        await resourcesApi.create(form);
        setSuccess('Resurs je dodan.');
      } else {
        await resourcesApi.update(editingId, form);
        setSuccess('Resurs je spremljen.');
      }

      resetForm();
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Spremanje nije uspjelo.');
    } finally {
      setBusy(false);
    }
  };

  const handleDelete = async (resource: Resource) => {
    if (!window.confirm(`Obrisati resurs "${resource.name}"?`)) {
      return;
    }

    setError(null);
    setSuccess(null);

    try {
      const result = await resourcesApi.remove(resource.id);
      setSuccess(result.message);

      if (editingId === resource.id) {
        resetForm();
      }

      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Brisanje nije uspjelo.');
    }
  };

  return (
    <div>
      <h1>Upravljanje resursima</h1>

      <Message text={error} />
      <Message text={success} tone="success" />

      <div className="panel">
        <h2>{editingId === null ? 'Novi resurs' : 'Uređivanje resursa'}</h2>

        <form onSubmit={handleSubmit} className="form form-grid">
          <label>
            Naziv
            <input
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              required
              maxLength={150}
            />
          </label>

          <label>
            Vrsta
            <select
              value={form.type}
              onChange={(e) => setForm({ ...form, type: e.target.value as ResourceType })}
            >
              <option value="MeetingRoom">Sala</option>
              <option value="Equipment">Oprema</option>
            </select>
          </label>

          <label>
            Lokacija
            <input
              value={form.location}
              onChange={(e) => setForm({ ...form, location: e.target.value })}
              required
              maxLength={150}
            />
          </label>

          <label>
            Kapacitet
            <input
              type="number"
              min={1}
              max={1000}
              value={form.capacity}
              onChange={(e) => setForm({ ...form, capacity: Number(e.target.value) })}
              required
            />
          </label>

          <label>
            Otvaranje
            <input
              type="time"
              value={form.openingTime}
              onChange={(e) => setForm({ ...form, openingTime: e.target.value })}
              required
            />
          </label>

          <label>
            Zatvaranje
            <input
              type="time"
              value={form.closingTime}
              onChange={(e) => setForm({ ...form, closingTime: e.target.value })}
              required
            />
          </label>

          <label>
            Trajanje termina (min)
            <input
              type="number"
              min={15}
              max={480}
              step={15}
              value={form.slotMinutes}
              onChange={(e) => setForm({ ...form, slotMinutes: Number(e.target.value) })}
              required
            />
          </label>

          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
            />
            Aktivan
          </label>

          <label className="span-2">
            Opis
            <input
              value={form.description ?? ''}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              maxLength={500}
            />
          </label>

          <div className="row-actions span-2">
            <button type="submit" className="btn btn-primary" disabled={busy}>
              {editingId === null ? 'Dodaj resurs' : 'Spremi izmjene'}
            </button>
            {editingId !== null && (
              <button type="button" className="btn btn-quiet" onClick={resetForm}>
                Odustani
              </button>
            )}
          </div>
        </form>
      </div>

      <table className="table">
        <thead>
          <tr>
            <th>Naziv</th>
            <th>Vrsta</th>
            <th>Lokacija</th>
            <th>Kapacitet</th>
            <th>Radno vrijeme</th>
            <th>Termin</th>
            <th>Status</th>
            <th />
          </tr>
        </thead>
        <tbody>
          {resources.map((resource) => (
            <tr key={resource.id} className={resource.isActive ? '' : 'row-muted'}>
              <td>{resource.name}</td>
              <td>{resourceTypeLabel(resource.type)}</td>
              <td>{resource.location}</td>
              <td>{resource.capacity}</td>
              <td>
                {shortTime(resource.openingTime)} - {shortTime(resource.closingTime)}
              </td>
              <td>{resource.slotMinutes} min</td>
              <td>
                <span className={`badge ${resource.isActive ? 'badge-ok' : 'badge-warn'}`}>
                  {resource.isActive ? 'Aktivan' : 'Neaktivan'}
                </span>
              </td>
              <td className="row-actions">
                <button type="button" className="btn btn-quiet" onClick={() => startEdit(resource)}>
                  Uredi
                </button>
                <button type="button" className="btn btn-danger" onClick={() => handleDelete(resource)}>
                  Obriši
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
