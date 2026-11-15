import SpotifyImage from '@/components/spotify-image';
import styles from './turntable.module.css';

/** off: no record on. playing: turning. paused: a record is on but still. */
export type TurntablePower = 'off' | 'playing' | 'paused';

interface TurntableProps {
  power: TurntablePower;

  /** Centre of the record: the artist photo when there is one, otherwise printed text. */
  labelImage?: string | null;
  labelTitle?: string;
  labelArtist?: string;

  /** The album cover in the sleeve. Ignored when `card` is given. */
  coverImage?: string | null;
  coverAlt?: string;

  /** Shown in place of the sleeve, as an empty sleeve holding a message. */
  card?: React.ReactNode;

  /** Makes the needle a button. Leave out and it is decoration only. */
  needle?: { label: string; onClick: () => void; disabled?: boolean };

  /** Anything else placed on the scene, such as the caption. */
  children?: React.ReactNode;
}

/**
 * The record player. It only draws: it knows nothing about Spotify or sessions, which is what
 * lets the log in page and the home page share it. All sizes and positions are in
 * turntable.module.css.
 */
export default function Turntable({
  power,
  labelImage,
  labelTitle,
  labelArtist,
  coverImage,
  coverAlt = '',
  card,
  needle,
  children,
}: TurntableProps) {
  const hasRecord = power !== 'off';

  return (
    <div className={styles.scene} data-power={power}>
      <div className={styles.record} aria-hidden="true">
        <div className={`${styles.ring} ${styles.ringOuter}`} />
        <div className={`${styles.ring} ${styles.ringInner}`} />
      </div>

      {/* Hidden from screen readers: the caption already says what is playing, as real text. */}
      <div className={styles.label} aria-hidden="true">
        {hasRecord && (
          <>
            {(labelTitle || labelArtist) && (
              <div className={styles.labelText}>
                <span className={styles.labelTitle}>{labelTitle}</span>
                <span className={styles.labelArtist}>{labelArtist}</span>
              </div>
            )}
            {/* Drawn over the text, so a missing or refused photo leaves the text showing. */}
            <SpotifyImage src={labelImage} alt="" className={styles.labelImage} />
          </>
        )}
        <span className={styles.spindle} />
      </div>

      {card ? (
        <div className={styles.card}>{card}</div>
      ) : (
        <div className={styles.sleeve}>
          <SpotifyImage src={coverImage} alt={coverAlt} className={styles.sleeveImage} />
        </div>
      )}

      <div className={`${styles.arm} ${styles.armLower}`} aria-hidden="true" />
      <div className={`${styles.arm} ${styles.armUpper}`} aria-hidden="true" />
      <div className={styles.elbow} aria-hidden="true" />

      {needle ? (
        <button
          type="button"
          className={styles.needle}
          aria-label={needle.label}
          onClick={needle.onClick}
          disabled={needle.disabled}
        >
          <span className={styles.needleHead} aria-hidden="true" />
        </button>
      ) : (
        <div className={styles.needle} aria-hidden="true">
          <span className={styles.needleHead} />
        </div>
      )}

      {children}
    </div>
  );
}
