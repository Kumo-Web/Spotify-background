/**
 * Facts about the shelf that both the 3D room and the plain list need.
 *
 * They live here, not in room-scene.ts, for a practical reason: room-scene.ts imports three.js,
 * and anything that imports from it pulls three.js into the same download. Keeping these apart
 * lets the list render straight away while the 3D loads on its own afterwards.
 */

/** Four piles, as in the sketch. */
export const PILES = [8, 8, 7, 7] as const;

/** How many records fit on the shelf at once. */
export const SHELF_CAPACITY = PILES.reduce((sum, count) => sum + count, 0);

export const COVER_COLOURS = ['#7a3b2e', '#2f5d62', '#5b3a6b', '#7a5c22', '#2e4a7a', '#6b2e45', '#3e6b4a'];

/** The same id always gets the same colour, so a record looks the same on every visit. */
export function colourFor(id: string): string {
  let hash = 0;
  for (let i = 0; i < id.length; i++) {
    hash = (hash * 31 + id.charCodeAt(i)) >>> 0;
  }
  return COVER_COLOURS[hash % COVER_COLOURS.length];
}

/** day: sun outside, lamp off, a light room. night: moon outside, lamp on, a dim one. */
export type RoomMode = 'day' | 'night';
