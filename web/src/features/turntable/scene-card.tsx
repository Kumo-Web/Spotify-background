interface SceneCardProps {
  eyebrow: string;
  title: string;
  children?: React.ReactNode;
}

/**
 * The words inside the empty sleeve: a small label, a headline, then whatever follows.
 *
 * One component so that "log in", "nothing playing" and "something failed" all read the same
 * way. The page has a single <h1> at a time, and this is it whenever a card is showing.
 */
export default function SceneCard({ eyebrow, title, children }: SceneCardProps) {
  return (
    <>
      <p className="text-[11px] font-semibold uppercase tracking-[0.08em] text-muted">{eyebrow}</p>
      <h1 className="font-display text-[34px] font-bold leading-[1.08] tracking-tight sm:text-[52px]">
        {title}
      </h1>
      {children}
    </>
  );
}
