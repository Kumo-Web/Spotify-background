/**
 * Switches for parts of the UI whose API endpoints do not exist yet.
 *
 * Each one stays off until its endpoint is written, so the page never shows a control that
 * cannot work. Turning a flag on is the only front-end change needed afterwards — the calls
 * themselves are already in services/spotifyService.ts.
 */
export const features = {
  /**
   * Play, pause and skip from the turntable.
   *
   * Needs PUT /api/me/player/play, PUT /api/me/player/pause, POST /api/me/player/next and
   * POST /api/me/player/previous, and the `user-modify-playback-state` scope in
   * SpotifySettings.Scopes (existing sessions must log in again to be granted it).
   */
  playbackControls: false,

  /**
   * The records inside one playlist.
   *
   * Needs GET /api/playlists/{playlistId}/tracks?limit=&offset= returning
   * `{ total, items: [{ id, name, artists, album, albumArt, durationMs }] }`.
   */
  playlistTracks: false,
} as const;
