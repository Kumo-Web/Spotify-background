import Header from '@/components/header';
import RequireSession from '@/features/auth/require-session';

/**
 * Everything inside the (signed-in) folder needs a session and gets the header.
 *
 * The brackets make this a route group: the folder organises files and shares this layout, but
 * adds nothing to the URL, so (signed-in)/playlists is simply /playlists. The log in page sits
 * outside the group, which is how it gets neither the session check nor the menu.
 */
export default function SignedInLayout({ children }: { children: React.ReactNode }) {
  return (
    <RequireSession>
      <Header />
      {children}
    </RequireSession>
  );
}
