'use client';

import { useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import StatusScreen from '@/components/status-screen';
import { useSession } from './session-provider';

/**
 * Sends a signed-out visitor to the log in page.
 *
 * This is a convenience, not the security boundary. Anything running in the browser can be
 * skipped by whoever controls the browser, so it must never be the thing that protects data.
 * The API is: every endpoint is deny-by-default and answers 401 without a valid session cookie.
 * All this component does is stop a signed-out visitor from looking at an empty page.
 */
export default function RequireSession({ children }: { children: React.ReactNode }) {
  const { session, loading, error, refresh } = useSession();
  const router = useRouter();
  const pathname = usePathname();

  const signedIn = session?.authenticated === true;

  useEffect(() => {
    if (loading || signedIn || error) return;
    router.replace(`/login?next=${encodeURIComponent(pathname)}`);
  }, [loading, signedIn, error, pathname, router]);

  // Shown instead of redirecting: with the API down, /login could not do anything either, and
  // bouncing there would hide the one message that says what is actually wrong.
  if (error) {
    return (
      <StatusScreen title="Can't reach the API" message={error}>
        <button
          type="button"
          onClick={() => void refresh()}
          className="rounded-full bg-accent px-6 py-3 font-semibold text-on-accent"
        >
          Try again
        </button>
      </StatusScreen>
    );
  }

  if (!signedIn) {
    return <StatusScreen title="Checking your session…" />;
  }

  return <>{children}</>;
}
