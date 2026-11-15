'use client';

import { useCallback, useState } from 'react';
import usePolling from '@/hooks/usePolling';
import { getNowPlaying } from '@/services/spotifyService';
import type { NowPlaying } from '@/types/spotify';

/**
 * How often to ask what is playing.
 *
 * Five seconds is 12 requests a minute, well inside the API's limit of 100 a minute per
 * session, and quick enough that a new song shows up while its first bars are still playing.
 */
const POLL_INTERVAL_MS = 5_000;

export default function useNowPlaying() {
  const [nowPlaying, setNowPlaying] = useState<NowPlaying | null>(null);
  // When the answer arrived, so the page can keep the clock moving between polls.
  const [fetchedAt, setFetchedAt] = useState(0);
  const [error, setError] = useState<unknown>(null);

  const refresh = useCallback(async () => {
    try {
      const latest = await getNowPlaying();
      setNowPlaying(latest);
      setFetchedAt(Date.now());
      setError(null);
    } catch (err) {
      setError(err);
    }
  }, []);

  usePolling(() => void refresh(), POLL_INTERVAL_MS);

  return { nowPlaying, fetchedAt, error, refresh };
}
