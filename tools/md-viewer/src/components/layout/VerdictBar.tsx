import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { type ParsedReview, type Story, type InvestHealth, INVEST_LABELS } from '@/lib/types';
import { INVEST_LETTERS } from '@/components/invest/invest-types';
import { criterionVerdict } from '@/components/invest/invest-utils';

// ── Props ──

interface VerdictBarProps {
  reviews: ParsedReview[];
  story: Story;
  totalAgents: number;
  investHealth?: InvestHealth[];
  onVerdict: (verdict: 'approve' | 'request-changes' | 'defer') => void;
}

// ── Component ──

export function VerdictBar({ reviews, story, totalAgents, investHealth = [], onVerdict }: VerdictBarProps) {
  const total = Math.max(totalAgents, 1);
  const reviewed = reviews.length;
  const blockers = reviews.reduce(
    (acc, r) => acc + r.findings.filter((f) => f.severity === 'blocker').length,
    0,
  );
  const concerns = reviews.reduce(
    (acc, r) => acc + r.findings.filter((f) => f.severity === 'concern').length,
    0,
  );

  const hasHealthData = investHealth.length > 0;
  const passCount = investHealth.filter(h => h.verdict === 'pass').length;
  const hasBlockers = blockers > 0;

  // Criteria details for enhanced display
  const failedCriteria = investHealth.filter(h => h.verdict === 'fail');
  const contestedCriteria = investHealth.filter(h => h.verdict === 'contested');

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

        {/* INVEST pills + criteria info */}
        <div className="flex items-center gap-2">
          <div className="flex gap-0.5">
            {INVEST_LETTERS.map(letter => {
              const health = investHealth.find(h => h.criterion === letter);
              const verdict = criterionVerdict(health);
              const pillStyle =
                verdict === 'pass' ? 'bg-approve/15 text-approve' :
                verdict === 'fail' ? 'bg-red-500/15 text-red-600' :
                verdict === 'contested' ? 'bg-amber-500/15 text-amber-600' :
                'bg-muted text-muted-foreground/50';

              return (
                <span
                  key={letter}
                  className={cn(
                    'w-5 h-5 rounded text-[9px] font-bold flex items-center justify-center',
                    pillStyle,
                  )}
                  title={`${INVEST_LABELS[letter] ?? letter}: ${verdict}`}
                >
                  {letter}
                </span>
              );
            })}
          </div>
          <span className="text-[12px] text-muted-foreground tabular-nums">
            {hasHealthData ? `${passCount}/6` : '0/6'}
          </span>

          {/* Failed criteria detail */}
          {failedCriteria.length > 0 && (
            <span className="text-[11px] text-red-600 font-medium">
              {failedCriteria.map(h => `[${h.criterion}]`).join('')} flagged
            </span>
          )}

          {/* Contested criteria detail */}
          {contestedCriteria.length > 0 && (
            <span className="text-[11px] text-amber-600 font-medium">
              {contestedCriteria.map(h => `[${h.criterion}]`).join('')} contested
            </span>
          )}
        </div>
      </div>

      {/* ── Right: Action Buttons ── */}
      <div className="flex items-center gap-2">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onVerdict('defer')}
        >
          Defer
        </Button>

        <Button
          variant="outline"
          size="sm"
          onClick={() => onVerdict('request-changes')}
          className="text-changes border-changes/30 hover:bg-changes/10 hover:text-changes"
        >
          Request Changes
        </Button>

        <Button
          size="sm"
          onClick={() => onVerdict('approve')}
          disabled={hasBlockers}
          className={cn(
            hasBlockers
              ? 'bg-approve/20 text-approve/40'
              : 'bg-approve text-white hover:bg-approve/90 shadow-sm',
          )}
          title={hasBlockers ? `Cannot approve with ${blockers} active blocker${blockers > 1 ? 's' : ''}` : undefined}
        >
          Approve
        </Button>
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
