import Footer from '@/components/footer';
import SceneCard from '@/features/turntable/scene-card';

interface LoginCardProps {
  /** Why the last attempt failed, already turned into a sentence. */
  problem: string | null;
  /** True while the session check is still running, when starting a login would be premature. */
  checking: boolean;
  onLogin: () => void;
}

/** The words and the one button on the log in page. */
export default function LoginCard({ problem, checking, onLogin }: LoginCardProps) {
  return (
    <SceneCard eyebrow="No record on" title="Put a record on.">
      <p className="max-w-[380px] text-[15px] leading-relaxed text-muted sm:text-base">
        Log in with Spotify and this turntable plays whatever you are listening to.
      </p>

      {problem && (
        <p role="alert" className="rounded-lg border border-accent bg-raised p-3 text-sm">
          {problem}
        </p>
      )}

      <button
        type="button"
        onClick={onLogin}
        disabled={checking}
        className="mt-2 flex h-[52px] w-full items-center justify-center rounded-full bg-accent px-7 font-semibold text-on-accent disabled:opacity-60 sm:w-auto sm:self-start"
      >
        {checking ? 'Checking session…' : 'Log in with Spotify'}
      </button>

      <p className="text-[13px] text-muted">You need Spotify Premium to control playback.</p>
      <Footer />
    </SceneCard>
  );
}
