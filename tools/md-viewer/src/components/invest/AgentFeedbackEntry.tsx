import { cn } from '@/lib/utils';
import type { EvidenceQuality } from './invest-types';
import { EvidenceQualityBadge } from './EvidenceQualityBadge';

interface AgentFeedbackEntryProps {
  agentName: string;
  pass: boolean;
  reasoning: string;
  evidenceQuality: EvidenceQuality;
  isOutlier?: boolean;
  timestamp?: string;
}

export function AgentFeedbackEntry({
  agentName,
  pass,
  reasoning,
  evidenceQuality,
  isOutlier = false,
  timestamp,
}: AgentFeedbackEntryProps) {
  return (
    <div
      className={cn(
        'rounded-md border bg-card px-3 py-2.5',
        isOutlier && 'border-l-[3px] border-l-amber-400',
      )}
    >
      <div className="flex items-center gap-2 mb-1">
        <span className="text-sm font-semibold text-foreground">{agentName}</span>
        <span
          className={cn(
            'inline-block px-1.5 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider',
            pass
              ? 'bg-emerald-100 text-emerald-700'
              : 'bg-red-100 text-red-600',
          )}
        >
          {pass ? 'Pass' : 'Fail'}
        </span>
        <span className="flex-1" />
        <EvidenceQualityBadge quality={evidenceQuality} variant="inline" />
        {timestamp && (
          <span className="text-[10px] text-muted-foreground">{timestamp}</span>
        )}
      </div>
      <p className="text-sm text-foreground/80">{reasoning}</p>
    </div>
  );
}
