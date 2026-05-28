import { create } from "zustand";
import type { Album } from "@/types/music";

interface PlayerState {
  currentVinyl: Album | null;
  isPlaying: boolean;
  playVinyl: (album: Album) => void;
  togglePlay: () => void;
  stop: () => void;
}

export const usePlayerStore = create<PlayerState>((set) => ({
  currentVinyl: null,
  isPlaying: false,
  playVinyl: (album) => set({ currentVinyl: album, isPlaying: true }),
  togglePlay: () => set((state) => ({ isPlaying: !state.isPlaying })),
  stop: () => set({ currentVinyl: null, isPlaying: false }),
}));
