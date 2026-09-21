import { apiFetch } from './client';
import type { Reservation, ReservationStats } from '../types';

export const reservationsApi = {
  create: (resourceId: number, startsAt: string, endsAt: string, note: string) =>
    apiFetch<Reservation>('/api/reservations', {
      method: 'POST',
      body: JSON.stringify({ resourceId, startsAt, endsAt, note: note || null }),
    }),

  mine: () => apiFetch<Reservation[]>('/api/reservations/my'),

  stats: () => apiFetch<ReservationStats>('/api/reservations/my/stats'),

  cancel: (id: number) =>
    apiFetch<Reservation>(`/api/reservations/${id}`, { method: 'DELETE' }),

  all: () => apiFetch<Reservation[]>('/api/reservations'),
};
