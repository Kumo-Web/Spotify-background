'use client';

import { useSearchParams, useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { exchangeCode } from '../../services/spotifyService';

export default function CallbackPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const code = searchParams.get('code');

  useEffect(() => {
    if (code) {
      exchangeCode(code).then(() => {
        router.push('/dashboard');
      });
    }
  }, [code]);

  return <p>Handling Spotify callback...</p>;
}
