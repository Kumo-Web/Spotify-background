'use client';

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError, startLogin } from '@/lib/api';
import { getSession, logout as logoutRequest } from '@/services/spotifyService';
import type { Session } from '@/types/spotify';

interface SessionContextValue {
  session: Session | null;
  loading: boolean;
  error: string | null;
  login: (returnPath?: string) => void;
  logout: () => Promise<void>;
  refresh: () => Promise<void>;
}

const SessionContext = createContext<SessionContextValue | null>(null);

/**
 * Session state for the current browser, fetched once and shared.
 *
 * This replaced a hook that each component called for itself. Every caller then held its own
 * copy and made its own request, so the header and the page could disagree about whether you
 * were signed in. One provider at the root means one request and one answer.
 *
 * There is no token handling here by design — the Spotify access token stays server-side. The
 * client only ever knows whether it has a session.
 */
export function SessionProvider({ children }: { children: React.ReactNode }) {
  const [session, setSession] = useState<Session | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    try {
      setSession(await getSession());
      setError(null);
    } catch (err) {
      // "Not signed in" and "the API never answered" need different fixes, and collapsing both
      // into authenticated:false is what sends you round the /login redirect loop with nothing
      // on screen to explain why. /api/auth/session is anonymous and answers 200 either way, so
      // anything thrown here is a transport or server failure, not a missing session.
      setSession({ authenticated: false });
      setError(err instanceof ApiError && err.isUnreachable ? err.message : null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const login = useCallback((returnPath?: string) => startLogin(returnPath), []);

  /**
   * Signs out, then reloads rather than updating state in place.
   *
   * A full navigation throws away everything this tab was holding in memory — the previous
   * user's playlists, the track on the turntable — instead of relying on every component to
   * remember to clear its own.
   */
  const logout = useCallback(async () => {
    await logoutRequest();
    window.location.assign('/login');
  }, []);

  const value = useMemo(
    () => ({ session, loading, error, login, logout, refresh }),
    [session, loading, error, login, logout, refresh],
  );

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession(): SessionContextValue {
  const value = useContext(SessionContext);

  if (!value) {
    throw new Error('useSession must be used inside <SessionProvider>.');
  }

  return value;
}
