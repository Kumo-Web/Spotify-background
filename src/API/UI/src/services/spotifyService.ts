export const getAuthUrl = async (): Promise<string> => {
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifAuth`);
  return await res.text();
};

export const exchangeCode = async (code: string): Promise<any> => {
  const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/spotifAuth/callback?code=${code}`);
  return await res.json();
};
