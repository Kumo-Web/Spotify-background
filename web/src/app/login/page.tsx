'use client';

import { useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import useSpotify from '@/hooks/useSpotify';

/** Maps the error codes the API's OAuth callback can redirect back with. */
const ERROR_MESSAGES: Record<string, string> = {
  denied: 'You declined access on Spotify. Nothing was connected.',
  invalid_callback: 'Spotify sent an incomplete response. Please try again.',
  invalid_state: 'That login attempt expired or was already used. Please start again.',
  no_refresh_token: 'Spotify did not issue a refresh token, so the connection could not be kept.',
};

export default function LoginPage() {
  const { session, loading, login } = useSpotify();
  const router = useRouter();
  const error = useSearchParams().get('error');

  useEffect(() => {
    if (session?.authenticated) {
      router.replace('/dashboard');
    }
  }, [session, router]);

  return (
    <main className="mx-auto flex min-h-[60vh] max-w-md flex-col justify-center gap-6 px-4">
      <div>
        <h1 className="mb-2">Connect Spotify</h1>
        <p className="text-sm opacity-70">
          Sign in with Spotify to build playlists from your listening history.
        </p>
      </div>

      {error && (
        <p role="alert" className="rounded border border-red-500/40 bg-red-500/10 p-3 text-sm">
          {ERROR_MESSAGES[error] ?? 'Login failed. Please try again.'}
        </p>
      )}

      <button
        type="button"
        onClick={() => login('/dashboard')}
        disabled={loading}
        className="rounded-full bg-[#1DB954] px-6 py-3 font-medium text-black transition hover:brightness-110 disabled:opacity-50"
      >
        {loading ? 'Checking session…' : 'Continue with Spotify'}
      </button>
    </main>
  );
}
