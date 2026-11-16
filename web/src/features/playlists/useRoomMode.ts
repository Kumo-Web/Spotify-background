'use client';

import { useEffect, useSyncExternalStore } from 'react';
import type { RoomMode } from './shelf';

const STORAGE_KEY = 'room-mode';
const PREFERS_LIGHT = '(prefers-color-scheme: light)';

// The choice made in this tab. Kept in memory as well as in storage, because storage can be
// unavailable (private windows, blocked site data) and the switch must still work then.
let chosen: RoomMode | null = null;
const listeners = new Set<() => void>();

function read(): RoomMode {
  if (chosen) return chosen;

  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    // Storage is editable by anyone with the browser open, so only the two known values count.
    if (stored === 'day' || stored === 'night') return stored;
  } catch {
    // Fall through to the system setting.
  }

  // No choice made yet: follow the device. Light theme means daytime in the room.
  return window.matchMedia(PREFERS_LIGHT).matches ? 'day' : 'night';
}

function subscribe(onChange: () => void) {
  const system = window.matchMedia(PREFERS_LIGHT);

  listeners.add(onChange);
  system.addEventListener('change', onChange);
  // Fires when another tab changes the choice, so two open tabs stay in step.
  window.addEventListener('storage', onChange);

  return () => {
    listeners.delete(onChange);
    system.removeEventListener('change', onChange);
    window.removeEventListener('storage', onChange);
  };
}

function choose(mode: RoomMode) {
  chosen = mode;

  try {
    window.localStorage.setItem(STORAGE_KEY, mode);
  } catch {
    // Not saved for next time; the in-memory choice still applies now.
  }

  listeners.forEach((notify) => notify());
}

/**
 * Day or night in the playlist room, remembered between visits.
 *
 * It also flips the page's colours while a playlist page is open, by setting `data-theme` on
 * <html> (the light values are in globals.css). Daylight needs dark text: the header floats
 * over the room, and pale text on a sunlit wall cannot be read. The attribute is removed on
 * leaving, so the turntable pages keep their one dark look.
 */
export default function useRoomMode(): [RoomMode, (mode: RoomMode) => void] {
  // The server cannot know the visitor's choice, so it renders night and the browser corrects
  // it straight after. useSyncExternalStore exists for exactly this kind of browser-only value.
  const mode = useSyncExternalStore(subscribe, read, () => 'night' as const);

  useEffect(() => {
    const root = document.documentElement;

    if (mode === 'day') root.dataset.theme = 'day';
    else delete root.dataset.theme;

    return () => {
      delete root.dataset.theme;
    };
  }, [mode]);

  return [mode, choose];
}
