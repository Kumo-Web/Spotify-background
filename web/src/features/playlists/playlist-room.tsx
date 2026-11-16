'use client';

import { useEffect, useMemo, useRef, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import SpotifyImage from '@/components/spotify-image';
import styles from './playlist-room.module.css';
import type { RoomScene } from './room-scene';
import { SHELF_CAPACITY, colourFor, type RoomMode } from './shelf';
import useRoomMode from './useRoomMode';

/** One record on the shelf, and the matching row in the list. */
export interface RoomRecord {
  id: string;
  title: string;
  subtitle?: string;
  image?: string | null;
  href: string;
  /** True for links that leave this site (to Spotify); they open in a new tab. */
  external?: boolean;
  /** A heading for the part of the list this record belongs to, e.g. "Made by you". */
  group?: string;
}

interface PlaylistRoomProps {
  heading: string;
  /** What this shelf is, given the two numbers: how many are showing, and how many exist. */
  describe: (shown: number, total: number) => string;
  records: RoomRecord[];
  /** Shown in place of the list when there are no records. */
  empty: React.ReactNode;
  /** Extra links under the list. */
  footer?: React.ReactNode;
}

/**
 * The playlist room: the 3D shelf with a plain list beside it.
 *
 * The list is the real interface. It is ordinary links, so it works with a keyboard, a screen
 * reader, and in a browser with no WebGL at all. The 3D is drawn on top of that as decoration
 * you can also click — which is why the canvas is hidden from assistive technology and every
 * record on the shelf has a twin in the list.
 *
 * The shelf holds 30. A longer list is shown 30 at a time rather than as 200 spines.
 */
export default function PlaylistRoom({ heading, describe, records, empty, footer }: PlaylistRoomProps) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const sceneRef = useRef<RoomScene | null>(null);
  const router = useRouter();

  const [mode, setMode] = useRoomMode();
  const [page, setPage] = useState(0);
  const [hot, setHot] = useState(-1);
  const [tip, setTip] = useState<{ x: number; y: number } | null>(null);
  const [sceneReady, setSceneReady] = useState(false);
  const [sceneFailed, setSceneFailed] = useState(false);

  const pageCount = Math.max(1, Math.ceil(records.length / SHELF_CAPACITY));
  // The list can shrink under us (a reload returns fewer), so never trust the stored page.
  const currentPage = Math.min(page, pageCount - 1);
  const onShelf = useMemo(
    () => records.slice(currentPage * SHELF_CAPACITY, (currentPage + 1) * SHELF_CAPACITY),
    [records, currentPage],
  );

  // The scene is created once but its click handler must see today's records, not the ones
  // that existed when it was created. A ref holds the latest without recreating the scene.
  const pickRef = useRef<(index: number) => void>(() => {});
  useEffect(() => {
    pickRef.current = (index) => {
      const record = onShelf[index];
      if (!record) return;

      if (record.external) {
        // noopener: the page we open must not get a handle back to this window.
        window.open(record.href, '_blank', 'noopener,noreferrer');
      } else {
        router.push(record.href);
      }
    };
  }, [onShelf, router]);

  // Same reason as pickRef: the scene starts in whatever mode is current when it finishes
  // loading, which may not be the mode from when this component first rendered.
  const modeRef = useRef<RoomMode>(mode);
  useEffect(() => {
    modeRef.current = mode;
    sceneRef.current?.setMode(mode);
  }, [mode]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    let cancelled = false;
    let scene: RoomScene | null = null;

    // Loaded here, not imported at the top, so three.js is only downloaded by someone who
    // actually opens a playlist page. The home and log in pages never pay for it.
    import('./room-scene')
      .then(({ createRoomScene }) => {
        if (cancelled) return;

        scene = createRoomScene(canvas, {
          mode: modeRef.current,
          reducedMotion: window.matchMedia('(prefers-reduced-motion: reduce)').matches,
          onHover: (index, point) => {
            setHot(index);
            setTip(point);
          },
          onPick: (index) => pickRef.current(index),
        });
        sceneRef.current = scene;
        setSceneReady(true);
      })
      .catch(() => {
        // No WebGL, or the download failed. The list still works, so say so and carry on.
        if (!cancelled) setSceneFailed(true);
      });

    return () => {
      cancelled = true;
      scene?.dispose();
      sceneRef.current = null;
    };
  }, []);

  useEffect(() => {
    if (sceneReady) sceneRef.current?.setRecords(onShelf.map((record) => record.id));
  }, [onShelf, sceneReady]);

  useEffect(() => {
    sceneRef.current?.setHot(hot);
  }, [hot]);

  const hotRecord = hot >= 0 ? onShelf[hot] : undefined;

  // Rows that share a group heading, kept together under it. Each row remembers its position
  // on the shelf, because that number is what links a row to its record in the 3D scene.
  const sections = useMemo(() => {
    const result: { heading?: string; rows: { record: RoomRecord; index: number }[] }[] = [];

    onShelf.forEach((record, index) => {
      const current = result[result.length - 1];
      if (current && current.heading === record.group) current.rows.push({ record, index });
      else result.push({ heading: record.group, rows: [{ record, index }] });
    });

    return result;
  }, [onShelf]);

  return (
    <main className={styles.room}>
      <canvas ref={canvasRef} aria-hidden="true" className={styles.canvas} />

      {sceneFailed && (
        <p className={`${styles.notice} rounded-xl border border-line bg-surface p-3 text-sm text-muted`}>
          The 3D room needs WebGL, which this browser did not provide. The list still works.
        </p>
      )}

      <div className={styles.panelArea}>
        <section
          aria-labelledby="room-heading"
          className={`${styles.panel} rounded-2xl border border-line bg-surface`}
        >
          <div className="border-b border-line p-4">
            <div className="flex items-start justify-between gap-3">
              <h1 id="room-heading" className="min-w-0 truncate font-display text-xl font-bold tracking-tight">
                {heading}
              </h1>
              <ModeSwitch mode={mode} onChange={setMode} />
            </div>
            <p className="mt-1 text-xs text-muted">{describe(onShelf.length, records.length)}</p>
          </div>

          {onShelf.length === 0 ? (
            <div className="p-4 text-sm leading-relaxed text-muted">{empty}</div>
          ) : (
            <div className="min-h-0 flex-1 overflow-auto p-1.5">
              {sections.map((section, sectionIndex) => (
                <section key={`${section.heading ?? 'all'}-${sectionIndex}`}>
                  {section.heading && (
                    <h2 className="px-2.5 pb-1 pt-3 text-[11px] font-semibold uppercase tracking-[0.08em] text-muted">
                      {section.heading}
                    </h2>
                  )}
                  <ol>
                    {section.rows.map(({ record, index }) => (
                      // A playlist can hold the same track twice, so the id alone is not a safe key.
                      <li key={`${record.id}-${index}`}>
                        <RecordLink
                          record={record}
                          hot={index === hot}
                          onHot={(isHot) => {
                            setHot(isHot ? index : -1);
                            setTip(null);
                          }}
                        />
                      </li>
                    ))}
                  </ol>
                </section>
              ))}
            </div>
          )}

          {(pageCount > 1 || footer) && (
            <div className="flex flex-wrap items-center gap-x-4 gap-y-2 border-t border-line px-4 py-3 text-sm">
              {currentPage > 0 && (
                <button type="button" onClick={() => setPage(currentPage - 1)} className="font-semibold underline">
                  Show previous {SHELF_CAPACITY}
                </button>
              )}
              {currentPage < pageCount - 1 && (
                <button type="button" onClick={() => setPage(currentPage + 1)} className="font-semibold underline">
                  Show next {SHELF_CAPACITY}
                </button>
              )}
              {footer}
            </div>
          )}
        </section>
      </div>

      {tip && hotRecord && (
        <div
          role="presentation"
          className="pointer-events-none fixed z-40 -translate-y-1/2 translate-x-4 rounded-xl border border-line bg-surface px-3 py-2 text-[13px] font-semibold"
          style={{ left: tip.x, top: tip.y }}
        >
          {hotRecord.title}
          <span className="block text-xs font-normal text-muted">Click to open</span>
        </div>
      )}
    </main>
  );
}

/** Day or night. Two labelled buttons rather than one icon, so nobody has to guess the state. */
function ModeSwitch({ mode, onChange }: { mode: RoomMode; onChange: (mode: RoomMode) => void }) {
  const options: { value: RoomMode; label: string }[] = [
    { value: 'day', label: 'Day' },
    { value: 'night', label: 'Night' },
  ];

  return (
    <div role="group" aria-label="Time of day in the room" className="flex flex-none rounded-full border border-line p-0.5">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          aria-pressed={mode === option.value}
          onClick={() => onChange(option.value)}
          className="min-h-8 rounded-full px-3 text-xs font-semibold text-muted aria-pressed:bg-accent aria-pressed:text-on-accent"
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}

function RecordLink({
  record,
  hot,
  onHot,
}: {
  record: RoomRecord;
  hot: boolean;
  onHot: (isHot: boolean) => void;
}) {
  const className = `flex min-h-11 items-center gap-3 rounded-xl px-2.5 py-1.5 hover:bg-raised ${hot ? 'bg-raised' : ''}`;
  const handlers = {
    onMouseEnter: () => onHot(true),
    onMouseLeave: () => onHot(false),
    onFocus: () => onHot(true),
    onBlur: () => onHot(false),
  };

  const content = (
    <>
      {/* The coloured edge is this record's spine colour on the shelf, tying row to record. */}
      <span
        className="h-9 w-10 flex-none overflow-hidden rounded bg-raised"
        style={{ borderLeft: `4px solid ${colourFor(record.id)}` }}
      >
        <SpotifyImage src={record.image} alt="" className="h-full w-full object-cover" />
      </span>
      <span className="flex min-w-0 flex-col">
        <span className="truncate font-semibold">{record.title}</span>
        {record.subtitle && <span className="truncate text-xs text-muted">{record.subtitle}</span>}
      </span>
    </>
  );

  return record.external ? (
    <a href={record.href} target="_blank" rel="noopener noreferrer" className={className} {...handlers}>
      {content}
    </a>
  ) : (
    <Link href={record.href} className={className} {...handlers}>
      {content}
    </Link>
  );
}
