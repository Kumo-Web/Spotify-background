import type { NextConfig } from "next";

// Where /api/* is forwarded to. Server-side only — the browser never sees this address.
const apiProxyTarget = process.env.API_PROXY_TARGET ?? "https://localhost:7249";

// Sent with every response. The Content Security Policy is not here: it needs a value that
// changes on each request, so it is set in src/middleware.ts.
const securityHeaders = [
  // Stop browsers second-guessing declared content types.
  { key: "X-Content-Type-Options", value: "nosniff" },
  // Older browsers' version of the CSP's frame-ancestors rule.
  { key: "X-Frame-Options", value: "DENY" },
  // Never tell another site which page a visitor came from.
  { key: "Referrer-Policy", value: "no-referrer" },
  // This site uses none of these device features, so no script on it may ask for them.
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=(), payment=(), usb=()" },
  // A window this site opens, or is opened by, gets no handle back to it.
  { key: "Cross-Origin-Opener-Policy", value: "same-origin" },
  // HTTPS only from the first visit on. Left off in development, where the site is plain HTTP
  // on localhost and a remembered "always use HTTPS" would lock the browser out of it.
  ...(process.env.NODE_ENV === "production"
    ? [{ key: "Strict-Transport-Security", value: "max-age=31536000; includeSubDomains" }]
    : []),
];

const nextConfig: NextConfig = {
  // Do not announce the framework in an "X-Powered-By" header. It helps nobody but scanners.
  poweredByHeader: false,

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

  async headers() {
    return [{ source: "/:path*", headers: securityHeaders }];
  },
};

export default nextConfig;
