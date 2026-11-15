'use client';

import useSignedOutRedirect from '@/features/auth/useSignedOutRedirect';
import { describeError } from '@/lib/api';
import { formatTrackCount } from '@/lib/format';
import { isSpotifyId, SPOTIFY_HOME_URL } from '@/lib/security';
import PlaylistRoom, { type RoomRecord } from './playlist-room';
import usePlaylists from './usePlaylists';

/** /playlists: the room with one record per playlist. Picking one opens that playlist. */
export default function PlaylistsShelf() {
  const { playlists, error } = usePlaylists();
  const signedOut = useSignedOutRedirect(error);

  const records: RoomRecord[] = (playlists ?? [])
    // The id becomes part of a URL on this site, so anything that is not a real id is dropped.
    .filter((playlist) => isSpotifyId(playlist.id))
    .map((playlist) => ({
      id: playlist.id,
      title: playlist.name,
      subtitle: formatTrackCount(playlist.trackCount),
      image: playlist.image,
      href: `/playlists/${playlist.id}`,
    }));

  let empty: React.ReactNode = 'Loading your playlists…';
  if (error && !signedOut) {
    empty = <span role="alert">{describeError(error)}</span>;
  } else if (playlists) {
    empty = 'No playlists yet. Make one in Spotify and it will be on this shelf.';
  }

  return (
    <PlaylistRoom
      heading="Your playlists"
      describe={(shown, total) =>
        total === 0 ? 'Pick a record or a row.' : `${shown} of ${total} playlists on the shelf. Pick a record or a row.`
      }
      records={records}
      empty={empty}
      footer={
        <a href={SPOTIFY_HOME_URL} target="_blank" rel="noopener noreferrer" className="text-muted underline">
          Open Spotify
        </a>
      }
    />
  );
}
