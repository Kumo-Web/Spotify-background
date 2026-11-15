'use client';

import { useEffect } from 'react';
import Link from 'next/link';
import StatusScreen from '@/components/status-screen';

/**
 * Route-level error boundary. Next renders this in place of a segment that threw during
 * rendering, so it is often the only place a failure is visible at all — it needs to show what
 * went wrong rather than swallow it.
 */
export default function ErrorBoundary({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <StatusScreen
      title="Something went wrong"
      message={error.message || 'No further detail was reported.'}
    >
      <button
        type="button"
        onClick={reset}
        className="rounded-full bg-accent px-6 py-3 font-semibold text-on-accent"
      >
        Try again
      </button>
      <Link href="/" className="text-sm text-muted underline hover:text-fg">
        Back to the turntable
      </Link>
      {/* Set for server-side errors, where the real message is kept out of the browser. */}
      {error.digest && <p className="w-full text-xs text-muted">Reference: {error.digest}</p>}
    </StatusScreen>
  );
}
