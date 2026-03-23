import { useMemo } from 'react';
import { cn } from '@/lib/utils';
import { VerdictBadge } from '@/components/ui/badges';
import { Card } from '@/components/ui/card';
import { SectionHeading } from '@/components/ui/section-heading';
import { AlertTriangle } from 'lucide-react';
import type { Review, Story, ParsedReview } from '@/lib/types';
import {
  parseReview,
  verdictLabel,
  confidenceLevel,
} from '@/lib/types';

// ── Confidence dots ──

function ConfidenceDots({ level }: { level: number }) {
  return (
    <span className="inline-flex gap-0.5" title={`Confidence: ${level}/5`}>
      {Array.from({ length: 5 }, (_, i) => (
        <span
          key={i}
          className={cn(
            'w-1.5 h-1.5 rounded-full',
            i < level ? 'bg-primary' : 'bg-border',
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

  // Group reviews by round number
  const roundGroups = useMemo(() => {
    const groups = new Map<number, typeof parsed>();
    for (const r of parsed) {
      const round = r.round ?? 1;
      if (!groups.has(round)) groups.set(round, []);
      groups.get(round)!.push(r);
    }
    return Array.from(groups.entries()).sort(([a], [b]) => a - b);
  }, [parsed]);

  // Collect all blockers across all reviews
  const blockers = useMemo(() => {
    const items: { agent: string; finding: { title: string; text: string } }[] = [];
    for (const r of parsed) {
      const agentName = r.agent_display_name || r.agent_template_id;
      for (const f of r.findings ?? []) {
        if (f.severity === 'blocker') {
          items.push({ agent: agentName, finding: f });
        }
      }
    }
    return items;
  }, [parsed]);

  return (
    <div className="space-y-8">
      {roundGroups.map(([round, roundParsed]) => {
        const approveCount = roundParsed.filter(
          (r) => r.verdict === 'approve' || r.verdict === 'approve-with-notes',
        ).length;
        const objectCount = roundParsed.filter((r) => r.verdict === 'object').length;
        const total = roundParsed.length;
        const totalAgents = Math.max(total, 1);
        const approvePct = total > 0 ? (approveCount / totalAgents) * 100 : 0;
        const { label: strengthLabel, color: strengthColor } = consensusStrength(approveCount, total);

        return (
          <div key={round} className="space-y-6">
            {/* Section title */}
            <div>
              <SectionHeading>Agent Review &mdash; Round {round}</SectionHeading>
              {round === 2 && (
                <p className="text-xs text-muted-foreground -mt-3 mb-2">Challenge Round</p>
              )}
            </div>

            {/* Consensus bar */}
            <div className="space-y-2">
              <div className="h-2 w-full rounded-full bg-muted overflow-hidden flex">
                <div
                  className="h-full bg-approve transition-all duration-500"
                  style={{ width: `${approvePct}%` }}
                />
                {objectCount > 0 && (
                  <div
                    className="h-full bg-changes transition-all duration-500"
                    style={{ width: `${(objectCount / totalAgents) * 100}%` }}
                  />
                )}
              </div>
              <div className="flex items-center justify-between text-xs">
                <span className="text-muted-foreground">
                  {approveCount} approve, {objectCount} object &mdash; {total}/{totalAgents} agents reviewed
                </span>
                <span className={cn('font-bold uppercase tracking-wider text-[10px]', strengthColor)}>
                  {strengthLabel}
                </span>
              </div>
            </div>

            {/* Agent cards grid */}
            <div className="grid grid-cols-2 lg:grid-cols-3 gap-3">
              {roundParsed.map((review) => {
                const roleName = review.agent_display_name || review.agent_template_id;
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
                    className="text-left focus:outline-none focus:ring-2 focus:ring-ring rounded-xl"
                  >
                    <Card className="h-full transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md hover:ring-primary/30 hover:ring-1">
                      <div className="px-4 py-3.5 space-y-2.5">
                        {/* Role + verdict */}
                        <div className="flex items-start justify-between gap-2">
                          <span className="text-sm font-semibold text-foreground leading-snug">
                            {roleName}
                          </span>
                          <VerdictBadge verdict={review.verdict} label={verdictLabel(review.verdict)} />
                        </div>

                        {/* Counts */}
                        <div className="flex items-center gap-3 text-[11px] text-muted-foreground">
                          {blockerCount > 0 && (
                            <span className="flex items-center gap-1 text-blocker font-medium">
                              <span className="w-1.5 h-1.5 rounded-full bg-blocker" />
                              {blockerCount} blocker{blockerCount !== 1 ? 's' : ''}
                            </span>
                          )}
                          {concernCount > 0 && (
                            <span className="flex items-center gap-1 text-changes font-medium">
                              <span className="w-1.5 h-1.5 rounded-full bg-changes" />
                              {concernCount} concern{concernCount !== 1 ? 's' : ''}
                            </span>
                          )}
                          {blockerCount === 0 && concernCount === 0 && (
                            <span className="text-approve font-medium">No issues</span>
                          )}
                        </div>

                        {/* Confidence */}
                        <div className="flex items-center gap-2">
                          <span className="text-[10px] text-muted-foreground uppercase tracking-wider">
                            Confidence
                          </span>
                          <ConfidenceDots level={confidenceLevel(review.confidence)} />
                        </div>
                      </div>
                    </Card>
                  </button>
                );
              })}
            </div>
          </div>
        );
      })}

      {/* Attention section (blockers only) */}
      {blockers.length > 0 && (
        <div className="space-y-3">
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-3.5 w-3.5 text-blocker" />
            <h3 className="section-heading !mb-0 !text-blocker">Requires Attention</h3>
          </div>
          <div className="space-y-2">
            {blockers.map((item, idx) => (
              <Card key={idx} className="border-l-[3px] border-l-blocker bg-blocker-light/30">
                <div className="px-4 py-3 flex items-start gap-3">
                  <div className="min-w-0">
                    <span className="text-xs font-semibold text-blocker">
                      {item.agent}
                    </span>
                    <div className="text-sm font-medium text-foreground mt-0.5">
                      {item.finding.title}
                    </div>
                    <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">
                      {item.finding.text}
                    </p>
                  </div>
                </div>
              </Card>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
