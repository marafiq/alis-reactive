import { useMemo } from 'react';
import { cn } from '@/lib/utils';
import type { Review, Story, ParsedReview } from '@/lib/types';
import {
  parseReview,
  ROLE_NAMES,
  verdictLabel,
  confidenceLevel,
  type AgentRole,
} from '@/lib/types';

// ── Verdict colors ──

const VERDICT_STYLES: Record<string, string> = {
  approve: 'bg-approve-light text-approve',
  object: 'bg-blocker-light text-blocker',
  'approve-with-notes': 'bg-changes-light text-changes',
};

// ── Confidence dots ──

function ConfidenceDots({ level }: { level: number }) {
  return (
    <span className="inline-flex gap-0.5" title={`Confidence: ${level}/5`}>
      {Array.from({ length: 5 }, (_, i) => (
        <span
          key={i}
          className={cn(
            'w-1.5 h-1.5 rounded-full',
            i < level ? 'bg-primary' : 'bg-muted',
          )}
        />
      ))}
    </span>
  );
}

// ── Consensus strength ──

function consensusStrength(approvals: number, total: number): { label: string; color: string } {
  if (total === 0) return { label: 'PENDING', color: 'text-muted-foreground' };
  const pct = approvals / total;
  if (pct >= 0.8) return { label: 'STRONG', color: 'text-approve' };
  if (pct >= 0.5) return { label: 'MODERATE', color: 'text-changes' };
  return { label: 'WEAK', color: 'text-blocker' };
}

// ── ReviewSection ──

interface ReviewSectionProps {
  reviews: Review[];
  story: Story;
  onSelectReview: (review: ParsedReview) => void;
  onVerdict?: (verdict: string) => void;
}

export function ReviewSection({
  reviews,
  story: _story,
  onSelectReview,
  onVerdict: _onVerdict,
}: ReviewSectionProps) {
  const parsed = useMemo(
    () => reviews.map((r) => parseReview(r)),
    [reviews],
  );

  const round = parsed.length > 0 ? parsed[0].round : 1;

  const approveCount = parsed.filter(
    (r) => r.verdict === 'approve' || r.verdict === 'approve-with-notes',
  ).length;
  const objectCount = parsed.filter((r) => r.verdict === 'object').length;
  const total = parsed.length;
  const { label: strengthLabel, color: strengthColor } = consensusStrength(approveCount, total);

  // Collect all blockers across reviews
  const blockers = useMemo(() => {
    const items: { agent: string; finding: { title: string; text: string } }[] = [];
    for (const r of parsed) {
      const roleName = ROLE_NAMES[r.agent_role as AgentRole] || r.agent_role;
      for (const f of r.findings ?? []) {
        if (f.severity === 'blocker') {
          items.push({ agent: roleName, finding: f });
        }
      }
    }
    return items;
  }, [parsed]);

  const approvePct = total > 0 ? (approveCount / 6) * 100 : 0;

  return (
    <div className="space-y-6">
      {/* Section title */}
      <h2 className="text-xs font-bold uppercase tracking-[0.15em] text-muted-foreground">
        Agent Review &mdash; Round {round}
      </h2>

      {/* Consensus bar */}
      <div className="space-y-2">
        <div className="h-3 w-full rounded-full bg-muted overflow-hidden flex">
          <div
            className="h-full bg-approve transition-all duration-500"
            style={{ width: `${approvePct}%` }}
          />
          {objectCount > 0 && (
            <div
              className="h-full bg-changes transition-all duration-500"
              style={{ width: `${(objectCount / 6) * 100}%` }}
            />
          )}
        </div>
        <div className="flex items-center justify-between text-xs">
          <span className="text-muted-foreground">
            {approveCount} approve, {objectCount} object &mdash; {total}/6 agents reviewed
          </span>
          <span className={cn('font-bold uppercase tracking-wider text-[10px]', strengthColor)}>
            {strengthLabel}
          </span>
        </div>
      </div>

      {/* Agent cards grid */}
      <div className="grid grid-cols-2 lg:grid-cols-3 gap-3">
        {parsed.map((review) => {
          const roleName = ROLE_NAMES[review.agent_role as AgentRole] || review.agent_role;
          const blockerCount = (review.findings ?? []).filter(
            (f) => f.severity === 'blocker',
          ).length;
          const concernCount = (review.findings ?? []).filter(
            (f) => f.severity === 'concern',
          ).length;

          return (
            <button
              key={review.id}
              onClick={() => onSelectReview(review)}
              className={cn(
                'text-left border border-border rounded-lg p-3.5 space-y-2.5',
                'bg-card hover:shadow-md hover:-translate-y-0.5 transition-all duration-150',
                'cursor-pointer focus:outline-none focus:ring-2 focus:ring-ring',
              )}
            >
              {/* Role + verdict */}
              <div className="flex items-start justify-between gap-2">
                <span className="text-sm font-semibold text-foreground leading-snug">
                  {roleName}
                </span>
                <span
                  className={cn(
                    'shrink-0 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider',
                    VERDICT_STYLES[review.verdict] ?? 'bg-muted text-muted-foreground',
                  )}
                >
                  {verdictLabel(review.verdict)}
                </span>
              </div>

              {/* Counts */}
              <div className="flex items-center gap-3 text-[11px] text-muted-foreground">
                {blockerCount > 0 && (
                  <span className="flex items-center gap-1 text-blocker">
                    <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor">
                      <circle cx="8" cy="8" r="8" opacity="0.15" />
                      <path d="M8 4v5M8 11h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                    </svg>
                    {blockerCount} blocker{blockerCount !== 1 ? 's' : ''}
                  </span>
                )}
                {concernCount > 0 && (
                  <span className="flex items-center gap-1 text-changes">
                    <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor">
                      <circle cx="8" cy="8" r="8" opacity="0.15" />
                      <path d="M8 5v4M8 11h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                    </svg>
                    {concernCount} concern{concernCount !== 1 ? 's' : ''}
                  </span>
                )}
                {blockerCount === 0 && concernCount === 0 && (
                  <span className="text-approve">No issues</span>
                )}
              </div>

              {/* Confidence */}
              <div className="flex items-center gap-2">
                <span className="text-[10px] text-muted-foreground uppercase tracking-wider">
                  Confidence
                </span>
                <ConfidenceDots level={confidenceLevel(review.confidence)} />
              </div>
            </button>
          );
        })}
      </div>

      {/* Attention section (blockers only) */}
      {blockers.length > 0 && (
        <div className="space-y-2">
          <h3 className="text-xs font-bold uppercase tracking-[0.12em] text-blocker flex items-center gap-1.5">
            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" className="shrink-0">
              <path d="M8 1l7 14H1L8 1z" fill="none" stroke="currentColor" strokeWidth="1.2" />
              <path d="M8 6v4M8 12h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
            </svg>
            Requires Attention
          </h3>
          <div className="space-y-2">
            {blockers.map((item, idx) => (
              <div
                key={idx}
                className="flex items-start gap-3 rounded-lg bg-blocker-light/50 border border-blocker/20 p-3"
              >
                <svg
                  width="16"
                  height="16"
                  viewBox="0 0 16 16"
                  fill="none"
                  className="shrink-0 mt-0.5 text-blocker"
                >
                  <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5" />
                  <path d="M8 4.5v4M8 11h.01" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                </svg>
                <div className="min-w-0">
                  <div className="flex items-center gap-2 mb-0.5">
                    <span className="text-xs font-semibold text-blocker">
                      {item.agent}
                    </span>
                  </div>
                  <div className="text-sm font-medium text-foreground">
                    {item.finding.title}
                  </div>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                    {item.finding.text}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
