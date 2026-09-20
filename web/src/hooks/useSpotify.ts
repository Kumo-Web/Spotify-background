'use client';

import { useCallback, useEffect, useState } from 'react';
import { startLogin } from '../lib/api';
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
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    try {
      setSession(await getSession());
    } catch {
      setSession({ authenticated: false });
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const login = useCallback((returnPath?: string) => startLogin(returnPath), []);

  const logout = useCallback(async () => {
    await logoutRequest();
    await refresh();
  }, [refresh]);

  return { session, loading, login, logout, refresh };
}
