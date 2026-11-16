import type { Playlist } from '@/types/spotify';

/** Which part of the list a playlist falls in. The list shows them in this order. */
export type PlaylistGroup = 'recent' | 'mine' | 'others';

export const GROUP_LABELS: Record<PlaylistGroup, string> = {
  recent: 'Listened to lately',
  mine: 'Made by you',
  others: 'From other people',
};

export interface GroupedPlaylist {
  playlist: Playlist;
  group: PlaylistGroup;
}

// A to Z the way a person expects it: "apple" next to "Apple", and "Mix 2" before "Mix 10".
const byName = new Intl.Collator(undefined, { sensitivity: 'base', numeric: true });

/**
 * Whether the signed-in user made this playlist.
 *
 * The API only sends the owner's display name, so this compares names. That is good enough to
 * order a list, but it is not proof: a stranger can pick the same display name. When the API
 * sends `isMine` (decided on the server from the owner's id), that answer is used instead.
 */
export function isMine(playlist: Playlist, myDisplayName: string | undefined): boolean {
  if (typeof playlist.isMine === 'boolean') return playlist.isMine;
  return myDisplayName !== undefined && playlist.owner === myDisplayName;
}

/**
 * Puts playlists in the order the room shows them:
 *
 *   1. listened to lately, most recent first (only when the API says which those are)
 *   2. made by you, A to Z
 *   3. everyone else's, A to Z
 *
 * A playlist appears once, in the first group it qualifies for. Returns a new array.
 */
export function sortPlaylists(
  playlists: Playlist[],
  myDisplayName: string | undefined,
  recentIds: string[] = [],
): GroupedPlaylist[] {
  const recentRank = new Map(recentIds.map((id, position) => [id, position]));

  const recent: Playlist[] = [];
  const mine: Playlist[] = [];
  const others: Playlist[] = [];

  for (const playlist of playlists) {
    if (recentRank.has(playlist.id)) recent.push(playlist);
    else if (isMine(playlist, myDisplayName)) mine.push(playlist);
    else others.push(playlist);
  }

  recent.sort((a, b) => (recentRank.get(a.id) ?? 0) - (recentRank.get(b.id) ?? 0));
  mine.sort((a, b) => byName.compare(a.name, b.name));
  others.sort((a, b) => byName.compare(a.name, b.name));

  return [
    ...recent.map((playlist) => ({ playlist, group: 'recent' as const })),
    ...mine.map((playlist) => ({ playlist, group: 'mine' as const })),
    ...others.map((playlist) => ({ playlist, group: 'others' as const })),
  ];
}
