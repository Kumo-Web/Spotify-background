import type { NextConfig } from "next";

// Where /api/* is forwarded to. Server-side only — the browser never sees this address.
const apiProxyTarget = process.env.API_PROXY_TARGET ?? "https://localhost:7249";

const nextConfig: NextConfig = {
  // Serve the API from the frontend's own origin. The session cookie is set by whichever host
  // answers /api/auth/callback, and it is SameSite=Lax, so if the browser talked to the API on a
  // different host (localhost:7249 vs an ngrok URL, say) the cookie would never be sent back and
  // every call after login would 401. Proxying keeps one host for the whole OAuth round trip.
  //
  // The target is HTTPS so the API's UseHttpsRedirection leaves these requests alone; Node has
  // to trust the .NET dev certificate for that, which `pnpm dev` arranges via
  // NODE_EXTRA_CA_CERTS (see `pnpm certs`).
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${apiProxyTarget}/api/:path*` }];
  },
};

export default nextConfig;
