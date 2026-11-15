'use client';

import { Suspense, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import StatusScreen from '@/components/status-screen';
import LoginCard from '@/features/auth/login-card';
import { useSession } from '@/features/auth/session-provider';
import Turntable from '@/features/turntable/turntable';
import { sanitiseReturnPath } from '@/lib/security';
import { SITE_NAME } from '@/lib/site';

/**
 * Maps the error codes the API's OAuth callback can redirect back with.
 *
 * The code in the URL only ever selects one of these sentences. It is never printed itself, so
 * a crafted `?error=` cannot put its own text on the page.
 */
const ERROR_MESSAGES: Record<string, string> = {
  denied: 'You declined access on Spotify. Nothing was connected.',
  invalid_callback: 'Spotify sent an incomplete response. Please try again.',
  invalid_state: 'That login attempt expired or was already used. Please start again.',
  no_refresh_token: 'Spotify did not issue a refresh token, so the connection could not be kept.',
};

/**
 * useSearchParams() opts its whole subtree into client-side rendering, and Next refuses to
 * prerender a page that does so without a Suspense boundary — without this split the page
 * renders fine in dev but `next build` fails outright.
 */
export default function LoginPage() {
  return (
    <Suspense fallback={<StatusScreen title="Loading…" />}>
      <LoginScreen />
    </Suspense>
  );
}

/** The turntable switched off, with the log in card where the album sleeve will be. */
function LoginScreen() {
  const { session, loading, error, login } = useSession();
  const router = useRouter();
  const searchParams = useSearchParams();

  const callbackError = searchParams.get('error');
  // Where to go after login. Only ever a path on this site — see sanitiseReturnPath.
  const returnPath = sanitiseReturnPath(searchParams.get('next')) ?? '/';

  useEffect(() => {
    if (session?.authenticated) {
      router.replace(returnPath);
    }
  }, [session, router, returnPath]);

  const problem = callbackError
    ? (ERROR_MESSAGES[callbackError] ?? 'Login failed. Please try again.')
    : error;

  return (
    <main>
      <Turntable
        power="off"
        card={<LoginCard problem={problem} checking={loading} onLogin={() => login(returnPath)} />}
      >
        <p className="absolute inset-x-0 top-4 z-10 text-center font-display text-lg font-bold tracking-tight sm:top-8 sm:text-[22px]">
          {SITE_NAME}
        </p>
      </Turntable>
    </main>
  );
}
