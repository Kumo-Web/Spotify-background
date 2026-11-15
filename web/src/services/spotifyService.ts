import { apiFetch } from '../lib/api';
import type {
  Artist,
  NowPlaying,
  Playlist,
  PlaylistDetail,
  PlaylistTracksPage,
  Profile,
  Session,
  Track,
} from '../types/spotify';

/**
 * None of these take a user id. Identity travels in the session cookie, so the server decides
 * who "me" is — the client cannot ask for someone else's data.
 */

export const getSession = () => apiFetch<Session>('/api/auth/session');

export const logout = () => apiFetch<void>('/api/auth/logout', { method: 'POST' });

export const getProfile = () => apiFetch<Profile>('/api/me');

export const getTopArtists = (range = 'medium_term', limit = 20) =>
  apiFetch<Artist[]>(`/api/me/top-artists?range=${range}&limit=${limit}`);

export const getTopTracks = (range = 'medium_term', limit = 20) =>
  apiFetch<Track[]>(`/api/me/top-tracks?range=${range}&limit=${limit}`);

export const getNowPlaying = () => apiFetch<NowPlaying>('/api/me/now-playing');

export const getPlaylists = (limit = 50) => apiFetch<Playlist[]>(`/api/playlists?limit=${limit}`);

export const getPlaylist = (playlistId: string) =>
  apiFetch<PlaylistDetail>(`/api/playlists/${encodeURIComponent(playlistId)}`);

/** Unfollows the playlist. Spotify has no delete; this is what "delete" means there. */
export const removePlaylist = (playlistId: string) =>
  apiFetch<void>(`/api/playlists/${encodeURIComponent(playlistId)}`, { method: 'DELETE' });

// ---------------------------------------------------------------------------------------------
// Not in the API yet. Nothing calls these while their flag in lib/features.ts is off, so a
// missing endpoint cannot surface as a broken button. They are written now so that the contract
// the front end expects is in one place when the endpoints are built.
// ---------------------------------------------------------------------------------------------

export const getPlaylistTracks = (playlistId: string, limit = 30, offset = 0) =>
  apiFetch<PlaylistTracksPage>(
    `/api/playlists/${encodeURIComponent(playlistId)}/tracks?limit=${limit}&offset=${offset}`,
  );

export const resumePlayback = () => apiFetch<void>('/api/me/player/play', { method: 'PUT' });

export const pausePlayback = () => apiFetch<void>('/api/me/player/pause', { method: 'PUT' });

export const skipToNext = () => apiFetch<void>('/api/me/player/next', { method: 'POST' });

export const skipToPrevious = () => apiFetch<void>('/api/me/player/previous', { method: 'POST' });
