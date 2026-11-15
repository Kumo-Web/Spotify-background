'use client';

import Link from 'next/link';
import useSpotify from '@/hooks/useSpotify';

const Header = () => {
  const { session, logout } = useSpotify();

  return (
    <header className="mx-auto flex w-full max-w-3xl items-center justify-between px-4 py-4">
      <Link href="/" className="font-medium tracking-tight">
        Spotify Playlists
      </Link>

      {session?.authenticated && (
        <button
          type="button"
          onClick={logout}
          className="text-sm underline opacity-70 transition hover:opacity-100"
        >
          Sign out
        </button>
      )}
    </header>
  );
};

export default Header;
