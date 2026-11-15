import { redirect } from 'next/navigation';

/**
 * The old landing place after login. The home page took its job.
 *
 * Kept as a redirect rather than removed because the API still falls back to "/dashboard" when
 * a login carries no return path (AuthController.Callback). Without this route that fallback
 * would land on a 404 straight after a successful login.
 */
export default function DashboardRedirect() {
  redirect('/');
}
