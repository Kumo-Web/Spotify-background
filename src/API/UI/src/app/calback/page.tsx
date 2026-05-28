'use client';
import { useSearchParams, useRouter } from 'next/navigation';
import { Suspense, useEffect } from 'react';
import { exchangeCode } from '../../services/spotifyService';

const CallbackInner = () => {
  const searchParams = useSearchParams();
  const router = useRouter();
  const code = searchParams.get('code');

  useEffect(() => {
    if (code) {
      exchangeCode(code).then(() => {
        router.push('/dashboard');
      });
    }
  }, [code, router]);

  return <p>Handling Spotify callback...</p>;
}

const Callback = () => (
  <Suspense fallback={<p>Handling Spotify callback...</p>}>
    <CallbackInner />
  </Suspense>
);

export default Callback;