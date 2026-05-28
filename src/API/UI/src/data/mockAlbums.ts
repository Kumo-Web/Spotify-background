import type { Album } from "@/types/music";

const cover = (seed: string) => `https://picsum.photos/seed/${seed}/600/600`;

const tracks = (prefix: string): Album["tracks"] =>
  Array.from({ length: 5 }, (_, i) => ({
    id: `${prefix}-t${i + 1}`,
    title: `Track ${i + 1}`,
    durationMs: 180000 + i * 24000,
  }));

export const mockAlbums: Album[] = [
  { id: "midnight-city", title: "Midnight City", artist: "Neon Skyline", year: 2019, accentColor: "#e8b04b", coverUrl: cover("midnight-city"), tracks: tracks("midnight-city") },
  { id: "velvet-haze", title: "Velvet Haze", artist: "The Slow Hours", year: 2021, accentColor: "#c2476a", coverUrl: cover("velvet-haze"), tracks: tracks("velvet-haze") },
  { id: "analog-dreams", title: "Analog Dreams", artist: "Cassette Sun", year: 2017, accentColor: "#4bb3a8", coverUrl: cover("analog-dreams"), tracks: tracks("analog-dreams") },
  { id: "northern-lights", title: "Northern Lights", artist: "Aurora Drift", year: 2020, accentColor: "#6c8ae4", coverUrl: cover("northern-lights"), tracks: tracks("northern-lights") },
  { id: "desert-bloom", title: "Desert Bloom", artist: "Cacti & Co", year: 2018, accentColor: "#d9772b", coverUrl: cover("desert-bloom"), tracks: tracks("desert-bloom") },
  { id: "paper-moons", title: "Paper Moons", artist: "Lullaby Engine", year: 2022, accentColor: "#9a6cd1", coverUrl: cover("paper-moons"), tracks: tracks("paper-moons") },
  { id: "rust-and-gold", title: "Rust & Gold", artist: "The Foundry", year: 2016, accentColor: "#b8893b", coverUrl: cover("rust-and-gold"), tracks: tracks("rust-and-gold") },
  { id: "ocean-static", title: "Ocean Static", artist: "Tidewave", year: 2023, accentColor: "#2f9bd1", coverUrl: cover("ocean-static"), tracks: tracks("ocean-static") },
  { id: "city-of-glass", title: "City of Glass", artist: "Mirage", year: 2015, accentColor: "#7ec06b", coverUrl: cover("city-of-glass"), tracks: tracks("city-of-glass") },
  { id: "low-orbit", title: "Low Orbit", artist: "Satellite Hearts", year: 2021, accentColor: "#e0556b", coverUrl: cover("low-orbit"), tracks: tracks("low-orbit") },
  { id: "warm-static", title: "Warm Static", artist: "Hearth", year: 2019, accentColor: "#d98c4a", coverUrl: cover("warm-static"), tracks: tracks("warm-static") },
  { id: "after-hours", title: "After Hours", artist: "Lamplight", year: 2024, accentColor: "#8c6ce0", coverUrl: cover("after-hours"), tracks: tracks("after-hours") },
];

export const getAlbumById = (id: string): Album | undefined =>
  mockAlbums.find((a) => a.id === id);
