import type { Metadata } from 'next';
import { notFound } from 'next/navigation';
import PlaylistTracks from '@/features/playlists/playlist-tracks';
import { isSpotifyId } from '@/lib/security';

export const metadata: Metadata = { title: 'Playlist' };

export default async function PlaylistPage({
  params,
}: {
  params: Promise<{ playlistId: string }>;
}) {
  const { playlistId } = await params;

  // The id comes straight from the URL, so anyone can type anything there. Refusing whatever
  // is not shaped like a Spotify id means nothing odd is ever passed on to the API.
  if (!isSpotifyId(playlistId)) {
    notFound();
  }

  return <PlaylistTracks playlistId={playlistId} />;
}
