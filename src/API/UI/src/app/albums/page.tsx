"use client";

import { useEffect, useMemo, useState } from "react";
import { LayoutGroup } from "framer-motion";
import type { Album } from "@/types/music";
import { getAlbums } from "@/services/spotifyService";
import VinylGrid from "@/components/VinylGrid";

export default function AlbumsPage() {
  const [albums, setAlbums] = useState<Album[]>([]);
  const [query, setQuery] = useState("");

  useEffect(() => {
    getAlbums().then(setAlbums);
  }, []);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return albums;
    return albums.filter(
      (a) => a.title.toLowerCase().includes(q) || a.artist.toLowerCase().includes(q)
    );
  }, [albums, query]);

  return (
    <LayoutGroup>
      <div className="mx-auto max-w-[1600px] px-4 py-8 md:px-10">
        <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <h1 className="text-3xl font-bold text-vinyl-cream">Albums</h1>
          <input
            type="search"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search records or artists…"
            className="w-full rounded-full border border-white/10 bg-vinyl-panel px-4 py-2 text-vinyl-cream placeholder:text-vinyl-cream/40 focus:border-vinyl-amber focus:outline-none sm:w-72"
          />
        </div>

        {filtered.length > 0 ? (
          <VinylGrid albums={filtered} />
        ) : (
          <p className="text-vinyl-cream/50">No records match “{query}”.</p>
        )}
      </div>
    </LayoutGroup>
  );
}
