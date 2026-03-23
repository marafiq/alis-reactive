import { cn } from '@/lib/utils';
import { type ParsedReview, type Story, investScores, INVEST_LABELS } from '@/lib/types';

// ── Props ──

interface VerdictBarProps {
  reviews: ParsedReview[];
  story: Story;
  onVerdict: (verdict: 'approve' | 'request-changes' | 'defer') => void;
}

// ── Component ──

export function VerdictBar({ reviews, story, onVerdict }: VerdictBarProps) {
  const total = 6; // total agent roles
  const reviewed = reviews.length;
  const blockers = reviews.reduce(
    (acc, r) => acc + r.findings.filter((f) => f.severity === 'blocker').length,
    0,
  );
  const concerns = reviews.reduce(
    (acc, r) => acc + r.findings.filter((f) => f.severity === 'concern').length,
    0,
  );
  const scores = investScores(story);
  const investCount = Object.values(scores).filter(Boolean).length;
  const hasBlockers = blockers > 0;

  return (
    <div className="fixed bottom-0 left-[260px] right-0 h-[72px] bg-card border-t border-border z-30 px-6 flex items-center justify-between">
      {/* ── Left: Stats ── */}
      <div className="flex items-center gap-5">
        {/* Review progress */}
        <div className="flex items-center gap-2">
          <div className="flex gap-0.5">
            {Array.from({ length: total }, (_, i) => (
              <span
                key={i}
                className={cn(
                  'w-2 h-2 rounded-full',
                  i < reviewed ? 'bg-primary' : 'bg-border',
                )}
              />
            ))}
          </div>
          <span className="text-[12px] text-muted-foreground tabular-nums">
            {reviewed}/{total} reviewed
          </span>
        </div>

        {/* Divider */}
        <span className="w-px h-5 bg-border" />

        {/* Blockers */}
        <Stat
          label="Blockers"
          value={blockers}
          color={blockers > 0 ? 'text-blocker' : 'text-muted-foreground'}
        />

        {/* Concerns */}
        <Stat
          label="Concerns"
          value={concerns}
          color={concerns > 0 ? 'text-changes' : 'text-muted-foreground'}
        />

        {/* Divider */}
        <span className="w-px h-5 bg-border" />

        {/* INVEST */}
        <div className="flex items-center gap-1.5">
          <div className="flex gap-0.5">
            {Object.entries(scores).map(([letter, pass]) => (
              <span
                key={letter}
                className={cn(
                  'w-5 h-5 rounded text-[9px] font-bold flex items-center justify-center',
                  pass
                    ? 'bg-approve/15 text-approve'
                    : 'bg-muted text-muted-foreground/50',
                )}
                title={`${INVEST_LABELS[letter] ?? letter}: ${pass ? 'Pass' : 'Not validated'}`}
              >
                {letter}
              </span>
            ))}
          </div>
          <span className="text-[12px] text-muted-foreground tabular-nums">
            {investCount}/6
          </span>
        </div>
      </div>

      {/* ── Right: Action Buttons ── */}
      <div className="flex items-center gap-2">
        {/* Defer */}
        <button
          onClick={() => onVerdict('defer')}
          className="px-4 py-2 rounded-lg text-[12px] font-medium text-muted-foreground bg-muted hover:bg-muted/80 transition-colors"
        >
          Defer
        </button>

        {/* Request Changes */}
        <button
          onClick={() => onVerdict('request-changes')}
          className="px-4 py-2 rounded-lg text-[12px] font-medium text-changes border border-changes/30 hover:bg-changes/10 transition-colors"
        >
          Request Changes
        </button>

        {/* Approve */}
        <button
          onClick={() => onVerdict('approve')}
          disabled={hasBlockers}
          className={cn(
            'px-5 py-2 rounded-lg text-[12px] font-semibold transition-all',
            hasBlockers
              ? 'bg-approve/20 text-approve/40 cursor-not-allowed'
              : 'bg-approve text-white hover:bg-approve/90 shadow-sm hover:shadow-md',
          )}
          title={hasBlockers ? `Cannot approve with ${blockers} active blocker${blockers > 1 ? 's' : ''}` : undefined}
        >
          Approve
        </button>
      </div>
    </div>
  );
}

// ── Stat chip ──

function Stat({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div className="flex items-center gap-1.5">
      <span className={cn('text-[14px] font-bold tabular-nums', color)}>{value}</span>
      <span className="text-[11px] text-muted-foreground">{label}</span>
    </div>
  );
}
