"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import type { Album, Playlist } from "@/types/music";
import { getAlbums, getPlaylists } from "@/services/spotifyService";

export default function PlaylistsPage() {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [albums, setAlbums] = useState<Album[]>([]);

  useEffect(() => {
    getPlaylists().then(setPlaylists);
    getAlbums().then(setAlbums);
  }, []);

  const coverFor = (id: string) => albums.find((a) => a.id === id)?.coverUrl;

  return (
    <div className="mx-auto max-w-[1600px] px-4 py-8 md:px-10">
      <div className="mb-8 flex items-center justify-between">
        <h1 className="text-3xl font-bold text-vinyl-cream">Your Playlists</h1>
        <Link
          href="/playlists/create"
          className="rounded-full bg-vinyl-amber px-4 py-2 text-sm font-semibold text-vinyl-ink transition-transform hover:scale-105"
        >
          + New Playlist
        </Link>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {playlists.map((playlist) => (
          <div
            key={playlist.id}
            className="rounded-xl border border-white/10 bg-vinyl-panel p-5 transition-colors hover:border-vinyl-amber/50"
          >
            {/* stacked sleeves = a record crate */}
            <div className="relative mb-4 h-32">
              {playlist.albumIds.slice(0, 4).map((id, i) => {
                const cover = coverFor(id);
                return (
                  <div
                    key={id}
                    className="absolute top-0 h-32 w-32 rounded-md border border-white/10 bg-vinyl-charcoal bg-cover bg-center shadow-lg"
                    style={{
                      left: i * 28,
                      zIndex: i,
                      backgroundImage: cover ? `url(${cover})` : undefined,
                      transform: `rotate(${(i - 1.5) * 3}deg)`,
                    }}
                  />
                );
              })}
            </div>
            <h3 className="text-xl font-semibold text-vinyl-cream">{playlist.name}</h3>
            <p className="mt-1 text-sm text-vinyl-cream/60">{playlist.description}</p>
            <p className="mt-3 text-xs uppercase tracking-wide text-vinyl-cream/40">
              {playlist.albumIds.length} records
            </p>
          </div>
        ))}
      </div>

      {playlists.length === 0 && (
        <p className="text-vinyl-cream/50">No playlists yet — create your first crate.</p>
      )}
    </div>
  );
}
