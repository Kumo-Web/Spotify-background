"use client";

import { AnimatePresence, motion } from "framer-motion";
import { FaPlay, FaPause, FaStop } from "react-icons/fa6";
import { usePlayerStore } from "@/store/playerStore";

const NowPlayingBar = () => {
  const currentVinyl = usePlayerStore((s) => s.currentVinyl);
  const isPlaying = usePlayerStore((s) => s.isPlaying);
  const togglePlay = usePlayerStore((s) => s.togglePlay);
  const stop = usePlayerStore((s) => s.stop);

  return (
    <AnimatePresence>
      {currentVinyl && (
        <motion.div
          initial={{ y: 80, opacity: 0 }}
          animate={{ y: 0, opacity: 1 }}
          exit={{ y: 80, opacity: 0 }}
          transition={{ type: "spring", stiffness: 320, damping: 32 }}
          className="fixed bottom-0 left-0 right-0 z-50 border-t border-white/10 bg-vinyl-charcoal/95 backdrop-blur-md"
        >
          <div className="mx-auto flex max-w-[1600px] items-center gap-4 px-4 py-3 md:px-10">
            <div
              className={`h-12 w-12 shrink-0 rounded-full bg-cover bg-center ring-2 ring-vinyl-amber/70 ${
                isPlaying ? "animate-spin-slow" : ""
              }`}
              style={{ backgroundImage: `url(${currentVinyl.coverUrl})` }}
            />
            <div className="min-w-0 flex-1">
              <p className="truncate font-semibold text-vinyl-cream">{currentVinyl.title}</p>
              <p className="truncate text-sm text-vinyl-cream/60">{currentVinyl.artist}</p>
            </div>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={togglePlay}
                aria-label={isPlaying ? "Pause" : "Play"}
                className="flex h-10 w-10 items-center justify-center rounded-full bg-vinyl-amber text-vinyl-ink transition-transform hover:scale-110"
              >
                {isPlaying ? <FaPause /> : <FaPlay className="ml-0.5" />}
              </button>
              <button
                type="button"
                onClick={stop}
                aria-label="Stop"
                className="flex h-10 w-10 items-center justify-center rounded-full border border-white/15 text-vinyl-cream/80 transition-colors hover:bg-white/10"
              >
                <FaStop />
              </button>
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};

export default NowPlayingBar;
