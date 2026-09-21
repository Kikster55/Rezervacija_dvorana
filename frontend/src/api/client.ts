const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5001';
const TOKEN_KEY = 'rb_token';

export const tokenStorage = {
  get: (): string | null => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY),
};

/**
 * Jedno mjesto kroz koje idu svi pozivi prema API-ju: dodaje token,
 * postavlja zaglavlja i pretvara greške s poslužitelja u obicnu Error poruku.
 */
export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = tokenStorage.get();

  const headers: Record<string, string> = {
    ...(options.body ? { 'Content-Type': 'application/json' } : {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...((options.headers as Record<string, string>) ?? {}),
  };

  const response = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

async function readErrorMessage(response: Response): Promise<string> {
  let body: unknown = null;

  try {
    const text = await response.text();
    body = text ? JSON.parse(text) : null;
  } catch {
    body = null;
  }

  if (body && typeof body === 'object') {
    const record = body as Record<string, unknown>;

    if (typeof record.message === 'string') {
      return record.message;
    }

    // Poruke iz ugrađene validacije ASP.NET-a dolaze u polju "errors".
    if (record.errors && typeof record.errors === 'object') {
      const messages = Object.values(record.errors as Record<string, string[]>).flat();
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    if (typeof record.title === 'string') {
      return record.title;
    }
  }

  if (response.status === 401) {
    return 'Prijava je istekla. Prijavi se ponovno.';
  }

  if (response.status === 403) {
    return 'Nemaš ovlasti za tu radnju.';
  }

  return `Greška ${response.status}.`;
}
