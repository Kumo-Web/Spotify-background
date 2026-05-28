"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import type { Album } from "@/types/music";
import { createPlaylist, getAlbums } from "@/services/spotifyService";

export default function CreatePlaylistPage() {
  const router = useRouter();
  const [albums, setAlbums] = useState<Album[]>([]);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    getAlbums().then(setAlbums);
  }, []);

  const toggle = (id: string) =>
    setSelected((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setSubmitting(true);
    await createPlaylist({ name: name.trim(), description: description.trim(), albumIds: selected });
    router.push("/playlists");
  };

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 md:px-10">
      <h1 className="mb-8 text-3xl font-bold text-vinyl-cream">Create a Playlist</h1>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div>
          <label className="mb-1 block text-sm text-vinyl-cream/70">Name</label>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="My late-night crate"
            className="w-full rounded-lg border border-white/10 bg-vinyl-panel px-4 py-2 text-vinyl-cream placeholder:text-vinyl-cream/40 focus:border-vinyl-amber focus:outline-none"
            required
          />
        </div>

        <div>
          <label className="mb-1 block text-sm text-vinyl-cream/70">Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="What's the vibe?"
            rows={3}
            className="w-full rounded-lg border border-white/10 bg-vinyl-panel px-4 py-2 text-vinyl-cream placeholder:text-vinyl-cream/40 focus:border-vinyl-amber focus:outline-none"
          />
        </div>

        <div>
          <label className="mb-2 block text-sm text-vinyl-cream/70">
            Pick records ({selected.length} selected)
          </label>
          <div className="grid grid-cols-3 gap-3 sm:grid-cols-4 md:grid-cols-6">
            {albums.map((album) => {
              const isSel = selected.includes(album.id);
              return (
                <button
                  type="button"
                  key={album.id}
                  onClick={() => toggle(album.id)}
                  className={`group relative aspect-square overflow-hidden rounded-md border-2 transition-all ${
                    isSel ? "border-vinyl-amber" : "border-transparent opacity-70 hover:opacity-100"
                  }`}
                  style={{
                    backgroundImage: `url(${album.coverUrl})`,
                    backgroundSize: "cover",
                    backgroundPosition: "center",
                  }}
                  aria-pressed={isSel}
                  title={`${album.title} — ${album.artist}`}
                >
                  {isSel && (
                    <span className="absolute inset-0 flex items-center justify-center bg-vinyl-amber/30 text-lg font-bold text-vinyl-ink">
                      ✓
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        </div>

        <button
          type="submit"
          disabled={submitting || !name.trim()}
          className="rounded-full bg-vinyl-amber px-6 py-2.5 font-semibold text-vinyl-ink transition-transform hover:scale-105 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {submitting ? "Creating…" : "Create Playlist"}
        </button>
      </form>
    </div>
  );
}
