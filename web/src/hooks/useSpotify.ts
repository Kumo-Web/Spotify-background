'use client';

import { useCallback, useEffect, useState } from 'react';
import { ApiError, startLogin } from '../lib/api';
import { getSession, logout as logoutRequest } from '../services/spotifyService';
import type { Session } from '../types/spotify';

/**
 * Session state for the current browser.
 *
 * There is no token handling here by design — the Spotify access token stays server-side. The
 * client only ever knows whether it has a session.
 */
export default function useSpotify() {
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
   * Every caller of this hook holds its own independent copy of the session, so updating only
   * this one would leave the rest of the page rendering as though still signed in. A full
   * navigation rebuilds all of them from the now-absent cookie.
   */
  const logout = useCallback(async () => {
    await logoutRequest();
    window.location.assign('/');
  }, []);

  return { session, loading, error, login, logout, refresh };
}
