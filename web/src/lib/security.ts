/**
 * Small checks on values that arrive from outside this code: the URL bar, and Spotify's data.
 *
 * None of these replace the API's own checks. They exist so that a bad value is dropped here
 * as well, instead of being trusted because "the server probably validated it".
 */

/** The hosts Spotify serves artwork from. Keep in step with `img-src` in middleware.ts. */
const IMAGE_HOST_SUFFIXES = ['.scdn.co', '.spotifycdn.com'];

/**
 * Returns the URL only if it is HTTPS and points at Spotify's image CDN, otherwise null.
 *
 * Image URLs come out of Spotify's API, and parts of that data (playlist covers, for one) are
 * uploaded by other users. Rendering whatever arrives would let a crafted value make this
 * browser request an attacker's server, which leaks the visitor's IP address at the least.
 */
export function safeImageUrl(value: string | null | undefined): string | null {
  if (!value) return null;

  let url: URL;
  try {
    url = new URL(value);
  } catch {
    return null;
  }

  if (url.protocol !== 'https:') return null;

  // The leading dot matters: "evilscdn.co" must not pass as "scdn.co".
  const host = url.hostname.toLowerCase();
  return IMAGE_HOST_SUFFIXES.some((suffix) => host.endsWith(suffix)) ? url.toString() : null;
}

/**
 * Keeps only a path on this site, so `?next=` on the log in page cannot send the browser to
 * another host after login. The API sanitises its own copy too (see AuthController); this stops
 * a bad value before it is ever sent.
 */
export function sanitiseReturnPath(value: string | null | undefined): string | undefined {
  if (!value) return undefined;

  const isRooted = value.startsWith('/');
  // "//host" is scheme-relative, and browsers read "\" as "/", so "/\host" is the same trick.
  const leavesSite = value.startsWith('//') || value.includes('\\');
  const hasControlCharacters = /[\u0000-\u001f\u007f]/.test(value);

  return isRooted && !leavesSite && !hasControlCharacters ? value : undefined;
}

const SPOTIFY_ID = /^[A-Za-z0-9]{1,64}$/;

/** Spotify ids are plain base-62 text. Anything else in a route parameter is not an id. */
export function isSpotifyId(value: unknown): value is string {
  return typeof value === 'string' && SPOTIFY_ID.test(value);
}

/** A link back to Spotify for a track or playlist, or null when the id is not a real one. */
export function spotifyUrl(kind: 'track' | 'playlist', id: string): string | null {
  return isSpotifyId(id) ? `https://open.spotify.com/${kind}/${id}` : null;
}

export const SPOTIFY_HOME_URL = 'https://open.spotify.com/';
