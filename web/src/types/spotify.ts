export interface Session {
  authenticated: boolean;
  displayName?: string;
}

export interface Profile {
  id: string;
  displayName: string | null;
  images?: { url: string; height: number | null; width: number | null }[];
  followers?: number | null;
}

export interface Artist {
  id: string;
  name: string;
  genres: string[];
  image?: string | null;
}

export interface Track {
  id: string;
  name: string;
  artists: string[];
  album: string;
  durationMs: number;
}

export interface Playlist {
  id: string;
  name: string;
  description: string | null;
  trackCount: number;
  image?: string | null;
  isPublic: boolean | null;
  owner: string | null;
}

/** What GET /api/playlists/{id} returns — a subset of Playlist. */
export type PlaylistDetail = Pick<Playlist, 'id' | 'name' | 'description' | 'trackCount' | 'image'>;

/** One record on a playlist's shelf. Not served yet — see features.playlistTracks. */
export interface PlaylistTrack {
  id: string;
  name: string;
  artists: string[];
  album: string;
  albumArt?: string | null;
  durationMs: number;
}

export interface PlaylistTracksPage {
  total: number;
  items: PlaylistTrack[];
}

export interface PlayingTrack {
  isPlaying: boolean;
  trackId: string;
  title: string;
  artists: string[];
  album: string;
  albumArt?: string | null;
  progressMs: number | null;
  durationMs: number;
  /**
   * Not sent by the API yet. The turntable already uses them when present: the artist photo
   * goes in the centre of the record, the device name in the pill at the top.
   */
  artistImage?: string | null;
  deviceName?: string | null;
}

export type NowPlaying = { isPlaying: false } | PlayingTrack;

/**
 * Narrows NowPlaying to the case that carries a track.
 *
 * `isPlaying` cannot do this on its own: a paused track is still `isPlaying: false`, so the
 * presence of the track itself is the only reliable test.
 */
export function hasTrack(nowPlaying: NowPlaying | null): nowPlaying is PlayingTrack {
  return nowPlaying !== null && 'trackId' in nowPlaying;
}
