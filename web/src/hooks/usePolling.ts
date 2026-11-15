'use client';

import { useEffect, useRef } from 'react';

/**
 * Runs `callback` straight away, then every `intervalMs` — but only while the tab is visible.
 *
 * Stopping when hidden is deliberate. Every poll is a request to the API and, behind it, to
 * Spotify, and both are rate limited. A forgotten background tab should not spend that budget.
 */
export default function usePolling(callback: () => void, intervalMs: number, enabled = true) {
  // Kept in a ref so a new callback identity does not restart the timer on every render.
  const latest = useRef(callback);

  useEffect(() => {
    latest.current = callback;
  }, [callback]);

  useEffect(() => {
    if (!enabled) return;

    let timer: ReturnType<typeof setInterval> | undefined;

    const start = () => {
      if (timer) return;
      latest.current();
      timer = setInterval(() => latest.current(), intervalMs);
    };

    const stop = () => {
      clearInterval(timer);
      timer = undefined;
    };

    const onVisibilityChange = () => (document.hidden ? stop() : start());

    onVisibilityChange();
    document.addEventListener('visibilitychange', onVisibilityChange);

    return () => {
      stop();
      document.removeEventListener('visibilitychange', onVisibilityChange);
    };
  }, [intervalMs, enabled]);
}
