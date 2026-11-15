'use client';

import { useCallback, useSyncExternalStore } from 'react';

/**
 * Whether a CSS media query matches right now, kept up to date as the window changes.
 *
 * Use this only when the answer changes what the code *does* (which requests it makes). When
 * it only changes how something looks, a media query in the CSS is simpler and cannot disagree
 * with the server's first render.
 */
export default function useMediaQuery(query: string): boolean {
  const subscribe = useCallback(
    (onChange: () => void) => {
      const list = window.matchMedia(query);
      list.addEventListener('change', onChange);
      return () => list.removeEventListener('change', onChange);
    },
    [query],
  );

  return useSyncExternalStore(
    subscribe,
    () => window.matchMedia(query).matches,
    // On the server there is no window to measure, so assume "no" until the browser knows.
    () => false,
  );
}
