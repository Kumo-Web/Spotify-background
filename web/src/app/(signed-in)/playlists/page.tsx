import type { Metadata } from 'next';
import PlaylistsShelf from '@/features/playlists/playlists-shelf';

export const metadata: Metadata = { title: 'Playlists' };

export default function PlaylistsPage() {
  return <PlaylistsShelf />;
}
