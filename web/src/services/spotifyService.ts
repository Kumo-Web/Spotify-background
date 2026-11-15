import { apiFetch } from '../lib/api';
import type { Artist, NowPlaying, Playlist, Profile, Session, Track } from '../types/spotify';

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

/** Unfollows the playlist. Spotify has no delete; this is what "delete" means there. */
export const removePlaylist = (playlistId: string) =>
  apiFetch<void>(`/api/playlists/${encodeURIComponent(playlistId)}`, { method: 'DELETE' });
