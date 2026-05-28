export interface Track {
  id: string;
  title: string;
  durationMs: number;
}

export interface Album {
  id: string;
  title: string;
  artist: string;
  coverUrl: string;
  year: number;
  accentColor: string;
  tracks: Track[];
}

export type Vinyl = Album;

export interface Playlist {
  id: string;
  name: string;
  description: string;
  albumIds: string[];
}

export interface CreatePlaylistInput {
  name: string;
  description: string;
  albumIds: string[];
}
