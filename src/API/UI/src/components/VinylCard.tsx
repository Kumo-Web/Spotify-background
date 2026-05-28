"use client";

import { motion } from "framer-motion";
import { FaPlay } from "react-icons/fa6";
import type { Album } from "@/types/music";
import { usePlayerStore } from "@/store/playerStore";
import VinylDisc from "@/components/VinylDisc";

interface VinylCardProps {
  album: Album;
}

const VinylCard = ({ album }: VinylCardProps) => {
  const currentVinyl = usePlayerStore((s) => s.currentVinyl);
  const playVinyl = usePlayerStore((s) => s.playVinyl);
  const isCurrent = currentVinyl?.id === album.id;

  return (
    <motion.div
      className="group relative"
      whileHover={{ y: -6 }}
      transition={{ type: "spring", stiffness: 300, damping: 24 }}
    >
      <div className="relative aspect-square overflow-visible">
        {/* The disc peeks out from behind the sleeve; it leaves once playing. */}
        <div className="absolute inset-0 flex items-center justify-end">
          {!isCurrent && (
            <motion.div
              className="relative right-[-10%] w-[78%] transition-[right] duration-300 group-hover:right-[-26%]"
            >
              <VinylDisc album={album} size={220} />
            </motion.div>
          )}
        </div>

        {/* Sleeve */}
        <div
          className="absolute inset-0 overflow-hidden rounded-md border border-white/10 bg-vinyl-panel shadow-xl shadow-black/50"
          style={{
            backgroundImage: `url(${album.coverUrl})`,
            backgroundSize: "cover",
            backgroundPosition: "center",
          }}
        >
          <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/0 to-black/10" />
          <button
            type="button"
            onClick={() => playVinyl(album)}
            aria-label={`Play ${album.title}`}
            className="absolute bottom-3 right-3 flex h-12 w-12 items-center justify-center rounded-full bg-vinyl-amber text-vinyl-ink opacity-0 shadow-lg transition-all hover:scale-110 group-hover:opacity-100"
          >
            <FaPlay className="ml-0.5" />
          </button>
        </div>
      </div>

      <div className="mt-3 px-1">
        <p className="truncate font-semibold text-vinyl-cream">{album.title}</p>
        <p className="truncate text-sm text-vinyl-cream/60">
          {album.artist} · {album.year}
        </p>
      </div>
    </motion.div>
  );
};

export default VinylCard;
