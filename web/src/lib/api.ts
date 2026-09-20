const API_BASE = process.env.NEXT_PUBLIC_API_URL;

if (!API_BASE && typeof window !== 'undefined') {
  console.error('NEXT_PUBLIC_API_URL is not set; every API call will fail.');
}

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  /** The session is missing or expired, so the caller should send the user to log in again. */
  get isUnauthenticated() {
    return this.status === 401;
  }
}

/**
 * Calls the API with the session cookie attached.
 *
 * `credentials: 'include'` is essential and easy to miss: the API authenticates with an
 * HttpOnly cookie, and browsers omit cookies from cross-origin fetches unless asked. Without
 * it every authenticated request silently 401s.
 */
export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    credentials: 'include',
    headers: {
      Accept: 'application/json',
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw new ApiError(response.status, `${init?.method ?? 'GET'} ${path} failed (${response.status})`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

/**
 * Begins login by navigating to the API, which redirects on to Spotify.
 *
 * A full navigation rather than a fetch, because this is an OAuth redirect chain the browser
 * has to follow. Note there is no user id involved — the previous implementation sent a
 * hardcoded GUID, which is how any caller could start a flow as anyone.
 */
export function startLogin(returnPath?: string) {
  const query = returnPath ? `?returnPath=${encodeURIComponent(returnPath)}` : '';
  window.location.href = `${API_BASE}/api/auth/login${query}`;
}
