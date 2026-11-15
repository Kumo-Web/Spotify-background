'use client';

import Link from 'next/link';
import useSpotify from '@/hooks/useSpotify';

/**
 * Landing page. Deliberately plain — this is scaffolding to prove the round trip works, not the
 * real design.
 */
export default function Home() {
  const { session, loading, error, login } = useSpotify();

  return (
    <main className="mx-auto flex min-h-[60vh] max-w-2xl flex-col justify-center gap-6 px-4">
      <div>
        <h1 className="text-3xl font-semibold tracking-tight">
          Playlists from your listening history
        </h1>
        <p className="mt-3 text-sm opacity-70">
          Connect Spotify once. Everything after that happens server-side — the access token
          never reaches this browser.
        </p>
      </div>

      {error && (
        <p role="alert" className="rounded border border-amber-500/40 bg-amber-500/10 p-3 text-sm">
          {error}
        </p>
      )}

      {loading ? (
        <p className="text-sm opacity-60">Checking session…</p>
      ) : session?.authenticated ? (
        <div className="flex items-center gap-4">
          <Link
            href="/dashboard"
            className="rounded-full bg-[#1DB954] px-6 py-3 font-medium text-black transition hover:brightness-110"
          >
            Go to dashboard
          </Link>
          <span className="text-sm opacity-60">
            Signed in{session.displayName ? ` as ${session.displayName}` : ''}.
          </span>
        </div>
      ) : (
        <button
          type="button"
          onClick={() => login('/dashboard')}
          className="self-start rounded-full bg-[#1DB954] px-6 py-3 font-medium text-black transition hover:brightness-110"
        >
          Continue with Spotify
        </button>
      )}
    </main>
  );
}
