"use client";

import type { Album } from "@/types/music";
import VinylCard from "@/components/VinylCard";

interface VinylGridProps {
  albums: Album[];
}

const VinylGrid = ({ albums }: VinylGridProps) => {
  return (
    <div className="grid grid-cols-2 gap-x-8 gap-y-10 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
      {albums.map((album) => (
        <VinylCard key={album.id} album={album} />
      ))}
    </div>
  );
};

export default VinylGrid;
