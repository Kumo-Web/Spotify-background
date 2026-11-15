'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import useSignedOutRedirect from '@/features/auth/useSignedOutRedirect';
import { ApiError, describeError } from '@/lib/api';
import { features } from '@/lib/features';
import { isSpotifyId, spotifyUrl } from '@/lib/security';
import { getPlaylist, getPlaylistTracks } from '@/services/spotifyService';
import type { PlaylistDetail, PlaylistTracksPage } from '@/types/spotify';
import PlaylistRoom, { type RoomRecord } from './playlist-room';
import { SHELF_CAPACITY } from './shelf';

/**
 * /playlists/[playlistId]: the room with one record per track — a bunch, not every song.
 *
 * Only the first shelf-full is requested. A playlist can run to thousands of tracks, and the
 * rest are one click away in Spotify itself.
 */
export default function PlaylistTracks({ playlistId }: { playlistId: string }) {
  const [playlist, setPlaylist] = useState<PlaylistDetail | null>(null);
  const [tracks, setTracks] = useState<PlaylistTracksPage | null>(null);
  const [error, setError] = useState<unknown>(null);
  const signedOut = useSignedOutRedirect(error);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      try {
        // The track request is skipped entirely until its endpoint exists (lib/features.ts).
        const [detail, page] = await Promise.all([
          getPlaylist(playlistId),
          features.playlistTracks ? getPlaylistTracks(playlistId, SHELF_CAPACITY) : null,
        ]);
        if (cancelled) return;
        setPlaylist(detail);
        setTracks(page);
      } catch (err) {
        if (!cancelled) setError(err);
      }
    };

    void load();

    return () => {
      cancelled = true;
    };
  }, [playlistId]);

  const records: RoomRecord[] = (tracks?.items ?? [])
    // Local files and removed tracks come back without a usable id, so they cannot be linked.
    .filter((track) => isSpotifyId(track.id))
    .map((track) => ({
      id: track.id,
      title: track.name,
      subtitle: track.artists.join(', '),
      image: track.albumArt,
      href: spotifyUrl('track', track.id) ?? '#',
      external: true,
    }));

  const total = tracks?.total ?? playlist?.trackCount ?? 0;
  const playlistUrl = spotifyUrl('playlist', playlistId);

  let empty: React.ReactNode = 'Loading this playlist…';
  if (error && !signedOut) {
    // Spotify only shares the tracks of playlists the user made or helps edit. The tracks
    // endpoint is expected to answer 403 or 404 for anyone else's, and that case deserves a
    // plainer sentence than the default.
    const blocked = error instanceof ApiError && (error.status === 403 || error.status === 404);
    empty = (
      <span role="alert">
        {blocked
          ? "Can't open this playlist. Spotify only shares tracks from playlists you made or help edit."
          : describeError(error)}
      </span>
    );
  } else if (playlist && !features.playlistTracks) {
    empty = 'This shelf is empty for now. The API does not list the tracks inside a playlist yet.';
  } else if (playlist) {
    empty = 'This playlist has no tracks yet.';
  }

  return (
    <PlaylistRoom
      heading={playlist?.name ?? 'Playlist'}
      describe={(shown) =>
        shown === 0 ? `${total} tracks in this playlist.` : `${shown} of ${total} tracks on the shelf. Pick a record or a row.`
      }
      records={records}
      empty={empty}
      footer={
        <>
          <Link href="/playlists" className="font-semibold underline">
            All playlists
          </Link>
          {playlistUrl && (
            <a href={playlistUrl} target="_blank" rel="noopener noreferrer" className="text-muted underline">
              Open in Spotify
            </a>
          )}
        </>
      }
    />
  );
}
