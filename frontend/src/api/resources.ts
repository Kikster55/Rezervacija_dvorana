import { apiFetch } from './client';
import type { Resource, ResourceAvailability, ResourceInput } from '../types';

export const resourcesApi = {
  list: (search?: string, type?: string) => {
    const params = new URLSearchParams();
    if (search) params.set('search', search);
    if (type) params.set('type', type);
    const query = params.toString();
    return apiFetch<Resource[]>(`/api/resources${query ? `?${query}` : ''}`);
  },

  get: (id: number) => apiFetch<Resource>(`/api/resources/${id}`),

  availability: (id: number, date: string) =>
    apiFetch<ResourceAvailability>(`/api/resources/${id}/availability?date=${date}`),

  create: (input: ResourceInput) =>
    apiFetch<Resource>('/api/resources', { method: 'POST', body: JSON.stringify(input) }),

  update: (id: number, input: ResourceInput) =>
    apiFetch<Resource>(`/api/resources/${id}`, { method: 'PUT', body: JSON.stringify(input) }),

  remove: (id: number) =>
    apiFetch<{ message: string }>(`/api/resources/${id}`, { method: 'DELETE' }),
};
