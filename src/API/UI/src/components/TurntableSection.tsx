"use client";

import dynamic from "next/dynamic";
import { AnimatePresence, motion } from "framer-motion";
import { usePlayerStore } from "@/store/playerStore";
import VinylDisc from "@/components/VinylDisc";

// R3F touches `window`, so the turntable must never render on the server.
const Turntable3D = dynamic(() => import("@/components/Turntable3D"), {
  ssr: false,
  loading: () => (
    <div className="absolute inset-0 flex items-center justify-center text-vinyl-cream/40">
      Loading turntable…
    </div>
  ),
});

const TurntableSection = () => {
  const currentVinyl = usePlayerStore((s) => s.currentVinyl);
  const isPlaying = usePlayerStore((s) => s.isPlaying);

  return (
    <section className="relative mx-auto h-[420px] w-full max-w-4xl md:h-[520px]">
      <Turntable3D />

      {/* The record rests on the platter. It flies here (shared layout) when played. */}
      <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
        <div className="-mt-6 md:-mt-10">
          <AnimatePresence>
            {currentVinyl && (
              <motion.div
                key={currentVinyl.id}
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
              >
                <VinylDisc album={currentVinyl} size={300} spinning={isPlaying} />
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      </div>

      {!currentVinyl && (
        <div className="pointer-events-none absolute bottom-4 left-0 right-0 text-center text-sm text-vinyl-cream/50">
          Hover a record and hit ▶ to drop it on the deck
        </div>
      )}

      {currentVinyl && (
        <div className="absolute bottom-2 left-0 right-0 text-center">
          <p className="font-semibold text-vinyl-cream">{currentVinyl.title}</p>
          <p className="text-sm text-vinyl-cream/60">{currentVinyl.artist}</p>
        </div>
      )}
    </section>
  );
};

export default TurntableSection;
