import { safeImageUrl } from '@/lib/security';

interface SpotifyImageProps {
  src: string | null | undefined;
  alt: string;
  className?: string;
}

/**
 * The only place artwork from Spotify is put on the page.
 *
 * Renders nothing unless the URL passes safeImageUrl, so a value that is not on Spotify's image
 * CDN never becomes a request. `referrerPolicy` keeps this site's address out of the request
 * that does go to Spotify.
 *
 * A plain <img> rather than next/image, on purpose. next/image would fetch the picture through
 * this server and resize it, which turns the server into something that makes outbound requests
 * on a visitor's behalf — more to defend, for artwork Spotify already serves at sensible sizes.
 */
export default function SpotifyImage({ src, alt, className }: SpotifyImageProps) {
  const url = safeImageUrl(src);

  if (!url) return null;

  return (
    // eslint-disable-next-line @next/next/no-img-element -- see the note above
    <img
      src={url}
      alt={alt}
      className={className}
      loading="lazy"
      decoding="async"
      referrerPolicy="no-referrer"
      draggable={false}
    />
  );
}
