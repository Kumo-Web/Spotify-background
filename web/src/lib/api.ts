// Empty means same-origin: Next.js proxies /api/* to the backend (see next.config.ts), which is
// what keeps the session cookie on one host. Only set this to call the API directly.
const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? '';

export class ApiError extends Error {
  constructor(
    readonly status: number,
    message: string,
    options?: ErrorOptions,
  ) {
    super(message, options);
    this.name = 'ApiError';
  }

  /** The session is missing or expired, so the caller should send the user to log in again. */
  get isUnauthenticated() {
    return this.status === 401;
  }

  /** The request never reached the API, so there is no status — see the 0 set by apiFetch. */
  get isUnreachable() {
    return this.status === 0;
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
  let response: Response;

  try {
    response = await fetch(`${API_BASE}${path}`, {
      ...init,
      credentials: 'include',
      headers: {
        Accept: 'application/json',
        ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
        ...init?.headers,
      },
    });
  } catch (cause) {
    // fetch() rejects with a bare "Failed to fetch" for every network-level failure, which says
    // nothing about which one. Locally it is almost always one of two things, so name them both
    // rather than propagating three useless words to the UI.
    throw new ApiError(
      0,
      `Could not reach the API at ${API_BASE || 'this origin (proxied by Next.js)'}. ` +
        `Check that it is running, and that \`pnpm certs\` has been run so the Next.js proxy ` +
        `trusts its development certificate.`,
      { cause },
    );
  }

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
