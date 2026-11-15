interface StatusScreenProps {
  title: string;
  message?: string;
  /** Buttons or links to offer, if there is something the visitor can do. */
  children?: React.ReactNode;
}

/** A full page with one message on it: loading, not found, something failed. */
export default function StatusScreen({ title, message, children }: StatusScreenProps) {
  return (
    <main className="mx-auto flex min-h-dvh max-w-md flex-col justify-center gap-4 px-4">
      <h1 className="font-display text-3xl font-bold tracking-tight">{title}</h1>
      {message && (
        <p role="alert" className="text-sm leading-relaxed text-muted">
          {message}
        </p>
      )}
      {children && <div className="mt-2 flex flex-wrap items-center gap-4">{children}</div>}
    </main>
  );
}
