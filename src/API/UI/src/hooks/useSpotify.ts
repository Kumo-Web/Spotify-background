import { useState } from 'react';
import { getAuthUrl } from '../services/spotifyService';

 const useSpotify = () => {
  const [loading, setLoading] = useState(false);

  const login = async () => {
    setLoading(true);
    const url = await getAuthUrl();
    window.location.href = url;
  };

  return { login, loading };
};

export default useSpotify;