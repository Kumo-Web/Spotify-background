import type { Metadata, Viewport } from "next";
import { headers } from "next/headers";
import { Bricolage_Grotesque, Inter } from "next/font/google";
import "./globals.css";
import { SessionProvider } from "@/features/auth/session-provider";
import { SITE_NAME } from "@/lib/site";

// next/font downloads these at build time and serves them from this site, so no visitor's
// browser ever contacts Google — which is also why the CSP can keep `font-src 'self'`.
const displayFont = Bricolage_Grotesque({
  variable: "--font-display",
  subsets: ["latin"],
  display: "swap",
});

const bodyFont = Inter({
  variable: "--font-body",
  subsets: ["latin"],
  display: "swap",
});

export const metadata: Metadata = {
  title: { default: SITE_NAME, template: `%s · ${SITE_NAME}` },
  description: "A record player for what you are listening to on Spotify.",
  // A personal site showing one person's listening. Nothing here is for search engines.
  robots: { index: false, follow: false },
};

export const viewport: Viewport = {
  themeColor: "#0d0f14",
  colorScheme: "dark",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  // Reading the request makes every page render per request instead of once at build time.
  // That is required, not incidental: the CSP nonce set in middleware.ts only reaches Next's
  // own <script> tags on a page rendered for that request. A page prerendered at build time
  // carries no nonce, so the browser would refuse to run its scripts.
  await headers();

  return (
    <html lang="en">
      <body className={`${displayFont.variable} ${bodyFont.variable} font-body antialiased`}>
        <SessionProvider>{children}</SessionProvider>
      </body>
    </html>
  );
}
