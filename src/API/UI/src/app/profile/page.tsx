"use client";

import { useEffect, useState } from "react";
import { FaRecordVinyl, FaSpotify } from "react-icons/fa6";
import { getUserInfo } from "@/services/spotifyService";
import { mockAlbums } from "@/data/mockAlbums";

const DEMO_USER_ID = "6a6f528b-9ca5-4668-9144-dbe3221cd27f";

interface ProfileView {
  displayName: string;
  imageUrl?: string;
  product?: string;
  followers?: number;
  live: boolean;
}

const mockProfile: ProfileView = {
  displayName: "Crate Digger",
  product: "premium",
  followers: 128,
  live: false,
};

export default function ProfilePage() {
  const [profile, setProfile] = useState<ProfileView | null>(null);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await getUserInfo(DEMO_USER_ID);
        if (cancelled) return;
        if (data && (data.display_name || data.id)) {
          setProfile({
            displayName: data.display_name ?? "Spotify User",
            imageUrl: data.images?.[0]?.url,
            product: data.product,
            followers: data.followers?.total,
            live: true,
          });
        } else {
          setProfile(mockProfile);
        }
      } catch {
        if (!cancelled) setProfile(mockProfile);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  if (!profile) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-16 text-center text-vinyl-cream/50 md:px-10">
        Loading profile…
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 md:px-10">
      <div className="flex flex-col items-center gap-6 text-center sm:flex-row sm:text-left">
        <div
          className="flex h-32 w-32 shrink-0 items-center justify-center rounded-full bg-cover bg-center ring-4 ring-vinyl-amber/60"
          style={{ backgroundImage: profile.imageUrl ? `url(${profile.imageUrl})` : undefined }}
        >
          {!profile.imageUrl && <FaRecordVinyl className="text-5xl text-vinyl-amber" />}
        </div>
        <div>
          <h1 className="text-4xl font-bold text-vinyl-cream">{profile.displayName}</h1>
          <p className="mt-1 flex items-center justify-center gap-2 text-vinyl-cream/60 sm:justify-start">
            <FaSpotify className="text-vinyl-teal" />
            {profile.product ? `${profile.product} plan` : "Spotify listener"}
          </p>
          <span
            className={`mt-3 inline-block rounded-full px-3 py-1 text-xs ${
              profile.live
                ? "bg-vinyl-teal/20 text-vinyl-teal"
                : "bg-white/10 text-vinyl-cream/60"
            }`}
          >
            {profile.live ? "Connected to Spotify" : "Demo profile (API offline)"}
          </span>
        </div>
      </div>

      <div className="mt-10 grid grid-cols-2 gap-4 sm:grid-cols-3">
        <Stat label="Records" value={mockAlbums.length} />
        <Stat label="Followers" value={profile.followers ?? 0} />
        <Stat label="Artists" value={new Set(mockAlbums.map((a) => a.artist)).size} />
      </div>
    </div>
  );
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-xl border border-white/10 bg-vinyl-panel p-5 text-center">
      <p className="text-3xl font-bold text-vinyl-amber">{value}</p>
      <p className="mt-1 text-sm uppercase tracking-wide text-vinyl-cream/50">{label}</p>
    </div>
  );
}
