"use client";

import { motion } from "framer-motion";
import type { Album } from "@/types/music";

interface VinylDiscProps {
  album: Album;
  size: number;
  spinning?: boolean;
  withLayoutId?: boolean;
}

/** The record itself: grooved disc with the album cover as a center label. */
const VinylDisc = ({ album, size, spinning = false, withLayoutId = true }: VinylDiscProps) => {
  const labelSize = Math.round(size * 0.38);
  const holeSize = Math.round(size * 0.05);

  return (
    <motion.div
      {...(withLayoutId ? { layoutId: `vinyl-disc-${album.id}` } : {})}
      className="relative rounded-full shadow-2xl shadow-black/60"
      style={{ width: size, height: size }}
      transition={{ type: "spring", stiffness: 120, damping: 18 }}
    >
      <div
        className={`vinyl-grooves absolute inset-0 rounded-full ring-1 ring-black/60 ${
          spinning ? "animate-spin-slow" : ""
        }`}
      >
        {/* highlight sheen */}
        <div className="absolute inset-0 rounded-full bg-gradient-to-tr from-transparent via-white/5 to-transparent" />
        {/* center label */}
        <div
          className="absolute left-1/2 top-1/2 flex items-center justify-center rounded-full"
          style={{
            width: labelSize,
            height: labelSize,
            transform: "translate(-50%, -50%)",
            backgroundImage: `url(${album.coverUrl})`,
            backgroundSize: "cover",
            backgroundPosition: "center",
            boxShadow: `0 0 0 3px ${album.accentColor}`,
          }}
        >
          {/* spindle hole */}
          <div
            className="rounded-full bg-vinyl-ink ring-1 ring-white/20"
            style={{ width: holeSize, height: holeSize }}
          />
        </div>
      </div>
    </motion.div>
  );
};

export default VinylDisc;
