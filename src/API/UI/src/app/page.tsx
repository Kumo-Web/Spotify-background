"use client";

import { useEffect, useState } from "react";
import { LayoutGroup } from "framer-motion";
import type { Album } from "@/types/music";
import { getAlbums } from "@/services/spotifyService";
import TurntableSection from "@/components/TurntableSection";
import VinylGrid from "@/components/VinylGrid";

export default function Home() {
  const [albums, setAlbums] = useState<Album[]>([]);

  useEffect(() => {
    getAlbums().then(setAlbums);
  }, []);

  return (
    <LayoutGroup>
      <div className="mx-auto max-w-[1600px] px-4 py-8 md:px-10">
        <section className="mb-6 text-center">
          <h1 className="text-4xl font-bold tracking-tight text-vinyl-cream md:text-5xl">
            Your Vinyl Collection
          </h1>
          <p className="mt-2 text-vinyl-cream/60">
            Pick a record and watch it spin on the deck.
          </p>
        </section>

        <TurntableSection />

        <section className="mt-16">
          <h2 className="mb-6 text-2xl font-semibold text-vinyl-cream">The Crate</h2>
          <VinylGrid albums={albums} />
        </section>
      </div>
    </LayoutGroup>
  );
}
