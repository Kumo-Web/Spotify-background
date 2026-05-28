"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { motion } from "framer-motion";
import { FaRecordVinyl } from "react-icons/fa6";

const links = [
  { href: "/", label: "Home" },
  { href: "/profile", label: "Profile" },
  { href: "/albums", label: "Albums" },
  { href: "/playlists/create", label: "Create Playlist" },
  { href: "/playlists", label: "View Playlists" },
];

const Navbar = () => {
  const pathname = usePathname();

  const isActive = (href: string) =>
    href === "/" ? pathname === "/" : pathname.startsWith(href);

  return (
    <header className="sticky top-0 z-40 w-full border-b border-white/10 bg-vinyl-ink/80 backdrop-blur-md">
      <div className="mx-auto flex h-header-mobile max-w-[1600px] items-center justify-between px-4 md:h-header md:px-10">
        <Link href="/" className="flex items-center gap-2 text-vinyl-cream">
          <FaRecordVinyl className="text-2xl text-vinyl-amber" />
          <span className="text-lg font-semibold tracking-wide">Wax &amp; Groove</span>
        </Link>

        <nav>
          <ul className="flex items-center gap-1 md:gap-2">
            {links.map((link) => (
              <li key={link.href} className="relative">
                <Link
                  href={link.href}
                  className={`relative inline-block rounded-full px-3 py-2 text-xs transition-colors md:text-sm ${
                    isActive(link.href)
                      ? "text-vinyl-amber"
                      : "text-vinyl-cream/70 hover:text-vinyl-cream"
                  }`}
                >
                  {link.label}
                  {isActive(link.href) && (
                    <motion.span
                      layoutId="nav-active"
                      className="absolute inset-0 -z-10 rounded-full bg-white/5"
                      transition={{ type: "spring", stiffness: 380, damping: 30 }}
                    />
                  )}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
      </div>
    </header>
  );
};

export default Navbar;
