'use client';
import { useState } from 'react';
import useSpotify  from '@/hooks/useSpotify';

const Login = () => {
    const { login, loading } = useSpotify();

    return ( <main>
      <h1>Login to Spotify</h1>
      <button onClick={login} disabled={loading}>
        {loading ? 'Redirecting...' : 'Login with Spotify'}
      </button>
    </main>
  );
}
 
export default Login;