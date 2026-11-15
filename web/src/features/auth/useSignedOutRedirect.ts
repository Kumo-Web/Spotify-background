'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { ApiError } from '@/lib/api';

/**
 * Sends the visitor to the log in page when a request came back 401.
 *
 * A session can end while a page is open — it expires, or is signed out in another tab. The
 * next request then fails, and the useful response is the log in page, not an error message.
 * Returns true in that case so the caller can skip drawing the error.
 */
export default function useSignedOutRedirect(error: unknown): boolean {
  const router = useRouter();
  const signedOut = error instanceof ApiError && error.isUnauthenticated;

  useEffect(() => {
    if (signedOut) router.replace('/login');
  }, [signedOut, router]);

  return signedOut;
}
