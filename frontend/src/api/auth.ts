import { apiFetch } from './client';
import type { AuthResponse, User } from '../types';

export const authApi = {
  login: (email: string, password: string) =>
    apiFetch<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    }),

  register: (email: string, password: string, fullName: string) =>
    apiFetch<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password, fullName }),
    }),

  me: () => apiFetch<User>('/api/auth/me'),
};
