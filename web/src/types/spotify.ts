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

export type NowPlaying =
  | { isPlaying: false }
  | {
      isPlaying: boolean;
      trackId: string;
      title: string;
      artists: string[];
      album: string;
      albumArt?: string | null;
      progressMs: number | null;
      durationMs: number;
    };
