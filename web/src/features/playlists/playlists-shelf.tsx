'use client';

import useSignedOutRedirect from '@/features/auth/useSignedOutRedirect';
import { describeError } from '@/lib/api';
import { formatTrackCount } from '@/lib/format';
import { isSpotifyId, SPOTIFY_HOME_URL } from '@/lib/security';
import PlaylistRoom, { type RoomRecord } from './playlist-room';
import { GROUP_LABELS } from './sortPlaylists';
import usePlaylists from './usePlaylists';

/** The most the API hands over in one go. Ordering is only right if all of them are fetched. */
const ALL_PLAYLISTS = 200;

/**
 * /playlists: the room with one record per playlist. Picking one opens that playlist.
 *
 * The order comes from usePlaylists: yours A to Z, then everyone else's. The shelf and the list
 * share it, so the first record on the shelf is the first row in the list.
 */
export default function PlaylistsShelf() {
  const { playlists, error } = usePlaylists(ALL_PLAYLISTS);
  const signedOut = useSignedOutRedirect(error);

  const records: RoomRecord[] = (playlists ?? [])
    // The id becomes part of a URL on this site, so anything that is not a real id is dropped.
    .filter(({ playlist }) => isSpotifyId(playlist.id))
    .map(({ playlist, group }) => ({
      id: playlist.id,
      title: playlist.name,
      subtitle: formatTrackCount(playlist.trackCount),
      image: playlist.image,
      href: `/playlists/${playlist.id}`,
      group: GROUP_LABELS[group],
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
