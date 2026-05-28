import type { Album, Playlist, CreatePlaylistInput } from "@/types/music";
import { mockAlbums, getAlbumById } from "@/data/mockAlbums";
import { mockPlaylists } from "@/data/mockPlaylists";

export const getAuthUrl = async (): Promise<string> => {

  //TODO: get user id from auth service
  const userId = "6a6f528b-9ca5-4668-9144-dbe3221cd27f"
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifyAuth/login`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify(userId ),
});

const authUrl = await res.text();
  console.log("url", authUrl);
  return authUrl;
};

export const exchangeCode = async (code: string): Promise<any> => {
  debugger;
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifyAuth/callback?code=${code}`);
  return await res.json();
};


export const getUserInfo = async (userId: string): Promise<any> => {
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifyUser/getUserInfo/${userId}`, {
   method: 'GET',
   headers: {
    'Content-Type': 'application/json',
   },
  });
  return await res.json();
}

// In-memory store so newly created playlists persist for the session.
let playlists: Playlist[] = [...mockPlaylists];

// TODO: replace with real endpoint (e.g. GET /api/spotifyUser/savedAlbums) once exposed.
export const getAlbums = async (): Promise<Album[]> => {
  return Promise.resolve(mockAlbums);
};

// TODO: replace with real endpoint (e.g. GET /api/spotifyAlbum/{id}) once exposed.
export const getAlbum = async (id: string): Promise<Album | undefined> => {
  return Promise.resolve(getAlbumById(id));
};

// TODO: replace with real endpoint (GET /api/spotifyPlaylist) once wired to a controller.
export const getPlaylists = async (): Promise<Playlist[]> => {
  return Promise.resolve(playlists);
};

// TODO: replace with real endpoint (GET /api/spotifyPlaylist/{id}) — currently a backend stub.
export const getPlaylist = async (id: string): Promise<Playlist | undefined> => {
  return Promise.resolve(playlists.find((p) => p.id === id));
};

// TODO: replace with real endpoint (POST /api/spotifyPlaylist) once wired to a controller.
export const createPlaylist = async (input: CreatePlaylistInput): Promise<Playlist> => {
  const playlist: Playlist = {
    id: `${input.name.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "")}-${Date.now()}`,
    name: input.name,
    description: input.description,
    albumIds: input.albumIds,
  };
  playlists = [playlist, ...playlists];
  return Promise.resolve(playlist);
};