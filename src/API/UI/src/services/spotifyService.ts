export const getAuthUrl = async (): Promise<string> => {
 
  const userId = "6a6f528b-9ca5-4668-9144-dbe3221cd27f"
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifyAuth/login`, {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify(userId ),
});

const authUrl = await res.text();
  console.log("url", authUrl);
  return authUrl;
};

export const exchangeCode = async (code: string): Promise<any> => {
  debugger;
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifyAuth/callback?code=${code}`);
  return await res.json();
};
