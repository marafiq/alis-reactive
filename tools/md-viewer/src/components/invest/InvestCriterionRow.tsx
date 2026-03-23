import { useState } from 'react';
import { ChevronRight } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { CriterionVerdict, EvidenceQuality } from './invest-types';
import { CRITERION_DISPLAY } from './invest-types';
import { DisagreementBanner } from './DisagreementBanner';
import { AgentFeedbackEntry } from './AgentFeedbackEntry';

interface AgentFeedback {
  agentName: string;
  pass: boolean;
  reasoning: string;
  evidenceQuality: EvidenceQuality;
}

interface InvestCriterionRowProps {
  letter: string;
  verdict: CriterionVerdict;
  authorStatement: string | null;
  agentFeedback: AgentFeedback[];
  defaultExpanded?: boolean;
}

const PILL_STYLES: Record<CriterionVerdict, string> = {
  pass: 'bg-emerald-500 text-white',
  fail: 'bg-red-500 text-white',
  contested: 'bg-amber-500 text-white border border-dashed border-amber-300',
  pending: 'bg-zinc-200 text-zinc-400',
};

export function InvestCriterionRow({
  letter,
  verdict,
  authorStatement,
  agentFeedback,
  defaultExpanded,
}: InvestCriterionRowProps) {
  const shouldDefaultExpand = defaultExpanded ?? (verdict === 'fail' || verdict === 'contested');
  const [expanded, setExpanded] = useState(shouldDefaultExpand);

  const display = CRITERION_DISPLAY[letter] ?? { name: letter, description: '' };

  const passCount = agentFeedback.filter(f => f.pass).length;
  const failCount = agentFeedback.filter(f => !f.pass).length;

  return (
    <div>
      {/* Header — always visible, clickable */}
      <button
        type="button"
        onClick={() => setExpanded(prev => !prev)}
        className="w-full flex items-center gap-3 px-4 py-3 text-left hover:bg-muted/50 transition-colors"
      >
        {/* Letter pill */}
        <span
          className={cn(
            'w-6 h-6 rounded text-xs font-bold flex items-center justify-center shrink-0',
            PILL_STYLES[verdict],
          )}
        >
          {letter}
        </span>

        {/* Name + description */}
        <div className="flex-1 min-w-0">
          <span className="text-sm font-medium text-foreground">{display.name}</span>
          <span className="text-xs text-muted-foreground ml-2">{display.description}</span>
        </div>

        {/* Chevron */}
        <ChevronRight
          className={cn(
            'w-4 h-4 text-muted-foreground shrink-0 transition-transform',
            expanded && 'rotate-90',
          )}
        />
      </button>

      {/* Body — collapsible */}
      {expanded && (
        <div className="px-4 pb-4 space-y-3">
          {/* Author statement */}
          {authorStatement ? (
            <blockquote className="border-l-2 border-zinc-300 pl-3 text-sm italic text-foreground/80">
              {authorStatement}
            </blockquote>
          ) : (
            <p className="text-sm text-muted-foreground italic">(no statement provided)</p>
          )}

          {/* Disagreement banner */}
          {verdict === 'contested' && (
            <DisagreementBanner
              passCount={passCount}
              failCount={failCount}
              criterionName={display.name}
            />
          )}

          {/* Agent feedback list */}
          {agentFeedback.length > 0 ? (
            <div className="space-y-2">
              {agentFeedback.map((fb, idx) => {
                // Agent is outlier if their verdict disagrees with the majority
                const majorityPass = passCount > failCount;
                const isOutlier = agentFeedback.length > 1 && fb.pass !== majorityPass;

                return (
                  <AgentFeedbackEntry
                    key={idx}
                    agentName={fb.agentName}
                    pass={fb.pass}
                    reasoning={fb.reasoning}
                    evidenceQuality={fb.evidenceQuality}
                    isOutlier={isOutlier}
                  />
                );
              })}
            </div>
          ) : (
            <p className="text-sm text-muted-foreground">No agent feedback yet</p>
          )}
        </div>
      )}
    </div>
  );
}
