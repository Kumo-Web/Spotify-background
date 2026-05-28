import type { Playlist } from "@/types/music";

export const mockPlaylists: Playlist[] = [
  {
    id: "late-night-spin",
    name: "Late Night Spin",
    description: "Smooth records for the small hours.",
    albumIds: ["midnight-city", "velvet-haze", "after-hours", "low-orbit"],
  },
  {
    id: "sunday-crate-digging",
    name: "Sunday Crate Digging",
    description: "Warm analog finds for a slow morning.",
    albumIds: ["analog-dreams", "warm-static", "rust-and-gold", "paper-moons"],
  },
];
