'use client';
import { useSearchParams, useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { exchangeCode } from '../../services/spotifyService';

const Callback = () => {
  const searchParams = useSearchParams();
  const router = useRouter();
  const code = searchParams.get('code');

  useEffect(() => {
    if (code) {
      debugger;
      exchangeCode(code).then(() => {
        router.push('/dashboard'); 
      });
    }
  }, [code]);

  return <p>Handling Spotify callback...</p>;
}

export default Callback;