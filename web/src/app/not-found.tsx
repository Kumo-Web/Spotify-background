import Link from 'next/link';
import StatusScreen from '@/components/status-screen';

export default function NotFound() {
  return (
    <StatusScreen title="Nothing on this shelf" message="That page does not exist.">
      <Link href="/" className="rounded-full bg-accent px-6 py-3 font-semibold text-on-accent">
        Back to the turntable
      </Link>
    </StatusScreen>
  );
}
