'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import useSpotify from '@/hooks/useSpotify';
import { getNowPlaying, getPlaylists, getProfile } from '@/services/spotifyService';
import { ApiError } from '@/lib/api';
import type { NowPlaying, Playlist, Profile } from '@/types/spotify';

export default function DashboardPage() {
  const { session, loading: sessionLoading, logout } = useSpotify();
  const router = useRouter();

  const [profile, setProfile] = useState<Profile | null>(null);
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [nowPlaying, setNowPlaying] = useState<NowPlaying | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!sessionLoading && !session?.authenticated) {
      router.replace('/login');
    }
  }, [session, sessionLoading, router]);

  useEffect(() => {
    if (!session?.authenticated) return;

    let cancelled = false;

    (async () => {
      try {
        const [me, lists, playing] = await Promise.all([
          getProfile(),
          getPlaylists(),
          getNowPlaying(),
        ]);

        if (cancelled) return;
        setProfile(me);
        setPlaylists(lists);
        setNowPlaying(playing);
      } catch (err) {
        if (cancelled) return;

        // An expired session is a redirect, not an error message.
        if (err instanceof ApiError && err.isUnauthenticated) {
          router.replace('/login');
          return;
        }

        setError(err instanceof Error ? err.message : 'Something went wrong.');
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [session, router]);

  if (sessionLoading) {
    return <main className="p-8 text-sm opacity-70">Loading…</main>;
  }

  return (
    <main className="mx-auto max-w-3xl px-4 py-8">
      <header className="mb-8 flex flex-wrap items-center justify-between gap-4">
        <h1>{profile?.displayName ?? session?.displayName ?? 'Your library'}</h1>
        <button type="button" onClick={logout} className="text-sm underline opacity-70 hover:opacity-100">
          Sign out
        </button>
      </header>

      {error && (
        <p role="alert" className="mb-6 rounded border border-red-500/40 bg-red-500/10 p-3 text-sm">
          {error}
        </p>
      )}

      <section className="mb-8">
        <h2 className="mb-2">Now playing</h2>
        {nowPlaying?.isPlaying ? (
          <p className="text-sm">
            {nowPlaying.title} — {nowPlaying.artists.join(', ')}
          </p>
        ) : (
          <p className="text-sm opacity-60">Nothing playing.</p>
        )}
      </section>

      <section>
        <h2 className="mb-2">Playlists ({playlists.length})</h2>
        <ul className="space-y-1 text-sm">
          {playlists.map((playlist) => (
            <li key={playlist.id} className="flex justify-between gap-4">
              <span>{playlist.name}</span>
              <span className="opacity-60">{playlist.trackCount} tracks</span>
            </li>
          ))}
        </ul>
      </section>
    </main>
  );
}
