'use client';

import { useEffect, useState } from 'react';
import Link from 'next/link';
import useSignedOutRedirect from '@/features/auth/useSignedOutRedirect';
import usePlaylists from '@/features/playlists/usePlaylists';
import useMediaQuery from '@/hooks/useMediaQuery';
import { ApiError, describeError } from '@/lib/api';
import { features } from '@/lib/features';
import { formatDuration } from '@/lib/format';
import { SPOTIFY_HOME_URL } from '@/lib/security';
import {
  pausePlayback,
  resumePlayback,
  skipToNext,
  skipToPrevious,
} from '@/services/spotifyService';
import { hasTrack, type PlayingTrack } from '@/types/spotify';
import SceneCard from './scene-card';
import Turntable from './turntable';
import styles from './turntable.module.css';
import useNowPlaying from './useNowPlaying';

/** The same test turntable.module.css uses to switch to the phone drawing. Keep them in step. */
const PHONE_LAYOUT = '(max-width: 700px), (max-aspect-ratio: 1/1)';

/**
 * The home page: the turntable showing what is playing, with a small caption.
 *
 * It has three faces. A track is on: record, sleeve and caption. Nothing is on, still loading,
 * or the request failed: the sleeve becomes a card that says so. Keeping those in one component
 * means the turntable itself never has to know why it is empty.
 */
export default function HomeTurntable() {
  const { nowPlaying, fetchedAt, error, refresh } = useNowPlaying();

  const [busy, setBusy] = useState(false);
  const [controlError, setControlError] = useState<string | null>(null);

  // An expired session is a trip to the log in page, not an error message.
  const signedOut = useSignedOutRedirect(error);

  // The playlist spines exist only in the phone drawing. Rendering them elsewhere would fetch
  // every playlist from Spotify on each visit to the home page, for something CSS then hides.
  const phoneLayout = useMediaQuery(PHONE_LAYOUT);

  const track = hasTrack(nowPlaying) ? nowPlaying : null;
  const progressMs = useLiveProgress(track, fetchedAt);

  if (!track) {
    return (
      <Turntable
        power="off"
        card={<EmptyCard loading={nowPlaying === null} error={signedOut ? null : error} onRetry={refresh} />}
      />
    );
  }

  /** Sends one playback command, then asks Spotify what is true now instead of guessing. */
  const send = async (command: () => Promise<void>) => {
    setBusy(true);
    try {
      await command();
      setControlError(null);
      await refresh();
    } catch (err) {
      // Spotify answers 404 when no device is active, and the API is expected to pass that
      // status on. It is by far the most common failure here, so it gets instructions instead
      // of the generic message.
      setControlError(
        err instanceof ApiError && err.status === 404
          ? "Can't find a device. Open Spotify on your phone or computer, then try again."
          : describeError(err),
      );
    } finally {
      setBusy(false);
    }
  };

  const controls = features.playbackControls;

  return (
    <Turntable
      power={track.isPlaying ? 'playing' : 'paused'}
      labelImage={track.artistImage}
      labelTitle={track.title}
      labelArtist={track.artists[0]}
      coverImage={track.albumArt}
      coverAlt={`Cover of ${track.album}`}
      needle={
        controls
          ? {
              label: track.isPlaying ? 'Pause' : 'Play',
              disabled: busy,
              onClick: () => void send(track.isPlaying ? pausePlayback : resumePlayback),
            }
          : undefined
      }
    >
      {track.deviceName && (
        <p className={`${styles.device} flex h-11 items-center rounded-full border border-line bg-surface px-4 text-[13px]`}>
          Playing on {track.deviceName}
        </p>
      )}

      {controls && (
        <p className={`${styles.hint} flex h-[30px] items-center whitespace-nowrap rounded-full border border-line bg-surface px-3 text-xs text-muted`}>
          {track.isPlaying ? 'Tap the needle to pause' : 'Tap the needle to play'}
        </p>
      )}

      <div className={styles.caption}>
        <div className="min-w-0 max-w-full">
          <p className="text-[11px] font-semibold uppercase tracking-[0.08em] text-muted">
            {track.isPlaying ? 'Now spinning' : 'Paused'}
          </p>
          <h1 className="truncate font-display text-[22px] font-bold leading-tight tracking-tight sm:text-3xl">
            {track.title}
          </h1>
          <p className="truncate text-sm text-muted">{track.artists.join(', ')}</p>
          <p className="text-xs tabular-nums text-muted">
            {formatDuration(progressMs)} of {formatDuration(track.durationMs)}
          </p>
          {controlError && (
            <p role="alert" className="mt-1 text-xs text-accent">
              {controlError}
            </p>
          )}
        </div>

        {controls && (
          <div className="flex gap-1.5">
            <SkipButton label="Previous track" disabled={busy} onClick={() => void send(skipToPrevious)}>
              <path d="M18 5v14L8 12z" fill="currentColor" />
              <path d="M6 5v14" />
            </SkipButton>
            <SkipButton label="Next track" disabled={busy} onClick={() => void send(skipToNext)}>
              <path d="M6 5v14l10-7z" fill="currentColor" />
              <path d="M18 5v14" />
            </SkipButton>
          </div>
        )}
      </div>

      {phoneLayout && <PlaylistSpines />}
    </Turntable>
  );
}

/**
 * The track position, ticking once a second between polls.
 *
 * The API reports where the track was when it was asked. Showing only that would make the clock
 * jump five seconds at a time, so the time since the answer is added on while music is playing.
 */
function useLiveProgress(track: PlayingTrack | null, fetchedAt: number): number {
  const [now, setNow] = useState(fetchedAt);
  const playing = track?.isPlaying === true;

  useEffect(() => {
    if (!playing) return;

    const timer = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(timer);
  }, [playing]);

  if (!track) return 0;

  const reported = track.progressMs ?? 0;
  const sinceAnswer = playing ? Math.max(0, now - fetchedAt) : 0;

  return Math.min(track.durationMs, reported + sinceAnswer);
}

function EmptyCard({
  loading,
  error,
  onRetry,
}: {
  loading: boolean;
  error: unknown;
  onRetry: () => Promise<void>;
}) {
  if (error) {
    return (
      <SceneCard eyebrow="Problem" title="Can't load what's playing.">
        <p role="alert" className="text-[15px] leading-relaxed text-muted">
          {describeError(error)}
        </p>
        <button
          type="button"
          onClick={() => void onRetry()}
          className="mt-2 flex h-[52px] items-center self-start rounded-full bg-accent px-7 font-semibold text-on-accent"
        >
          Try again
        </button>
      </SceneCard>
    );
  }

  if (loading) {
    return <SceneCard eyebrow="One moment" title="Checking what's playing…" />;
  }

  return (
    <SceneCard eyebrow="No record on" title="Nothing playing.">
      <p className="text-[15px] leading-relaxed text-muted">
        Start a song in Spotify on your phone or computer and it shows up here.
      </p>
      <a
        href={SPOTIFY_HOME_URL}
        target="_blank"
        rel="noopener noreferrer"
        className="mt-2 flex h-[52px] items-center self-start rounded-full bg-accent px-7 font-semibold text-on-accent"
      >
        Open Spotify
      </a>
    </SceneCard>
  );
}

function SkipButton({
  label,
  disabled,
  onClick,
  children,
}: {
  label: string;
  disabled: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className="flex h-11 w-11 items-center justify-center rounded-full border border-line bg-surface disabled:opacity-50"
    >
      <svg
        aria-hidden="true"
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      >
        {children}
      </svg>
    </button>
  );
}

/**
 * Phones only: the first two playlists as record spines, a way into the playlist room that
 * belongs to the scene. Failing to load them is not worth an error
 * on the home page — the menu still leads to the same place — so nothing is drawn instead.
 */
function PlaylistSpines() {
  const { playlists } = usePlaylists();

  if (!playlists || playlists.length === 0) return null;

  return (
    <nav aria-label="Your playlists" className={styles.spines}>
      <Link
        href="/playlists"
        className={`${styles.spinesLabel} text-[11px] font-semibold uppercase tracking-[0.08em] text-muted`}
      >
        Your playlists
      </Link>
      {playlists.slice(0, 2).map(({ playlist }, index) => (
        <Link
          key={playlist.id}
          href={`/playlists/${playlist.id}`}
          className={`${styles.spine} ${index === 0 ? styles.spineFirst : styles.spineSecond} text-[13px] font-semibold`}
        >
          <span className="truncate">{playlist.name}</span>
        </Link>
      ))}
      <span aria-hidden="true" className={`${styles.spine} ${styles.spineRest}`} />
    </nav>
  );
}
