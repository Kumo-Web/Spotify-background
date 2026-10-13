'use client';

import { useEffect } from 'react';
import Link from 'next/link';

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
    <main className="mx-auto flex min-h-[60vh] max-w-md flex-col justify-center gap-4 px-4">
      <h1 className="text-xl font-semibold">Something went wrong</h1>

      <p role="alert" className="rounded border border-red-500/40 bg-red-500/10 p-3 text-sm">
        {error.message || 'No further detail was reported.'}
      </p>

      {/* Set for server-side errors, where the real message is kept out of the browser. */}
      {error.digest && <p className="text-xs opacity-50">Reference: {error.digest}</p>}

      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={reset}
          className="rounded-full bg-[#1DB954] px-5 py-2 text-sm font-medium text-black transition hover:brightness-110"
        >
          Try again
        </button>
        <Link href="/" className="text-sm underline opacity-70 hover:opacity-100">
          Back to start
        </Link>
      </div>
    </main>
  );
}
