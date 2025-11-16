'use client'
import useSpotify from '@/hooks/useSpotify';
import { getUserInfo } from '@/services/spotifyService';
import React, { useEffect } from 'react'

function Dashboard() {
//  [setUserInfo, userInfo] = React.useState(null);  
  const {loading } = useSpotify();

  useEffect(() => {
    debugger;
    getUserInfo("6a6f528b-9ca5-4668-9144-dbe3221cd27f").then((data) => {
      console.log("user info", data);
    });
  }, []);
  return (
    <div>Here is the Dashboard</div>
  )
}

export default Dashboard