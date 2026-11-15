'use client';

import { useEffect, useRef, useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useSession } from '@/features/auth/session-provider';
import { SPOTIFY_HOME_URL } from '@/lib/security';
import { SITE_NAME } from '@/lib/site';
import Footer from './footer';

const LINKS = [
  { href: '/', label: 'Home' },
  { href: '/playlists', label: 'Playlists' },
];

/**
 * The bar across the top of every signed-in page: the name in the middle, the menu on the right.
 *
 * It floats over the page instead of taking a row of its own, because each page underneath is a
 * full-screen scene. `pointer-events-none` on the bar keeps the empty space between its items
 * from swallowing clicks meant for that scene.
 */
export default function Header() {
  const [open, setOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const pathname = usePathname();
  const { logout } = useSession();

  // Following a link in the menu should close it.
  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!open) return;

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setOpen(false);
    };
    const onPointerDown = (event: PointerEvent) => {
      if (!menuRef.current?.contains(event.target as Node)) setOpen(false);
    };

    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('pointerdown', onPointerDown);

    return () => {
      document.removeEventListener('keydown', onKeyDown);
      document.removeEventListener('pointerdown', onPointerDown);
    };
  }, [open]);

  return (
    <header className="pointer-events-none absolute inset-x-0 top-0 z-30 grid grid-cols-[1fr_auto_1fr] items-center px-4 py-2 sm:px-10 sm:py-6">
      <span />

      <Link
        href="/"
        className="pointer-events-auto font-display text-lg font-bold tracking-tight sm:text-[22px]"
      >
        {SITE_NAME}
      </Link>

      <div ref={menuRef} className="pointer-events-auto relative justify-self-end">
        <button
          type="button"
          aria-expanded={open}
          aria-controls="site-menu"
          onClick={() => setOpen((value) => !value)}
          className="flex h-11 items-center gap-2.5 rounded-full px-3 text-sm font-semibold sm:px-4 sm:text-[15px]"
        >
          Menu
          <svg
            aria-hidden="true"
            width="20"
            height="20"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
          >
            <path d="M4 9h16M4 15h16" />
          </svg>
        </button>

        {open && (
          <nav
            id="site-menu"
            aria-label="Main"
            className="absolute right-0 top-full mt-2 w-60 rounded-2xl border border-line bg-surface p-2 shadow-2xl"
          >
            <ul>
              {LINKS.map((link) => (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    aria-current={pathname === link.href ? 'page' : undefined}
                    className="flex min-h-11 items-center rounded-xl px-3 font-medium hover:bg-raised aria-[current=page]:text-accent"
                  >
                    {link.label}
                  </Link>
                </li>
              ))}
              <li>
                <a
                  href={SPOTIFY_HOME_URL}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="flex min-h-11 items-center rounded-xl px-3 font-medium hover:bg-raised"
                >
                  Open Spotify
                </a>
              </li>
              <li className="mt-1 border-t border-line pt-1">
                <button
                  type="button"
                  onClick={() => void logout()}
                  className="flex min-h-11 w-full items-center rounded-xl px-3 text-left font-medium text-muted hover:bg-raised hover:text-fg"
                >
                  Sign out
                </button>
              </li>
            </ul>
            <Footer className="px-3 pb-1 pt-2" />
          </nav>
        )}
      </div>
    </header>
  );
}
