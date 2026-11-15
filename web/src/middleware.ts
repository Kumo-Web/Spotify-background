import { NextResponse, type NextRequest } from 'next/server';

/**
 * Sets the Content Security Policy on every page.
 *
 * A CSP is a list, sent with the page, of where the browser may load things from. Its job is to
 * limit the damage if some text from outside ever does get onto the page as markup: a script
 * that is not on the list does not run, and a request to a host that is not on the list is not
 * made. It is a second line of defence. The first is that React escapes all text and nothing in
 * this codebase uses dangerouslySetInnerHTML.
 *
 * It lives in middleware, rather than with the fixed headers in next.config.ts, because it
 * carries a nonce: a random value made fresh for each request. Next stamps the same value on
 * its own <script> tags, and `script-src` then allows scripts bearing that nonce and nothing
 * else. An injected script cannot know the value, so it is refused.
 *
 * Note what this file does NOT do: decide who is signed in. A middleware check is easy to get
 * wrong and Next has shipped a bypass for one before (CVE-2025-29927). The API authorises every
 * request itself; nothing here needs to.
 */
export function middleware(request: NextRequest) {
  const nonce = Buffer.from(crypto.randomUUID()).toString('base64');
  const development = process.env.NODE_ENV !== 'production';

  const policy = [
    // Anything not named below falls back to this: same origin only.
    "default-src 'self'",
    // 'strict-dynamic' lets a script that carries the nonce load the chunks it needs.
    // 'unsafe-eval' is for the dev server's hot reloading only and is never sent in production.
    `script-src 'self' 'nonce-${nonce}' 'strict-dynamic'${development ? " 'unsafe-eval'" : ''}`,
    // The turntable positions itself with inline styles, so those have to be allowed.
    "style-src 'self' 'unsafe-inline'",
    // Artwork comes from Spotify's image CDN. Keep in step with safeImageUrl in lib/security.ts.
    "img-src 'self' data: https://*.scdn.co https://*.spotifycdn.com",
    // Fonts are served by this site (see next/font in app/layout.tsx).
    "font-src 'self'",
    // The API is reached through this site's own /api proxy, so nothing else is needed.
    `connect-src 'self'${development ? ' ws: wss:' : ''}`,
    "object-src 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    // Nobody may put this site in a frame: the defence against clickjacking.
    "frame-ancestors 'none'",
    ...(development ? [] : ['upgrade-insecure-requests']),
  ].join('; ');

  // Next reads the policy from the *request* headers to learn the nonce for its own scripts.
  const requestHeaders = new Headers(request.headers);
  requestHeaders.set('x-nonce', nonce);
  requestHeaders.set('Content-Security-Policy', policy);

  const response = NextResponse.next({ request: { headers: requestHeaders } });
  response.headers.set('Content-Security-Policy', policy);

  return response;
}

export const config = {
  matcher: [
    {
      // Pages only. /api is proxied to the .NET API, which sets its own stricter policy, and
      // static files are not documents, so a document policy does not apply to them.
      source: '/((?!api|_next/static|_next/image|favicon.ico).*)',
      // Link prefetches are not real page views; they do not need a fresh nonce.
      missing: [
        { type: 'header', key: 'next-router-prefetch' },
        { type: 'header', key: 'purpose', value: 'prefetch' },
      ],
    },
  ],
};
