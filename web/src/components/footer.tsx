/**
 * Says plainly that this site is not Spotify's own.
 *
 * The pages are full-screen scenes with no room for a classic footer, so this is a single line
 * that the menu and the log in card each place where it fits.
 */
export default function Footer({ className = '' }: { className?: string }) {
  return <p className={`text-xs text-muted ${className}`}>Not affiliated with Spotify AB.</p>;
}
