import { useState } from 'react';
import { cn } from '@/lib/utils';
import {
  type ParsedReview,
  type Finding,
  type Artifact,
  verdictLabel,
  confidenceLevel,
} from '@/lib/types';

// ── Props ──

interface ReviewPanelProps {
  review: ParsedReview | null;
  onClose: () => void;
}

// ── Verdict styling ──

const VERDICT_STYLES: Record<string, string> = {
  approve:            'bg-approve/15 text-approve border-approve/30',
  object:             'bg-blocker/10 text-blocker border-blocker/30',
  'approve-with-notes': 'bg-changes/10 text-changes border-changes/30',
};

const SEVERITY_STYLES: Record<string, { badge: string; border: string }> = {
  blocker:     { badge: 'bg-blocker text-white', border: 'border-l-blocker' },
  concern:     { badge: 'bg-changes text-white', border: 'border-l-changes' },
  observation: { badge: 'bg-blue-500 text-white', border: 'border-l-blue-400' },
};

// ── Component ──

export function ReviewPanel({ review, onClose }: ReviewPanelProps) {
  const isOpen = review !== null;

  return (
    <>
      {/* Backdrop */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/20 z-40 transition-opacity"
          onClick={onClose}
        />
      )}

      {/* Panel */}
      <div
        className={cn(
          'fixed top-0 right-0 h-screen w-[520px] bg-card z-50 shadow-2xl border-l border-border',
          'transform transition-transform duration-300 ease-out',
          isOpen ? 'translate-x-0' : 'translate-x-full',
        )}
      >
        {review && <ReviewPanelContent review={review} onClose={onClose} />}
      </div>
    </>
  );
}

function ReviewPanelContent({ review, onClose }: { review: ParsedReview; onClose: () => void }) {
  const role = review.agent_display_name || review.agent_template_id;
  const dots = confidenceLevel(review.confidence);

  return (
    <div className="flex flex-col h-full">
      {/* ── Header ── */}
      <div className="flex-shrink-0 px-6 py-4 border-b border-border">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <h2 className="text-base font-semibold text-foreground truncate">{role}</h2>
            <div className="flex items-center gap-3 mt-1.5">
              {/* Verdict badge */}
              <span className={cn(
                'text-[11px] font-semibold px-2.5 py-0.5 rounded-full border',
                VERDICT_STYLES[review.verdict] ?? 'bg-muted text-muted-foreground border-border',
              )}>
                {verdictLabel(review.verdict)}
              </span>

              {/* Confidence dots */}
              <div className="flex items-center gap-1" title={`Confidence: ${review.confidence}`}>
                {Array.from({ length: 5 }, (_, i) => (
                  <span
                    key={i}
                    className={cn(
                      'w-1.5 h-1.5 rounded-full transition-colors',
                      i < dots ? 'bg-primary' : 'bg-border',
                    )}
                  />
                ))}
                <span className="text-[10px] text-muted-foreground ml-1 capitalize">
                  {review.confidence}
                </span>
              </div>

              {/* Round */}
              <span className="text-[10px] text-muted-foreground">
                Round {review.round}
              </span>
            </div>
          </div>

          <button
            onClick={onClose}
            className="flex-shrink-0 w-8 h-8 rounded-lg hover:bg-muted flex items-center justify-center text-muted-foreground hover:text-foreground transition-colors"
          >
            <span className="text-lg leading-none">{'\u2715'}</span>
          </button>
        </div>
      </div>

      {/* ── Scrollable Content ── */}
      <div className="flex-1 overflow-y-auto px-6 py-5 space-y-6">
        {/* Executive Summary */}
        {review.executive && (
          <div className="bg-primary/5 border border-primary/10 rounded-lg px-4 py-3">
            <h3 className="text-[10px] font-semibold uppercase tracking-wider text-primary mb-1.5">
              Executive Summary
            </h3>
            <p className="text-[13px] text-foreground leading-relaxed">
              {review.executive}
            </p>
          </div>
        )}

        {/* INVEST Scores */}
        {review.investScores && Object.keys(review.investScores).length > 0 && (
          <InvestScoresSection scores={review.investScores} />
        )}

        {/* Findings */}
        {review.findings.length > 0 && (
          <div>
            <h3 className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
              Findings ({review.findings.length})
            </h3>
            <div className="space-y-2">
              {review.findings.map((finding, i) => (
                <FindingCard key={i} finding={finding} />
              ))}
            </div>
          </div>
        )}

        {/* Artifacts */}
        {review.artifacts.length > 0 && (
          <div>
            <h3 className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground mb-3">
              Artifacts ({review.artifacts.length})
            </h3>
            <div className="space-y-3">
              {review.artifacts.map((artifact, i) => (
                <ArtifactCard key={i} artifact={artifact} />
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// ── Finding Card ──

function FindingCard({ finding }: { finding: Finding }) {
  const defaultOpen = finding.severity === 'blocker';
  const [open, setOpen] = useState(defaultOpen);
  const styles = SEVERITY_STYLES[finding.severity] ?? SEVERITY_STYLES.observation;

  return (
    <div className={cn('border border-border rounded-lg overflow-hidden border-l-[3px]', styles.border)}>
      <button
        onClick={() => setOpen(!open)}
        className="w-full text-left px-4 py-2.5 flex items-center gap-3 hover:bg-muted/30 transition-colors"
      >
        <span className={cn('text-[9px] font-bold uppercase px-1.5 py-0.5 rounded', styles.badge)}>
          {finding.severity}
        </span>
        <span className="text-[13px] font-medium text-foreground flex-1 truncate">
          {finding.title}
        </span>
        <span className={cn(
          'text-muted-foreground text-xs transition-transform duration-200',
          open ? 'rotate-90' : '',
        )}>
          {'\u25B6'}
        </span>
      </button>

      {open && (
        <div className="px-4 pb-4 space-y-3">
          {/* Description */}
          <p className="text-[12px] text-foreground/80 leading-relaxed">
            {finding.text}
          </p>

          {/* Evidence */}
          {finding.evidence && (
            <div className="bg-sidebar rounded-md px-3 py-2.5">
              <div className="text-[9px] font-semibold uppercase tracking-wider text-sidebar-muted mb-1">
                Evidence
              </div>
              <p className="text-[11px] text-sidebar-foreground font-mono leading-relaxed whitespace-pre-wrap">
                {finding.evidence}
              </p>
            </div>
          )}

          {/* Recommendation */}
          {finding.recommendation && (
            <div className="flex gap-2 items-start">
              <span className="text-primary text-xs mt-0.5">{'\u2794'}</span>
              <p className="text-[12px] text-foreground/70 leading-relaxed">
                {finding.recommendation}
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// ── INVEST Scores ──

function InvestScoresSection({ scores }: { scores: Record<string, { pass: boolean; reasoning: string }> }) {
  const labels: Record<string, string> = {
    independent: 'I', negotiable: 'N', valuable: 'V',
    estimable: 'E', small: 'S', testable: 'T',
  };

  return (
    <div>
      <h3 className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground mb-2">
        INVEST Evaluation
      </h3>
      <div className="grid grid-cols-6 gap-1.5">
        {Object.entries(scores).map(([key, { pass }]) => (
          <div
            key={key}
            className={cn(
              'text-center py-1.5 rounded-md text-[11px] font-bold',
              pass
                ? 'bg-approve/10 text-approve border border-approve/20'
                : 'bg-blocker/10 text-blocker border border-blocker/20',
            )}
            title={`${key}: ${pass ? 'Pass' : 'Fail'}`}
          >
            {labels[key] ?? key[0].toUpperCase()}
          </div>
        ))}
      </div>
    </div>
  );
}

// ── Artifact Card ──

function ArtifactCard({ artifact }: { artifact: Artifact }) {
  return (
    <div className="rounded-lg overflow-hidden border border-border">
      <div className="flex items-center justify-between px-3 py-2 bg-muted/50 border-b border-border">
        <div className="flex items-center gap-2">
          <span className="text-[9px] font-bold uppercase px-1.5 py-0.5 rounded bg-primary/10 text-primary">
            {artifact.kind}
          </span>
          <span className="text-[12px] font-medium text-foreground">
            {artifact.label}
          </span>
        </div>
      </div>
      <div className="bg-sidebar overflow-x-auto">
        <pre className="px-4 py-3 text-[11px] text-sidebar-foreground font-mono leading-relaxed whitespace-pre-wrap">
          {artifact.content}
        </pre>
      </div>
    </div>
  );
}
