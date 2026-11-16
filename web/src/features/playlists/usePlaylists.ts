'use client';

import { useEffect, useMemo, useState } from 'react';
import { useSession } from '@/features/auth/session-provider';
import { features } from '@/lib/features';
import { getPlaylists, getRecentPlaylistIds } from '@/services/spotifyService';
import type { Playlist } from '@/types/spotify';
import { sortPlaylists, type GroupedPlaylist } from './sortPlaylists';

/**
 * The signed-in user's playlists, loaded once and already in display order (see sortPlaylists).
 * `playlists` is null until the answer arrives.
 *
 * `limit` is how many to ask the API for. The playlist room asks for the most the API allows,
 * because "yours first" is only true if every playlist was looked at; the home page needs two
 * and asks for far fewer.
 */
export default function usePlaylists(limit = 50) {
  const { session } = useSession();
  const [loaded, setLoaded] = useState<{ playlists: Playlist[]; recentIds: string[] } | null>(null);
  const [error, setError] = useState<unknown>(null);

  useEffect(() => {
    // Guards against setting state after the component has gone, e.g. on a quick navigation.
    let cancelled = false;

    const load = async () => {
      try {
        const [playlists, recentIds] = await Promise.all([
          getPlaylists(limit),
          // Skipped until its endpoint exists (lib/features.ts). And if it does exist but
          // fails, the list is still worth showing — just without the "lately" group.
          features.recentPlaylists ? getRecentPlaylistIds().catch(() => []) : [],
        ]);
        if (!cancelled) setLoaded({ playlists, recentIds });
      } catch (err) {
        if (!cancelled) setError(err);
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [limit]);

  const playlists = useMemo<GroupedPlaylist[] | null>(
    () => (loaded ? sortPlaylists(loaded.playlists, session?.displayName, loaded.recentIds) : null),
    [loaded, session?.displayName],
  );

  return { playlists, error };
}
