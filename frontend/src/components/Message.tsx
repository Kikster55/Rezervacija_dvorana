interface MessageProps {
  text: string | null;
  tone?: 'error' | 'success';
}

/** Kratka poruka iznad sadržaja - greška ili potvrda uspjeha. */
export function Message({ text, tone = 'error' }: MessageProps) {
  if (!text) {
    return null;
  }

  return <div className={`message message-${tone}`}>{text}</div>;
}
