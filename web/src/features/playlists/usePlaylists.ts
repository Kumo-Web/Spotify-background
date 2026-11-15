'use client';

import { useEffect, useState } from 'react';
import { getPlaylists } from '@/services/spotifyService';
import type { Playlist } from '@/types/spotify';

/** The signed-in user's playlists, loaded once. `playlists` is null until the answer arrives. */
export default function usePlaylists() {
  const [playlists, setPlaylists] = useState<Playlist[] | null>(null);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    // Guards against setting state after the component has gone, e.g. on a quick navigation.
    let cancelled = false;

    getPlaylists()
      .then((result) => {
        if (!cancelled) setPlaylists(result);
      })
      .catch((err: unknown) => {
        if (!cancelled) setError(err);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return { playlists, error };
}
